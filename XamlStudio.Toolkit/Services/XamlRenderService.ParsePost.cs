using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    //// Post-processing of a loaded UIElement from RenderAsync.

    public partial class XamlRenderService
    {
        private async Task ProcessDesignDataAsync(XamlRenderResultContext context, XamlRenderSettings settings)
        {
            var xaml = context.Document;
            if (xaml != null && xaml.Elements().Count() > 0)
            {
                var root = xaml.Root;

                // Set DataContext to root element or to provided DataContext (if it exists).
                // May get overwritten by d:DesignData loading later.
                if (context.Element is FrameworkElement fwe)
                {
                    fwe.DataContext = settings.DataContext == null ? context.Element : settings.DataContext;
                    context.DataContext = fwe.DataContext;

                    var attributes = root.Attributes();

                    if (attributes.GetNamedItem("{http://schemas.microsoft.com/expression/blend/2008}DesignWidth") is XAttribute dwidth)
                    {
                        if (int.TryParse(dwidth.Value, out int width))
                        {
                            fwe.Width = width;
                        }
                    }

                    if (attributes.GetNamedItem("{http://schemas.microsoft.com/expression/blend/2008}DesignHeight") is XAttribute dheight)
                    {
                        if (int.TryParse(dheight.Value, out int height))
                        {
                            fwe.Height = height;
                        }
                    }

                    if (attributes.GetNamedItem("{http://schemas.microsoft.com/expression/blend/2008}DataContext") is XAttribute ddatacontext && settings.ResourceRoot != null)
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
                                    if (data != null && fwe != null)
                                    {
                                        fwe.DataContext = data;
                                        context.DataContext = data;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
