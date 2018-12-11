using System;
using System.Linq;
using System.Threading.Tasks;
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

            // Load extra Metadata about other available types.
            if (!AppAssemblyInfo.Instance.IsLoaded)
            {
                await AppAssemblyInfo.Instance.InitializeAsync();
            }

            // If we're doing Binding Debugging, we have some required prefixes, so make sure we have them.
            if (settings.IsBindingDebuggingEnabled)
            {
                // TODO: Feel like this should be in PreProcessXmlns
                // also should be added to RenderedContent but not Suggested...
                settings.KnownNamespaces.Add(new XmlnsNamespace("x", XmlnsPathX));
                settings.KnownNamespaces.Add(new XmlnsNamespace(XmlnsPrefixXstc, XmlnsPathXstc));
            }

            // Start by pre-processing raw string to add any missing namespaces.
            PreProcessXmlns(ref result, ref settings);

            ReadXmlTree(ref result);

            if (settings.IsBindingDebuggingEnabled)
            {    
                GetBindings(result);

                // TODO: Record Line, Start, and Length of Changes to re-adjust error messages back to original positions.
                // TODO: Do this in XML (add required resources)
                InterceptBindings(ref result);
            }

            // Attempt RenderAsync
            try
            {
                if (settings.IsInitialTemplateValidated)
                {
                    result.Element = XamlReader.LoadWithInitialTemplateValidation(result.RenderedContent);
                }
                else
                {
                    result.Element = XamlReader.Load(result.RenderedContent);
                }
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

                var lineContent = GetLine(result.RenderedContent, line);
                result.Errors.Add(new XamlExceptionRange(msg, e, line, column, lineContent));
            }

            // Need to look for Design-Time 'd:' properties and link to object somehow for modification afterwards as they're ignored by parser usually with mc:Ignorable="d"
            await ProcessDesignDataAsync(result, settings);

            // Load Binding Converters
            if (result.Element is FrameworkElement fwe)
            {
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

            if (result.Element != null && result.IsUIElement)
            {
                if (settings.ResourceRoot != null)
                {
                    // Look for Image Objects in order to replace their Sources with our Images Loaded from Disk.
                    VisitUIElements(result.Element as UIElement, async (child) =>
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

                result.Bindings = XamlBindingWrapperManager.Instance.GetBindings(Id);
            }
            else
            {
                result.Bindings = Enumerable.Empty<XamlBindingInfo>();
            }

            return result;
        }

        /// <summary>
        /// Get the text at a specific line.
        /// Returns an Empty string if the lineNumber is out of range.
        /// </summary>
        /// <param name="content">The content to inspect</param>
        /// <param name="lineNumber">Target line number</param>
        /// <returns></returns>
        private string GetLine(string content, uint lineNumber)
        {
            if (lineNumber < 1 || string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var lines = content.Split("\n");

            if (lineNumber > lines.Count() + 1)
            {
                return string.Empty;
            }

            return lines[lineNumber - 1];
        }
    }
}
