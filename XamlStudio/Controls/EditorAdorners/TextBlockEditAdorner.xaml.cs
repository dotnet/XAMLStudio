using CommunityToolkit.WinUI.Controls.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace XamlStudio.Controls;

public sealed partial class TextBlockEditAdorner : UserControl
{
    public TextBlock AttachedElement { get; }

    public TextBlockEditAdorner(FrameworkElement attachedElement)
    {
        AttachedElement = attachedElement as TextBlock;

        // TODO: We may want to snapshot text and have a confirm/cancel approach, where it either goes back to original text or gets applied to the XAML in the editor...

        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AdornerLayer.SetXaml(AttachedElement, null);
    }
}
