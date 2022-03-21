// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Future
{
    /// <summary>
    /// Set of extensions for the Pivot control.
    /// </summary>
    [Bindable]
    public partial class PivotExtensions
    {
        private static void InitPivotStyle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pivot = d as Windows.UI.Xaml.Controls.Pivot;

            if (pivot == null)
            {
                return;
            }

            pivot.Loaded -= Pivot_Loaded;
            pivot.Loaded += Pivot_Loaded;
        }

        private static void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            var pivot = sender as Windows.UI.Xaml.Controls.Pivot;

            // Make sure we find the PivotHeaderPanels for just this Pivot (in case we have embedded pivots)
            var panels = pivot.FindDescendants().OfType<PivotHeaderPanel>().Where(panel => panel.FindAscendant<Windows.UI.Xaml.Controls.Pivot>() == pivot);

            var style = GetPivotHeaderItemStyle(pivot);

            foreach (var panel in panels)
            {
                foreach (var child in panel.Children)
                {
                    var phi = child as PivotHeaderItem;
                    phi.Style = style;
                }
            }

            // Listen to Pivot's Collection changes as better exposed than internal PivotHeaderPanel
            // We need to do a delegate here so we don't have to save a reference to the Pivots, as
            // otherwise the callback doesn't have a way to get to the Pivot/PivotHeaderItems we need.
            VectorChangedEventHandler<object> collectionHandler = (Windows.Foundation.Collections.IObservableVector<object> collection, Windows.Foundation.Collections.IVectorChangedEventArgs @event) =>
            {
                // Need to listen to new collection changes and make sure we catch new PivotHeaderItems to update their styles.
                foreach (var panel in panels)
                {
                    if (@event.CollectionChange == Windows.Foundation.Collections.CollectionChange.ItemInserted
                        && panel.Children.Count > @event.Index)
                    {
                        var phi = panel.Children[(int)@event.Index] as PivotHeaderItem;
                        phi.Style = style;
                    }
                }
            };

            pivot.Items.VectorChanged -= collectionHandler;
            pivot.Items.VectorChanged += collectionHandler;

            SetContentVisible(pivot, GetIsContentVisible(pivot));

            pivot.Loaded -= Pivot_Loaded;
        }

        private static void IsContentVisible_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pivot = d as Windows.UI.Xaml.Controls.Pivot;

            pivot.Loaded -= Pivot_Loaded;
            pivot.Loaded += Pivot_Loaded;

            if (e.NewValue != null)
            {
                SetContentVisible(pivot, (bool)e.NewValue);
            }
        }

        private static void SetContentVisible(Windows.UI.Xaml.Controls.Pivot pivot, bool value)
        {
            // Make sure we find the PivotHeaderPanels for just this Pivot (in case we have embedded pivots)
            var presenter = pivot.FindDescendant("PivotItemPresenter");

            if (presenter != null)
            {
                presenter.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
