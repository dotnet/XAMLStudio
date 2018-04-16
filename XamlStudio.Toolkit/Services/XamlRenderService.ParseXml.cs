using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    //// Helpers for processing XmlDocument in sync with Xaml tree.

    public partial class XamlRenderService
    {
        private void ReadXmlTree(ref XamlRenderResultContext context)
        {
            try
            {
                context.Document = XDocument.Parse(context.RenderedContent, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
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

                context.Errors.Add(new XamlExceptionRange(msg, e, line, column, line, column + 8)); // TODO: Inspect Content at this position and go until space / EOL
            }
        }

        private void GetBindings(XamlRenderResultContext context)
        {
            if (context.Document != null)
            {
                VisitXmlElements(context.Document.Root, (node) =>
                {
                    foreach (var attr in node.Attributes())
                    {
                        if (attr.Value.StartsWith("{Binding"))
                        {
                            // Found Binding
                            var bindingInfo = new Models.XamlBindingInfo((uint)((IXmlLineInfo)attr).LineNumber, (uint)((IXmlLineInfo)attr).LinePosition, attr.Value);

                            XamlBindingWrapperManager.Instance.AddNewBinding(Id, bindingInfo);


                        }
                    }
                });
            }
        }

        /// <summary>
        /// Visits each element and child of the given element and passes them to the given action.
        /// </summary>
        /// <param name="element">Root element to start from.</param>
        /// <param name="func">Function to execute.</param>
        private static void VisitXmlElements(XElement element, Action<XElement> func)
        {
            // Visit element
            func(element);

            foreach (var child in element.Elements())
            {
                VisitXmlElements(child, func);
            }
        }
    }
}
