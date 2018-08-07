using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Future
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
        private static void OnAllowDeselectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ListBox listBox)
            {
                if ((bool)e.NewValue == true)
                {
                    listBox.SelectionMode = SelectionMode.Multiple;
                    listBox.SelectionChanged += OnSelectionChanged;
                }
                else
                {
                    listBox.SelectionChanged -= OnSelectionChanged;
                }
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var listBox = (ListBox)sender;
                var newlySelected = e.AddedItems[0];

                foreach (var item in listBox.SelectedItems)
                {
                    if (item != newlySelected)
                    {
                        listBox.SelectedItems.Remove(item);
                    }
                }
            }
        }
    }
}