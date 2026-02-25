// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Future;

/// <summary>
/// Set of extensions for the Pivot control.
/// </summary>
[Bindable]
public partial class PivotExtensions
{
    private static void InitPivotStyle(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        #if UNO
        var pivot = d as Microsoft.UI.Xaml.Controls.Pivot;
        #else
        var pivot = d as Windows.UI.Xaml.Controls.Pivot;
        #endif

        if (pivot == null)
        {
            return;
        }

        pivot.Loaded -= Pivot_Loaded;
        pivot.Loaded += Pivot_Loaded;
    }

    private static void Pivot_Loaded(object sender, RoutedEventArgs e)
    {
        #if UNO
        var pivot = sender as Microsoft.UI.Xaml.Controls.Pivot;
        #else
        var pivot = sender as Windows.UI.Xaml.Controls.Pivot;
        #endif

        #if UNO
        var panels = pivot.FindDescendants().OfType<Microsoft.UI.Xaml.Controls.Primitives.PivotHeaderPanel>().Where(panel => panel.FindAscendant<Microsoft.UI.Xaml.Controls.Pivot>() == pivot);
        #else
        // Make sure we find the PivotHeaderPanels for just this Pivot (in case we have embedded pivots)
        var panels = pivot.FindDescendants().OfType<PivotHeaderPanel>().Where(panel => panel.FindAscendant<Windows.UI.Xaml.Controls.Pivot>() == pivot);
        #endif
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
        #if UNO
        var pivot = d as Microsoft.UI.Xaml.Controls.Pivot;
        #else
        var pivot = d as Windows.UI.Xaml.Controls.Pivot;
        #endif

        pivot.Loaded -= Pivot_Loaded;
        pivot.Loaded += Pivot_Loaded;

        if (e.NewValue != null)
        {
            SetContentVisible(pivot, (bool)e.NewValue);
        }
    }

    #if UNO
    private static void SetContentVisible(Microsoft.UI.Xaml.Controls.Pivot pivot, bool value)
    #else 
    private static void SetContentVisible(Windows.UI.Xaml.Controls.Pivot pivot, bool value)
    #endif
    {
        // Make sure we find the PivotHeaderPanels for just this Pivot (in case we have embedded pivots)
        var presenter = pivot.FindDescendant("PivotItemPresenter");

        if (presenter != null)
        {
            presenter.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
