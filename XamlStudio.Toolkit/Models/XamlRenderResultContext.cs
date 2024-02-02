using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
        /// Gets a value indicating if the content has a suggestion.
        /// </summary>
        public bool HasSuggestion { get; internal set; }

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
        public object Element { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="Element"/> from pre-parsed lookup.
        /// </summary>
        public Type ElementType { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="Element"/> after a successful load.
        /// </summary>
        public Type ElementActualType => Element?.GetType();

        /// <summary>
        /// Gets the Width requested by the <c>d:DesignWidth</c> attribute, if set.
        /// </summary>
        public double? RequestedWidth { get; set; }

        /// <summary>
        /// Gets the Height requested by the <c>d:DesignHeight</c> attribute, if set.
        /// </summary>
        public double? RequestedHeight { get; set; }

        /// <summary>
        /// Gets a value indicating if the Element is derived from <see cref="UIElement"/> and can be displayed.
        /// </summary>
        public bool IsUIElement => Element is UIElement;

        /// <summary>
        /// Gets a value indicating if the Element is derived from <see cref="ResourceDictionary"/> and is a resource library.
        /// </summary>
        public bool IsResourceDictionary => Element is ResourceDictionary;

        /// <summary>
        /// Gets a value indicating if the Element is derived from <see cref="FrameworkElement"/> as determined during pre-parsing.
        /// </summary>
        public bool IsFrameworkElement { get; internal set; }

        /// <summary>
        /// <see cref="XmlDocumentSyntax"/> New XML document result from <see cref="Parser.ParseText(string)"/> from GuiLabs.
        /// </summary>
        public XmlDocumentSyntax XmlDocument { get; internal set; }

        /// <summary>
        /// <see cref="XDocument"/> representing <see cref="RenderedContent"/>.
        /// </summary>
        public XDocument Document { get; internal set; } // TODO: Should move away from?

        /// <summary>
        /// Gets the DataContext provided to the <see cref="Services.XamlRenderService"/> or loaded from d:DesignData.
        /// </summary>
        public object DataContext { get; internal set; }

        /// <summary>
        /// Gets the DataContext source file path (from the <see cref="XamlRenderSettings.ResourceRoot"/>) if loaded from d:DesignData.
        /// </summary>
        public string DataContextSource { get; internal set; }

        /// <summary>
        /// Gets the list of Errors found if <see cref="Services.XamlRenderService.RenderAsync(string)"/> was unsuccessful.
        /// </summary>
        public IList<XamlExceptionRange> Errors { get; internal set; } = new List<XamlExceptionRange>();

        /// <summary>
        /// Gets the list of Bindings found when <see cref="IsBindingDebuggingEnabled"/> is turned on.  Cleared and Populated after a call to <see cref="Services.XamlRenderService.RenderAsync(string)"/>.
        /// </summary>
        public IEnumerable<XamlBindingInfo> Bindings { get; internal set; }

        /// <summary>
        /// Gets the list of known Namespaces found on the document.
        /// </summary>
        public XmlnsNamespace[] DetectedNamespaces { get; internal set; }

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
