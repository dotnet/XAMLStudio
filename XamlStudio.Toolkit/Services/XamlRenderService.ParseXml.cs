using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;

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

                var lineContent = GetLine(context.RenderedContent, line);
                context.Errors.Add(new XamlExceptionRange(msg, e, line, column, lineContent));
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
                            var bindingInfo = new XamlBindingInfo((uint)((IXmlLineInfo)attr).LineNumber, (uint)(((IXmlLineInfo)attr).LinePosition + attr.Name.LocalName.Length + 2), attr.Value)
                            {
                                PropertyAttribute = attr,
                                PropertyName = attr.Name.LocalName,
                                ElementTypeName = attr.Parent.Name.LocalName,
                                ElementName = attr.Parent.Attributes().GetNamedItem("{http://schemas.microsoft.com/winfx/2006/xaml}Name")?.Value
                            };

                            XamlBindingWrapperManager.Instance.AddNewBinding(Id, bindingInfo);

                            var bt = BindingParser.Parse(attr.Value);

                            attr.Value = InjectBindingConverter(attr.Value, bt, bindingInfo);
                        }
                    }
                });

                // TODO: Location?
                StringWriter sw = new StringWriter();

                context.Document.Save(sw, SaveOptions.DisableFormatting); // TODO: Bug #587

                context.RenderedContent = sw.ToString();

                // Remove xml <?xml version="1.0" encoding="utf-16"?>
                context.RenderedContent = context.RenderedContent.Substring(context.RenderedContent.IndexOf("<", 1));
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
