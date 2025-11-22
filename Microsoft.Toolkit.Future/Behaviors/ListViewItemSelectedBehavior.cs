using CommunityToolkit.WinUI.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.WinUI.Behaviors.Future;

/// <summary>
/// Behavior which allows for binding to the IsSelected property of a <see cref="ListViewItem"/>.
/// </summary>
public class ListViewItemSelectedBehavior : BehaviorBase<FrameworkElement>
{
    WeakReference<ListViewItem>? _parentContainer;

    public bool IsSelected
    {
        get { return (bool)GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ListViewItemSelectedBehavior), new PropertyMetadata(false, OnIsSelectedChanged));

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update our parent ListViewItem when we update the IsSelected property of our behavior.
        if (d is ListViewItemSelectedBehavior behavior
            && behavior._parentContainer is not null
            && behavior._parentContainer.TryGetTarget(out ListViewItem lvi))
        {
            lvi.IsSelected = e.NewValue as bool? == true;
        }
    }

    protected override void OnAssociatedObjectLoaded()
    {
        base.OnAssociatedObjectLoaded();

        // When loaded, match current state of parent ListViewItem
        var lvi = AssociatedObject?.FindAscendant<ListViewItem>();
        if (lvi is not null)
        {
            _parentContainer = new(lvi);
            lvi.IsSelected = IsSelected;
        }

        // Register for Selection Changed events (weakly)
        var listView = AssociatedObject?.FindAscendant<ListView>();

        var weakPropertyChangedListener = new WeakEventListener<ListViewItemSelectedBehavior, object, SelectionChangedEventArgs>(this)
        {
            OnEventAction = static (instance, source, eventArgs) => instance.OnSelectionChanged(source as ListView, eventArgs),
            OnDetachAction = (weakEventListener) => listView.SelectionChanged -= weakEventListener.OnEvent // Use Local References Only
        };
        listView.SelectionChanged += weakPropertyChangedListener.OnEvent;
    }

    private void OnSelectionChanged(ListView listview, SelectionChangedEventArgs eventArgs)
    {
        if (_parentContainer?.TryGetTarget(out ListViewItem lvi) == true)
        {
            if (eventArgs.AddedItems.Count > 0
                && listview.ContainerFromItem(eventArgs.AddedItems[0]) is ListViewItem lvi2)
            {
                if (lvi == lvi2)
                {
                    IsSelected = true;
                }
            }

            if (eventArgs.RemovedItems.Count > 0
                && listview.ContainerFromItem(eventArgs.RemovedItems[0]) is ListViewItem lvi3)
            {
                if (lvi == lvi3)
                {
                    IsSelected = false;
                }
            }
        }
    }
}
