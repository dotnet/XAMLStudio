using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlStudio.Controls;

public sealed partial class DesignerAdorner : UserControl
{
    public FrameworkElement AttachedElement { get; }

    public Type AttachedElementType => AttachedElement.GetType();

    public DesignerAdorner(FrameworkElement attachedElement)
    {
        AttachedElement = attachedElement;

        this.InitializeComponent();
    }

    // TODO: Centralize with one from Properties somewhere?
    public static string GetElementInfo(DependencyObject element)
    {
        if (element == null) return "null";

        var name = element.ReadLocalValue(FrameworkElement.NameProperty);

        return ((name != DependencyProperty.UnsetValue) ? $"\"{name}\" " : "") +
            "<" + element.GetType().Name + ">";
    }
}
