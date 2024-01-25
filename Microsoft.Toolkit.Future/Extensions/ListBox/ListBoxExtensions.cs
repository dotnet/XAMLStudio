using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.WinUI.Extensions.Future
{
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty AllowDeselectionProperty =
            DependencyProperty.RegisterAttached("AllowDeselection",
                                                typeof(bool),
                                                typeof(ListBoxExtensions),
                                                new PropertyMetadata(false, OnAllowDeselectionChanged));

        public static bool GetAllowDeselection(DependencyObject obj)
        {
            return (bool)obj.GetValue(AllowDeselectionProperty);
        }

        public static void SetAllowDeselection(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowDeselectionProperty, value);
        }

        public static object GetSelectedObject(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedObjectProperty);
        }

        public static void SetSelectedObject(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedObjectProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.RegisterAttached("SelectedObject", typeof(object), typeof(ListBoxExtensions), new PropertyMetadata(null));

        private static void OnAllowDeselectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ListBox listBox)
            {
                if ((bool)e.NewValue == true)
                {
                    listBox.Tapped += OnTapped;
                    listBox.SelectionChanged += OnSelectionChanged;
                }
                else
                {
                    listBox.Tapped -= OnTapped;
                    listBox.SelectionChanged -= OnSelectionChanged;
                }
            }
        }

        private static void OnTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (e.OriginalSource is FrameworkElement fe)
                {
                    var parent = fe.FindAscendant<ListBoxItem>();
                    if (parent != null && parent == listBox.SelectedItem)
                    {
                        if (GetSelectedObject(listBox) == null)
                        {
                            // If we tap the same item as selected, deselect the item.
                            listBox.SelectedIndex = -1;
                        }
                        else
                        {
                            // First time means we just selected it.
                            SetSelectedObject(listBox, null);
                        }
                    }
                }
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var listBox = (ListBox)sender;
                var newlySelected = e.AddedItems[0];

                SetSelectedObject(listBox, newlySelected);
            }
        }
    }
}