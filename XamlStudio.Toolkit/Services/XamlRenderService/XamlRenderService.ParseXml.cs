using Microsoft.Language.Xml;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Windows.UI.Xaml.Shapes;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;

namespace XamlStudio.Toolkit.Services
{
    //// Helpers for processing XmlDocument in sync with Xaml tree.

    public partial class XamlRenderService
    {
        private (int, int) GetLineColumnIndex(string str, int index)
        {
            var line = 1;
            var column = 1;

            for (var i = 0; i < index; i++)
            {
                if (str[i] == '\n')
                {
                    line++;
                    column = 0;
                }

                column++;
            }

            return (line, column);
        }

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

            //// New XML Document Parsing
            // TODO: Move this to a pre phase before the 'RenderedContent' in order to save off valid bits to enable parsing while typing, can leave error message processing here (should remain to use the original document lines?? Need to figure that bit out...
            context.XmlDocument = Parser.ParseText(context.RenderedContent);

            foreach (var errorNode in context.XmlDocument.DescendantNodesAndSelf().Where((node) => node.ContainsDiagnostics))
            {
                var diagnostic = errorNode.GetDiagnostics().Select(d => d.GetDescription());
                (var line, var column) = GetLineColumnIndex(context.RenderedContent, errorNode.FullSpan.Start);
                context.Errors.Add(new XamlExceptionRange(string.Join('\n', diagnostic), null, (uint)line, (uint)column, GetLine(context.RenderedContent, (uint)line)));
            }
        }

        private void GetBindings(XamlRenderResultContext context, bool isBinding)
        {
            if (context.Document != null)
            {
                VisitXmlElements(context.Document.Root, (node) =>
                {
                    foreach (var attr in node.Attributes())
                    {
                        if (isBinding && attr.Value.StartsWith("{Binding"))
                        {
                            var bt = BindingParser.Parse(attr.Value);

                            // Found Binding
                            var bindingInfo = new XamlBindingInfo((uint)((IXmlLineInfo)attr).LineNumber, (uint)(((IXmlLineInfo)attr).LinePosition + attr.Name.LocalName.Length + 2), attr.Value)
                            {
                                PropertyAttribute = attr,
                                PropertyName = attr.Name.LocalName,
                                ElementTypeName = attr.Parent.Name.LocalName,
                                ElementName = attr.Parent.Attributes().GetNamedItem("{http://schemas.microsoft.com/winfx/2006/xaml}Name")?.Value,
                                ConverterKey = bt.Converter,
                                ConverterParameter = bt.ConverterParameter
                            };

                            XamlBindingWrapperManager.Instance.AddNewBinding(Id, bindingInfo);

                            attr.Value = InjectBindingConverter(attr.Value, bt, bindingInfo);
                        }
                        else if (attr.Name == "{http://schemas.microsoft.com/winfx/2006/xaml}Class")
                        {
                            // Remove x:Class element from xml tree.
                            attr.Remove();
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
