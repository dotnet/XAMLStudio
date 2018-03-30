using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml;

namespace XamlStudio.Toolkit.Models
{
    public class XamlRenderResultContext
    {
        public string Content { get; internal set; }

        public UIElement Element { get; internal set; }

        public XmlDocument Document { get; internal set; }

        public object DataContext { get; internal set; }

        public IList<XamlExceptionRange> Errors { get; internal set; } = new List<XamlExceptionRange>();

        /// <summary>
        /// Gets the list of Bindings found when <see cref="IsBindingDebuggingEnabled"/> is turned on.  Cleared and Populated after a call to Render.
        /// </summary>
        public IEnumerable<XamlBindingInfo> Bindings { get; internal set; }
    }
}
