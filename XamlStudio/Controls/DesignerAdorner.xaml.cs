using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlStudio.Controls;

public sealed partial class DesignerAdorner : UserControl
{
    public FrameworkElement AttachedElement { get; }

    public Type AttachedElementType;

    public Point Position { get; }

    public Size Size;

    public Thickness AttachedBorder = new();

    public Thickness AttachedPadding = new();

    public Size InnerSize;

    public Thickness NegativeMargin;

    public DesignerAdorner(FrameworkElement attachedElement, Point position)
    {
        AttachedElement = attachedElement;
        AttachedElementType = AttachedElement.GetType();

        // TODO: We want to listen to any changes to these properties to update our calculations and/or use direct Binding/Converters (remember OneWay)

        // Note: The Position/Sizing of the Element already factors in the Margin defined on the element
        // So we need to add the margin from our sizing and subtract from position to compensate
        var mLeft = AttachedElement.Margin.Left;
        var mTop = AttachedElement.Margin.Top;
        var mRight = AttachedElement.Margin.Right;
        var mBottom = AttachedElement.Margin.Bottom;

        NegativeMargin = new(-mLeft, -mTop, -mRight, -mBottom); // Used to compensate for the auto-applied margin with our adorner.

        Size = new(AttachedElement.ActualSize.X + mLeft + mRight, AttachedElement.ActualSize.Y + mTop + mBottom);
        Position = new(position.X - mLeft, position.Y - mTop);

        // Then we'll see if we have Borders or Padding as they're specific to certain types of elements
        if (AttachedElementType.GetProperty("BorderThickness") is PropertyInfo border)
        {
            AttachedBorder = (Thickness)(border.GetValue(AttachedElement) ?? new());
        }

        if (AttachedElementType.GetProperty("Padding") is PropertyInfo padding)
        {
            AttachedPadding = (Thickness)(padding.GetValue(AttachedElement) ?? new());
        }

        // Calculate inner size (margin is already factord into size by framework)
        InnerSize = new Size(AttachedElement.ActualSize.X -
                             AttachedBorder.Left -
                             AttachedBorder.Right -
                             AttachedPadding.Left -
                             AttachedPadding.Right,
                             AttachedElement.ActualSize.Y -
                             AttachedBorder.Top -
                             AttachedBorder.Bottom -
                             AttachedPadding.Top -
                             AttachedPadding.Bottom);

        InitializeComponent();
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
