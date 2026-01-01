using CommunityToolkit.WinUI;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace XamlStudio.Toolkit.Controls.Adorners;

public sealed partial class ModifySelectorAdorner : Adorner
{
    public event TypedEventHandler<ModifySelectorAdorner, UIElement> SelectedElementClicked;

    public Type AdornedElementType
    {
        get { return (Type)GetValue(AdornedElementTypeProperty); }
        set { SetValue(AdornedElementTypeProperty, value); }
    }

    public static readonly DependencyProperty AdornedElementTypeProperty =
        DependencyProperty.Register(nameof(AdornedElementType), typeof(Type), typeof(ModifySelectorAdorner), new PropertyMetadata(null));

    public bool IsContainer
    {
        get { return (bool)GetValue(IsContainerProperty); }
        set { SetValue(IsContainerProperty, value); }
    }

    public static readonly DependencyProperty IsContainerProperty =
        DependencyProperty.Register(nameof(IsContainer), typeof(bool), typeof(ModifySelectorAdorner), new PropertyMetadata(null));

    public ModifySelectorAdorner()
    {
        this.InitializeComponent();
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AdornedElementType = AdornedElement.GetType();
        IsContainer = typeof(Panel).IsAssignableFrom(AdornedElementType) || typeof(ItemsControl).IsAssignableFrom(AdornedElementType);
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);

        SelectedElementClicked?.Invoke(this, AdornedElement);
    }

    public static HorizontalAlignment AlignmentForContainer(bool isContainer) => isContainer ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public static SolidColorBrush ColorForContainer(bool isContainer) => new(isContainer ? Colors.Purple : Colors.DarkCyan);
}
