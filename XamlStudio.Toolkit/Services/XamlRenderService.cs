using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    /// <summary>
    /// Class to assist in parsing a Xaml string and returning an UIElement.
    /// 
    /// Wrapper around XamlReader.Load* with extra pre/post processing to support more features like loading images from an external source.
    /// 
    /// References:
    ///     https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.markup.xamlreader
    ///     https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-namespaces-and-namespace-mapping
    ///     https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-in-depth
    ///     https://blogs.msdn.microsoft.com/mcsuksoldev/2010/08/27/designdata-mvvm-support-in-blend-vs2010-and-wpfsilverlight/
    /// </summary>
    public partial class XamlRenderService
    {        
        public XamlRenderService()
        {
            XamlBindingWrapperManager.Instance.Register(this.Id, this);
        }

        public async Task<XamlRenderResultContext> RenderAsync(string content, XamlRenderSettings settings = null)
        {
            // Use default settings if none provided.
            if (settings == null)
            {
                settings = new XamlRenderSettings();
            }

            // Remove previous Binding Tracking
            XamlBindingWrapperManager.Instance.Clear(this.Id);

            // Hold all outcomes of this process in an object we'll return when done.
            var result = new XamlRenderResultContext(content);

            // If we're doing Binding Debugging, we have some required prefixes, so make sure we have them.
            if (settings.IsBindingDebuggingEnabled)
            {
                // TODO: Feel like this should be in PreProcessXmlns
                // also should be added to RenderedContent but not Suggested...
                settings.KnownNamespaces["x"] = XmlnsPathX;
                settings.KnownNamespaces[XmlnsPrefixXstc] = XmlnsPathXstc;
            }

            // Start by pre-processing raw string to add any missing namespaces.
            PreProcessXmlns(ref result, ref settings);

            /*if (LoadedAssemblies == null)
            {
                await LoadAssembliesAsync();
            }*/

            // TODO: Need to be better at being non-destructive of original content when passing to different parsers which need to reference the original content.


            // TODO: have 'common' namespace list and inject needed namespaces found in document.  Should warn and provide way for user in editor to add in so they can copy-paste

            // TODO: Decide if we have option to save 'help' back to Document

            // TODO: Record Line, Start, and Length of Changes to re-adjust error messages back to original positions.
            if (settings.IsBindingDebuggingEnabled)
            {
                InterceptBindings(ref result);
            }

            // Attempt RenderAsync
            UIElement element = null;
            try
            {
                object obj = null;
                if (settings.IsInitialTemplateValidated)
                {
                    obj = XamlReader.LoadWithInitialTemplateValidation(result.RenderedContent);
                }
                else
                {
                    obj = XamlReader.Load(result.RenderedContent);
                }

                if (!(obj is UIElement))
                {
                    // TODO: ResourceDictionaries should be loadable, but they're just DependencyObject
                    // Investigate for future usages.
                    throw new NotSupportedException("Content must be a UIElement.");
                }
                element = obj as UIElement;
            }
            catch (Exception e)
            {
                // Highlight Error (we'll only get one at a time).
                string msg = e.Message;

                msg = msg.Replace("The text associated with this error code could not be found.", "").Trim();

                uint line = 1;
                uint column = 1;

                //No default namespace has been declared. [Line: 1 Position: 2]
                int il = msg.IndexOf("Line: ");
                if (il >= 0)
                {
                    line = uint.Parse(msg.Substring(il + 6, msg.IndexOf("P", il) - il - 7));
                }

                int pl = msg.IndexOf("Position: ");
                if (pl >= 0)
                {
                    column = uint.Parse(msg.Substring(pl + 9, msg.IndexOf("]", pl) - pl - 9));
                }

                result.Errors.Add(new XamlExceptionRange(msg, e, line, column, line, column + 8)); // TODO: Inspect Content at this position and go until space / EOL
            }

            // Need to look for Design-Time 'd:' properties and link to object somehow for modification afterwards as they're ignored by parser usually with mc:Ignorable="d"
            XmlDocument xaml = new XmlDocument();
            try
            {
                xaml.LoadXml(content);
                result.Document = xaml;
            }
            catch (Exception e)
            {
                // Highlight Error (we'll only get one at a time).
                string msg = e.Message;

                uint line = 1;
                uint column = 1;

                // message. Line 9, position 37.
                int il = msg.LastIndexOf("Line ");
                if (il >= 0)
                {
                    line = uint.Parse(msg.Substring(il + 5, msg.IndexOf(",", il) - il - 5));
                }

                int pl = msg.LastIndexOf("position ");
                if (pl >= 0)
                {
                    column = uint.Parse(msg.Substring(pl + 8, msg.IndexOf(".", pl) - pl - 8));
                }

                result.Errors.Add(new XamlExceptionRange(msg, e, line, column, line, column + 8)); // TODO: Inspect Content at this position and go until space / EOL
            }

            if (xaml.ChildNodes.Count > 0)
            {
                var root = xaml.ChildNodes.Item(0);

                // Set DataContext to root element or to provided DataContext (if it exists).
                // May get overwritten by d:DesignData loading later.
                if (element is FrameworkElement fwe)
                {
                    fwe.DataContext = settings.DataContext == null ? element : settings.DataContext;
                    result.DataContext = fwe.DataContext;

                    if (root.Attributes.GetNamedItem("d:DesignWidth") is XmlAttribute dwidth)
                    {
                        if (int.TryParse(dwidth.Value, out int width))
                        {
                            fwe.Width = width;
                        }
                    }

                    if (root.Attributes.GetNamedItem("d:DesignHeight") is XmlAttribute dheight)
                    {
                        if (int.TryParse(dheight.Value, out int height))
                        {
                            fwe.Height = height;
                        }
                    }

                    if (root.Attributes.GetNamedItem("d:DataContext") is XmlAttribute ddatacontext && settings.ResourceRoot != null)
                    {
                        var dc = ddatacontext.Value;
                        var ddi = dc.IndexOf("d:DesignData");
                        if (!String.IsNullOrWhiteSpace(dc) && ddi != -1)
                        {
                            // Grab inner d:DesignData Clause
                            var dd = dc.Substring(ddi, dc.IndexOf("}", ddi) - ddi);

                            // Grab source
                            var si = dd.IndexOf("Source");
                            if (si != -1)
                            {
                                var ei = dd.IndexOf(","); // Next Argument
                                if (ei == -1)
                                {
                                    ei = dd.IndexOf("}"); // Or End of Bind
                                }

                                if (ei != -1)
                                {
                                    var source = dd.Substring(si + 6, ei - si - 6).Trim('=', ' ');
                                    var data = await LoadDataSource(settings.ResourceRoot, source);
                                    if (data != null && element is FrameworkElement)
                                    {
                                        (element as FrameworkElement).DataContext = data;
                                        result.DataContext = data;
                                    }
                                }
                            }
                        }
                    }

                    // Load Binding Converters
                    if (settings.IsBindingDebuggingEnabled)
                    {
                        foreach (var binding in XamlBindingWrapperManager.Instance.GetBindings(this.Id))
                        {
                            if (!string.IsNullOrWhiteSpace(binding.ConverterKey) && fwe.Resources.ContainsKey(binding.ConverterKey))
                            {
                                binding.Converter = fwe.Resources[binding.ConverterKey] as IValueConverter;
                            }
                            // If Key not found, should already be Xaml Compiler Error and not get here.
                        }
                    }
                }
            }

            if (element != null)
            {
                if (settings.ResourceRoot != null)
                {
                    // Look for Image Objects in order to replace their Sources with our Images Loaded from Disk.
                    Visit(element, async (child) =>
                    {
                        // TODO: Generalize to support toolkit:ImageEx, toolkit:RoundImageEx, Converters?
                        if (child is Image)
                        {
                            var img = child as Image;
                            var uri = (img.Source as BitmapImage)?.UriSource?.AbsoluteUri;
                            if (uri != null)
                            {
                                if (uri.StartsWith("ms-appx:///", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    uri = uri.Substring(11);
                                }
                                else if (uri.StartsWith("ms-resource:///Files/", StringComparison.OrdinalIgnoreCase))
                                {
                                    uri = uri.Substring(21);
                                }

                                var imagefile = await GetFileFromPath(settings.ResourceRoot, uri);
                                var bitmapImage = new BitmapImage();
                                if (imagefile != null)
                                {
                                    using (var stream = await (imagefile as StorageFile).OpenAsync(FileAccessMode.Read))
                                    {
                                        await bitmapImage.SetSourceAsync(stream);
                                    }

                                    // Replace Image Source with our now injected image.
                                    img.Source = bitmapImage;
                                }
                            }
                        }
                    });
                }

                result.Bindings = XamlBindingWrapperManager.Instance.GetBindings(this.Id);
                result.Element = element;
            }

            return result;
        }

        /*private static List<Assembly> LoadedAssemblies { get; set; }

        private static async Task LoadAssembliesAsync()
        {
            LoadedAssemblies = new List<Assembly>();

            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    LoadedAssemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }*/

        /// <summary>
        /// Given a StorageFolder and path loads a Data file to load as an object.
        /// </summary>
        /// <param name="source">Starting StorageFolder.</param>
        /// <param name="path">string path.</param>
        /// <returns></returns>
        private static async Task<object> LoadDataSource(StorageFolder source, string path)
        {
            var file = await GetFileFromPath(source, path);

            dynamic data = null;

            if (file != null)
            {
                var content = await FileIO.ReadTextAsync(file);

                // TODO: Do I just add errors in deserializing to the Errors bucket as well?
                // Do I have a separate Error Bucket?
                switch (file.FileType.ToLower())
                {
                    case ".json":
                        try
                        {
                            data = JsonConvert.DeserializeObject<ExpandoObject>(content);
                        }
                        catch (Exception e)
                        {

                        }
                        break;
                    case ".xml":
                        // TODO
                        break;  
                }
            }

            return data;
        }

        /// <summary>
        /// Given a path, e.g. "Images\Owl.jpg", navigates the structure from StorageFolder/Files.
        /// </summary>
        /// <param name="root">Starting StorageFolder.</param>
        /// <param name="path">string path.</param>
        /// <returns></returns>
        private static async Task<StorageFile> GetFileFromPath(StorageFolder root, string path)
        {
            // Flip path around and try again.
            if (path.Contains("\\"))
            {
                return await GetFileFromPath(root, path.Replace("\\", "/"));
            }

            // Trim start if we have an absolute marker, as we'll still be going from the same place.
            // TODO: Do we need to do this outside here?
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            // If we have a subfolder path, we need to navigate into folder first.
            if (path.Contains("/"))
            {
                var folder = path.Substring(0, path.IndexOf("/"));
                if (await root.TryGetItemAsync(folder) is StorageFolder newroot)
                {
                    // Call again with subfolder and truncated path.
                    return await GetFileFromPath(newroot, path.Substring(folder.Length + 1));
                }

                // Couldn't find sub-folder.
                return null;
            }

            // Return file if it exists.
            return await root.TryGetItemAsync(path) as StorageFile;
        }

        /// <summary>
        /// Visits each element and child of the given element and passes them to the given action.
        /// </summary>
        /// <param name="element">Root element to start from.</param>
        /// <param name="func">Function to execute.</param>
        private static void Visit(UIElement element, Action<UIElement> func)
        {
            // Visit element
            func(element);

            if (element is Panel)
            {
                foreach (var child in (element as Panel).Children)
                {
                    // Visit each child in the panel
                    Visit(child, func);
                }
            }
            else if (element != null)
            {
                // Use ContentProperty Attribute to figure out which property we should look for as the 'Content' for this control
                var contentpropname = ContentPropertySearch(element.GetType());
                //var attr = element.GetType().GetTypeInfo().GetCustomAttribute(typeof(ContentPropertyAttribute), true) as ContentPropertyAttribute;
                if (contentpropname != null)
                {
                    if (element.GetType().GetProperty(contentpropname).GetValue(element) is UIElement child)
                    {
                        Visit(child, func);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the Content Property's Name for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string ContentPropertySearch(Type type)
        {
            if (type == null)
            {
                return null;
            }

            // Using GetCustomAttribute directly isn't working for some reason, so we'll dig in ourselves
            //var attr = type.GetTypeInfo().GetCustomAttribute(typeof(ContentPropertyAttribute), true);
            var attr = type.GetTypeInfo().CustomAttributes.FirstOrDefault((element) => element.AttributeType == typeof(ContentPropertyAttribute));
            if (attr != null)
            {
                //return attr as ContentPropertyAttribute;
                return attr.NamedArguments.First().TypedValue.Value as string;
            }

            return ContentPropertySearch(type.GetTypeInfo().BaseType);
        }
    }
}
