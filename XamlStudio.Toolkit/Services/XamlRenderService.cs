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
    public class XamlRenderService
    {
        /// <summary>
        /// Unique Id for this Service.
        /// </summary>
        public int Id { get; } = IdGenerator.Next();

        /// <summary>
        /// StorageFolder root folder to look for images and d:DesignData files from.
        /// </summary>
        public StorageFolder ResourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the setting for enabling binding debugger.
        /// </summary>
        public bool IsBindingDebuggingEnabled { get; set; }

        /// <summary>
        /// Gets the list of Bindings found when <see cref="IsBindingDebuggingEnabled"/> is turned on.  Cleared and Populated after a call to Render.
        /// </summary>
        public IEnumerable<XamlBindingInfo> Bindings { get { return XamlBindingWrapperManager.Instance.GetBindings(Id); } }

        /// <summary>
        /// Set the explicit DataContext used on the root UIElement.
        /// </summary>
        public object DataContext { get; set; }
        
        public XamlRenderService()
        {
            XamlBindingWrapperManager.Instance.Register(this.Id, this);
        }

        public async Task<XamlRenderResultContext> Render(string content)
        {
            var result = new XamlRenderResultContext() { Content = content };
            XamlBindingWrapperManager.Instance.Clear(this.Id);  // Remove previous Binding Tracking

            /*if (LoadedAssemblies == null)
            {
                await LoadAssembliesAsync();
            }*/

            // TODO: Need to be better at being non-destructive of original content when passing to different parsers which need to reference the original content.

            // Pre-parse            
            if (!content.Contains("xmlns")) // TODO: add flag about using pre-parsing or not.
            {
                // TODO: Should use Regex to skip over initial comments and ?xml and such.
                // Find the end of the first tag // TODO: Support single tagged content only as well '/>'
                var oti = content.IndexOf(">");
                if (oti != -1)
                {
                    content = content.Substring(0, oti) + @" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""" + content.Substring(oti);
                }
            }

            // TODO: have 'common' namespace list and inject needed namespaces found in document.  Should warn and provide way for user in editor to add in so they can copy-paste

            // TODO: Decide if we have option to save 'help' back to Document

            // TODO: Record Line, Start, and Length of Changes to re-adjust error messages back to original positions.
            if (IsBindingDebuggingEnabled)
            {
                content = InterceptBindings(content);
            }

            // Attempt Render
            UIElement element = null;
            try
            {
                var obj = XamlReader.LoadWithInitialTemplateValidation(content); // TODO: Add Flag to change which function to use.
                if (!(obj is UIElement))
                {
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
                    fwe.DataContext = this.DataContext == null ? element : this.DataContext;
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

                    if (root.Attributes.GetNamedItem("d:DataContext") is XmlAttribute ddatacontext && ResourceRoot != null)
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
                                    var data = await LoadDataSource(ResourceRoot, source);
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
                    if (IsBindingDebuggingEnabled)
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
                if (ResourceRoot != null)
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

                                var imagefile = await GetFileFromPath(ResourceRoot, uri);
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

        private const string InitialElementPattern = "<(?<Type>\\w+)";
        private const string BindingSearcherPattern = "([\"']){\\s*(?<Type>(?:Binding)|(?:x:Bind)).*?}\\1"; // \1 matches initial single or double quote used in first capturing group.
        private const string BindingPropertiesPattern = "((?<Property>(?:BindBack)|(?:Converter)|(?:ConverterLanguage)|(?:ConverterParameter)|(?:ElementName)|(?:FallbackValue)|(?:Mode)|(?:Path)|(?:RelativeSource)|(?:Source)|(?:TargetNullValue)|(?:UpdateSourceTrigger))\\s*=\\s*(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}])))))+";
        private static Regex InitialElementSearcher = new Regex(InitialElementPattern, RegexOptions.Compiled);
        private static Regex BindingSearcher = new Regex(BindingSearcherPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex BindingPropertyExtractor = new Regex(BindingPropertiesPattern, RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Replace Binding Expressions with equivalents but intercepted by our own converter for additional logic/redirection.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string InterceptBindings(string content)
        {
            // Need to inject Converter Resource into FrameworkElement
            Match type = InitialElementSearcher.Match(content);
            if (type.Success)
            {
                var typename = type.Groups["Type"]?.Value;

                if (!IsFrameworkElement(typename))
                {
                    // We can't inject resources (like our binding wrapper) into non-framework elements.
                    return content;
                }

                // Need 'x' namespace for resource key in our converter wrapper...
                if (!content.Contains("xmlns:x"))
                {
                    // Find the end of the first tag
                    var oti = content.IndexOf(">");
                    if (oti != -1)
                    {
                        content = content.Substring(0, oti) + @" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""" + content.Substring(oti);
                    }
                }

                // Inject our extension namespace (if needed), so we have access to our Binding Wrapper.
                if (!content.Contains("xmlns:xstc="))
                {
                    // Find the end of the first tag
                    var oti = content.IndexOf(">");
                    if (oti != -1)
                    {
                        content = content.Substring(0, oti) + @" xmlns:xstc=""using:XamlStudio.Toolkit.Converters""" + content.Substring(oti);
                    }
                }

                var resourceSearch = "<" + typename + ".Resources>";
                const string converter = "<xstc:XamlBindingWrapperConverter x:Key=\"XamlBindingWrapper\"/>";

                if (content.IndexOf(resourceSearch) != -1)
                {
                    content = content.Replace(resourceSearch, resourceSearch + converter);
                }
                else
                {
                    // If we don't have an existing resource section, add one right after our initial type tag.
                    var oti = content.IndexOf(">");
                    if (oti != -1)
                    {
                        content = content.Substring(0, oti + 1) + resourceSearch + converter + "</" + typename + ".Resources>" + content.Substring(oti + 1);
                    }
                }
            }

            foreach (Match binding in BindingSearcher.Matches(content))
            {
                var isXBind = binding.Groups["Type"]?.Value == "x:Bind";
                var quoteChar = binding.Value[0]; // Grab the ' or " char surrounding our binding expression.

                var original = binding.Value;

                // Calculate Editor Based Position // TODO: Make sure we're not out of line with earlier modification steps
                uint line = 1 + (uint)content.Substring(0, binding.Index).Count(c => c == '\n');
                var position = binding.Index - content.LastIndexOf('\n', binding.Index);

                var bindingInfo = new Models.XamlBindingInfo(line, (uint)position, original);

                XamlBindingWrapperManager.Instance.AddNewBinding(this.Id, bindingInfo);

                const string newBinding = "{StaticResource XamlBindingWrapper}";
                var foundConverter = false;
                var foundConverterParameter = false;

                // Copy of ongoing permutations to original binding string holder
                var newbindingstr = string.Empty + original;

                foreach (Match property in BindingPropertyExtractor.Matches(binding.Value))
                {
                    if (property.Groups["Property"]?.Value == "Converter")
                    {
                        foundConverter = true;

                        var value = property.Groups["Value"].Value;
                        var space = value.IndexOf(" ");

                        var converterkey = value.Substring(space + 1, value.Length - space - 2);
                        
                        bindingInfo.ConverterKey = converterkey;

                        // Replace converter with our new one
                        var str = newbindingstr.Replace(property.Groups["Value"].Value, newBinding);

                        // Inject back to original string
                        content = content.Replace(newbindingstr, str);

                        newbindingstr = str;
                    }
                    else if (property.Groups["Property"]?.Value == "ConverterParameter")
                    {
                        foundConverterParameter = true;

                        // TODO: Retrieve original converter parameter if resource??? (Probably have to do same as Converter, not sure how common/capabilties
                        bindingInfo.ConverterParameter = property.Groups["Value"].Value;

                        // Our new converter parameter is 'Id{Binding ...}'
                        var str = newbindingstr.Replace(property.Groups["Value"].Value, string.Empty + bindingInfo.Id);

                        // Inject back to original string
                        content = content.Replace(newbindingstr, str);

                        newbindingstr = str;
                    }
                }

                if (!foundConverter)
                {
                    // TODO: BUGBUG need to remember changes to string above too, as don't know if we had a parameter without a converter (odd?)
                    // If no converter on binding, add ours
                    var str = binding.Value.Substring(0, binding.Value.Length - 2) + ",Converter=" + newBinding + "}" + quoteChar;
                    
                    content = content.Replace(newbindingstr, str);

                    newbindingstr = str;
                }

                if (!foundConverterParameter)
                {
                    // If no converterparameter on binding, add ours
                    content = content.Replace(newbindingstr, newbindingstr.Substring(0, newbindingstr.Length - 2) + ",ConverterParameter=" + bindingInfo.Id + "}" + quoteChar);
                }
            }

            return content;
        }

        private static bool IsFrameworkElement(string typename)
        {
            // Look for other UI controls in Main Assembly.
            var foundtype = typeof(FrameworkElement).GetTypeInfo().Assembly.GetTypes().FirstOrDefault(type => type.Name == typename);

            // TODO: Look at other assemblies for custom framemwork element types of other libs?

            if (foundtype == null)
            {
                return false;
            }

            return typeof(FrameworkElement).GetTypeInfo().IsAssignableFrom(foundtype.GetTypeInfo());
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
