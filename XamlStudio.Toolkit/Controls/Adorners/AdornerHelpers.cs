using Windows.UI.Xaml;

namespace XamlStudio.Toolkit.Controls.Adorners;

public static class AdornerHelpers
{
    public static string GetElementInfo(DependencyObject element)
    {
        if (element == null) return "null";

        var name = element.ReadLocalValue(FrameworkElement.NameProperty);

        return ((name != DependencyProperty.UnsetValue) ? $"\"{name}\" " : "") +
            "<" + element.GetType().Name + ">";
    }
}
