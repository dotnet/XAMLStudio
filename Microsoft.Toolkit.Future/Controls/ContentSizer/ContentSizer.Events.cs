using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Future
{
    public partial class ContentSizer
    {
        private void ContentSizer_Loaded(object sender, RoutedEventArgs e)
        {
            // Adding Grip to Grid Splitter
            if (Content == default(UIElement))
            {
                CreateGripperDisplay();
            }

            if (TargetControl == null)
            {
                TargetControl = this.FindAscendant<FrameworkElement>();
            }
        }

        /// <inheritdoc />
        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            var horizontalChange = e.Delta.Translation.X;
            var verticalChange = e.Delta.Translation.Y;

            if (ResizeDirection == ContentResizeDirection.Vertical)
            {
                if (HorizontalMove(horizontalChange))
                {
                    return;
                }
            }
            else if (ResizeDirection == ContentResizeDirection.Horizontal)
            {
                if (VerticalMove(verticalChange))
                {
                    return;
                }
            }

            base.OnManipulationDelta(e);
        }

        private bool VerticalMove(double verticalChange)
        {
            return false;
        }

        private bool HorizontalMove(double horizontalChange)
        {
            if (TargetControl == null)
            {
                return true;
            }

            if (!IsValidWidth(TargetControl, horizontalChange))
            {
                return true;
            }

            TargetControl.Width += horizontalChange;            

            return false;
        }

        private bool IsValidWidth(FrameworkElement target, double horizontalChange)
        {
            var newWidth = target.ActualWidth + horizontalChange;

            var minWidth = target.MinWidth;
            if (!double.IsNaN(minWidth) && newWidth < minWidth)
            {
                return false;
            }

            var maxWidth = target.MaxWidth;
            if (!double.IsNaN(maxWidth) && newWidth > maxWidth)
            {
                return false;
            }

            if (newWidth <= ActualWidth)
            {
                return false;
            }

            return true;
        }
    }
}
