using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Toolkit.Models
{
    /// <summary>
    /// Storage for settings passed into <see cref="Services.XamlRenderService.RenderAsync(string)"/>.
    /// </summary>
    public class XamlRenderSettings
    {
        /// <summary>
        /// Gets the dictionary of known namespaces to automatically try and add if missing in given content to Render.
        /// </summary>
        public Dictionary<string, string> KnownNamespaces { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// StorageFolder root folder to look for images and d:DesignData files from.
        /// </summary>
        public StorageFolder ResourceRoot { get; set; }

        /// <summary>
        /// Gets or sets the setting for enabling binding debugger.
        /// </summary>
        public bool IsBindingDebuggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a setting which indicates if the <see cref="XamlRenderResultContext.SuggestedContent"/>
        /// should stay the same length as <see cref="XamlRenderResultContext.Content"/> so that
        /// Error line numbers will map to the original Content line numbers.
        /// Otherwise, injected content will be 'prettified' and line numbers will map to this SuggestedContent instead.
        /// Defaults to true.
        /// </summary>
        public bool KeepSuggestedContentSameLength { get; set; } = true;

        /// <summary>
        /// Gets or sets the setting which determines if <see cref="Windows.UI.Xaml.Markup.XamlReader.LoadWithInitialTemplateValidation(string)"/> or <see cref="Windows.UI.Xaml.Markup.XamlReader.Load(string)"/> method should be used to render content.
        /// Defaults to use InitialTemplateValidation.
        /// </summary>
        public bool IsInitialTemplateValidated { get; set; } = true;

        /// <summary>
        /// Set the explicit DataContext used on the root UIElement.
        /// </summary>
        public object DataContext { get; set; }
    }
}
