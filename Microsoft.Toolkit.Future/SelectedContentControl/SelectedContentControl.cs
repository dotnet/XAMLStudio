using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.WinUI.Controls.Future;

public sealed class SelectedContentControl : ListViewBase // Can't inherit from Selector: https://github.com/microsoft/microsoft-ui-xaml/issues/205
{
    public SelectedContentControl()
    {
        this.DefaultStyleKey = typeof(SelectedContentControl);

        UpdateSelection();

        SelectionChanged += SelectedContentControl_SelectionChanged;
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SelectedContentItem;

    protected override DependencyObject GetContainerForItemOverride() => new SelectedContentItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SelectedContentItem container)
        {
            container.Visibility = (item == SelectedItem) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void SelectedContentControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var removedItem in e.RemovedItems)
        {
            if (ContainerFromItem(removedItem) is SelectedContentItem removedContainer)
            {
                removedContainer.Visibility = Visibility.Collapsed;
            }
        }

        foreach (var addedItem in e.AddedItems)
        {
            if (ContainerFromItem(addedItem) is SelectedContentItem addedContainer)
            {
                addedContainer.Visibility = Visibility.Visible;
            }
        }
    }

    protected override void OnItemsChanged(object e)
    {
        base.OnItemsChanged(e);

        // Ensure a valid selection (auto-select first item)
        if ((SelectedIndex < 0 && Items.Count > 0)
            || SelectedIndex >= Items.Count)
        {
            SelectedIndex = 0;
        }

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        // Show only the selected container; hide the rest.
        for (int i = 0; i < Items.Count; i++)
        {
            if (ContainerFromIndex(i) is SelectedContentItem container)
            {
                container.Visibility = (SelectedIndex == i) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
