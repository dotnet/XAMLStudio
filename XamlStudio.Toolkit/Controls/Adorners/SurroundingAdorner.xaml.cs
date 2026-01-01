using CommunityToolkit.WinUI;
using System;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace XamlStudio.Toolkit.Controls.Adorners;

public sealed partial class SurroundingAdorner : Adorner
{
    // TODO: DP?
    public Type AdornedElementType { get; private set; }

    public Point Position { get; private set; }

    public Size Size { get; private set; }

    public Thickness AttachedBorder = new();

    public Thickness AttachedPadding = new();

    public Size InnerSize;

    public Thickness NegativeMargin;

    private UIElement _parentElement;

    public SurroundingAdorner(UIElement rootElement)
    {
        InitializeComponent();

        _parentElement = rootElement;

        // TODO: Could be nice for Adorner in Toolkit to have a callback event for us for when metrics change...
        SizeChanged += SurroundingAdorner_SizeChanged;
        LayoutUpdated += SurroundingAdorner_LayoutUpdated;
    }

    private void SurroundingAdorner_LayoutUpdated(object sender, object e)
    {
        if (AdornedElement is null) return;

        CalculateMetrics();
    }

    private void SurroundingAdorner_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (AdornedElement is null) return;

        CalculateMetrics();
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        CalculateMetrics();
    }

    private void CalculateMetrics()
    {
        AdornedElementType = AdornedElement.GetType();

        var AttachedElement = AdornedElement as FrameworkElement;

        // TODO: We want to listen to any changes to these properties to update our calculations and/or use direct Binding/Converters (remember OneWay)

        // Note: The Position/Sizing of the Element already factors in the Margin defined on the element
        // So we need to add the margin from our sizing and subtract from position to compensate
        var mLeft = AttachedElement.Margin.Left;
        var mTop = AttachedElement.Margin.Top;
        var mRight = AttachedElement.Margin.Right;
        var mBottom = AttachedElement.Margin.Bottom;

        NegativeMargin = new(-mLeft, -mTop, -mRight, -mBottom); // Used to compensate for the auto-applied margin with our adorner.

        Size = new(AttachedElement.ActualSize.X + mLeft + mRight, AttachedElement.ActualSize.Y + mTop + mBottom);

        var position = AdornedElement.CoordinatesFrom(_parentElement);
        Position = new(position.X - mLeft, position.Y - mTop);

        // Then we'll see if we have Borders or Padding as they're specific to certain types of elements
        if (AdornedElementType.GetProperty("BorderThickness") is PropertyInfo border)
        {
            AttachedBorder = (Thickness)(border.GetValue(AttachedElement) ?? new());
        }

        if (AdornedElementType.GetProperty("Padding") is PropertyInfo padding)
        {
            AttachedPadding = (Thickness)(padding.GetValue(AttachedElement) ?? new());
        }

        // Calculate inner size (margin is already factored into size by framework)
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
    }
}
