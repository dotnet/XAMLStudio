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
        private readonly string _content;
        /// <summary>
        /// Original Content passed in to the <see cref="Services.XamlRenderService.RenderAsync(string)"/> call.
        /// </summary>
        public string Content => _content;

        /// <summary>
        /// Content modified by pre-parser helpers with any missing xmlns elements.
        /// </summary>
        public string SuggestedContent { get; internal set; }

        /// <summary>
        /// Content that represents the actually rendered <see cref="Element"/>.
        /// This can be different from <see cref="Content"/> due to different settings and helpers
        /// which add missing elements or inject helpers for images, data context, and binding debugging.
        /// It is mainly intended for debugging the <see cref="Services.XamlRenderService"/> itself.
        /// </summary>
        public string RenderedContent { get; internal set; }

        /// <summary>
        /// Element rendered by <see cref="Services.XamlRenderService.RenderAsync(string)"/> or null if rendering was unsuccessful.
        /// </summary>
        public UIElement Element { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="Element"/> rendered.
        /// </summary>
        public Type ElementType { get; internal set; }

        /// <summary>
        /// Gets a value indiciating if the Element is derived from <see cref="FrameworkElement"/>.
        /// </summary>
        public bool IsFrameworkElement { get; internal set; }

        /// <summary>
        /// <see cref="XmlDocument"/> representing <see cref="RenderedContent"/>.
        /// </summary>
        public XmlDocument Document { get; internal set; }

        /// <summary>
        /// Gets the DataContext provided to the <see cref="Services.XamlRenderService"/> or loaded from d:DesignData.
        /// </summary>
        public object DataContext { get; internal set; }

        /// <summary>
        /// Gets the list of Errors found if <see cref="Services.XamlRenderService.RenderAsync(string)"/> was unsuccessful.
        /// </summary>
        public IList<XamlExceptionRange> Errors { get; internal set; } = new List<XamlExceptionRange>();

        /// <summary>
        /// Gets the list of Bindings found when <see cref="IsBindingDebuggingEnabled"/> is turned on.  Cleared and Populated after a call to <see cref="Services.XamlRenderService.RenderAsync(string)"/>.
        /// </summary>
        public IEnumerable<XamlBindingInfo> Bindings { get; internal set; }

        /// <summary>
        /// Create a new XamlRenderResultContext for the given initial content.
        /// </summary>
        /// <param name="initialContent">Initial string passed to render.</param>
        public XamlRenderResultContext(string initialContent)
        {
            _content = initialContent;
            SuggestedContent = initialContent;
            RenderedContent = initialContent;
        }
    }
}
