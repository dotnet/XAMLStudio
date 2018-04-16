using System;
using System.Xml;
using Windows.Storage;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    //// Helpers for processing XmlDocument in sync with Xaml tree.

    public partial class XamlRenderService
    {
        private void ReadXmlTree(ref XamlRenderResultContext context)
        {
            XmlDocument xaml = new XmlDocument();
            try
            {
                xaml.LoadXml(context.RenderedContent);
                context.Document = xaml;
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
    }
}
