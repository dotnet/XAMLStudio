using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using System;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using XamlStudio.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Controls;

/// <summary>
/// Used with Mouse Pointer events to help select an element to modify.
/// </summary>
public sealed partial class ModifySelectorAdorner : UserControl
{
    public FrameworkElement AttachedElement { get; }

    public Type AttachedElementType;

    public bool IsContainer => typeof(Panel).IsAssignableFrom(AttachedElementType) || typeof(ItemsControl).IsAssignableFrom(AttachedElementType);

    // TODO: Should just make a message type of changing the open activity, maybe a behavior/command bridge for sending message?
    private MainViewModel MainViewModel;

    public ModifySelectorAdorner(FrameworkElement attachedElement, MainViewModel mainViewModel)
    {
        AttachedElement = attachedElement;
        AttachedElementType = AttachedElement.GetType();
        MainViewModel = mainViewModel;

        this.InitializeComponent();
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (MainViewModel.OpenActivity != "PROPERTIES")
        {
            MainViewModel.OpenActivityPanelCommand.Execute("PROPERTIES");
        }
        WeakReferenceMessenger.Default.Send<SelectedVisualElementMessage>(new(AttachedElement));
    }

    public static HorizontalAlignment AlignmentForContainer(bool isContainer) => isContainer ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public static SolidColorBrush ColorForContainer(bool isContainer) => new(isContainer ? Colors.Purple : Colors.DarkCyan);
}
