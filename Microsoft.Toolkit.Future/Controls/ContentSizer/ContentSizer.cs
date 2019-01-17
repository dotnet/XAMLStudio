using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Future
{
    [ContentProperty(Name = nameof(Content))]
    public partial class ContentSizer : Control
    {
        // Symbols for GripperBar in Segoe MDL2 Assets
        private const string GripperBarVertical = "\xE784";
        private const string GripperBarHorizontal = "\xE76F";
        private const string GripperDisplayFont = "Segoe MDL2 Assets";

        private const double GripperKeyboardChange = 8.0d;

        public ContentSizer()
        {
            this.DefaultStyleKey = typeof(ContentSizer);

            CreateGripperDisplay();

            KeyUp += ContentSizer_KeyUp;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Unhook registered events
            Loaded -= ContentSizer_Loaded;

            // Register Events
            Loaded += ContentSizer_Loaded;

            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        }

        private void CreateGripperDisplay()
        {
            if (Content == null)
            {
                Content = new TextBlock
                {
                    FontFamily = new FontFamily(GripperDisplayFont),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = GripperForeground,
                    Text = ResizeDirection == ContentResizeDirection.Vertical ? GripperBarVertical : GripperBarHorizontal
                };
            }
        }
    }
}
