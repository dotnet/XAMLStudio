using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Future
{
    public partial class ContentSizer
    {
        public UIElement Content
        {
            get { return (UIElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Element.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(ContentSizer), new PropertyMetadata(default(UIElement)));

        public CoreCursorType GripperCursor
        {
            get { return (CoreCursorType)GetValue(GripperCursorProperty); }
            set { SetValue(GripperCursorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GripperCursor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GripperCursorProperty =
            DependencyProperty.Register(nameof(GripperCursor), typeof(CoreCursorType), typeof(ContentSizer), new PropertyMetadata(CoreCursorType.SizeWestEast));

        public Brush GripperForeground
        {
            get { return (Brush)GetValue(GripperForegroundProperty); }
            set { SetValue(GripperForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GripperForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GripperForegroundProperty =
            DependencyProperty.Register(nameof(GripperForeground), typeof(Brush), typeof(ContentSizer), new PropertyMetadata(default(Brush)));

        public ContentResizeDirection ResizeDirection
        {
            get { return (ContentResizeDirection)GetValue(ResizeDirectionProperty); }
            set { SetValue(ResizeDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResizeDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizeDirectionProperty =
            DependencyProperty.Register(nameof(ResizeDirection), typeof(ContentResizeDirection), typeof(ContentSizer), new PropertyMetadata(ContentResizeDirection.Vertical));
        
        public FrameworkElement TargetControl
        {
            get { return (FrameworkElement)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetControlProperty =
            DependencyProperty.Register(nameof(TargetControl), typeof(FrameworkElement), typeof(ContentSizer), new PropertyMetadata(null));
    }

    /// <summary>
    /// Enum to indicate whether <see cref="ContentSizer"/> resizes Vertically or Horizontally.
    /// </summary>
    public enum ContentResizeDirection
    {
        /// <summary>
        /// Determines whether to resize rows or columns based on its Alignment and
        /// width compared to height
        /// </summary>
        Auto,

        /// <summary>
        /// Resize columns when dragging Splitter.
        /// </summary>
        Vertical,

        /// <summary>
        /// Resize rows when dragging Splitter.
        /// </summary>
        Horizontal
    }
}
