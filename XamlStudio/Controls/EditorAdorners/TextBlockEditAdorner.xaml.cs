using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;

namespace XamlStudio.Controls;

public sealed partial class TextBlockEditAdorner : Adorner
{
    private string _originalText;

    public TextBlock AdornedTextBlock
    {
        get { return (TextBlock)GetValue(AdornedTextBlockProperty); }
        set { SetValue(AdornedTextBlockProperty, value); }
    }

    public static readonly DependencyProperty AdornedTextBlockProperty =
        DependencyProperty.Register(nameof(AdornedTextBlock), typeof(TextBlock), typeof(TextBlockEditAdorner), new PropertyMetadata(null));

    public TextBlockEditAdorner()
    {
        this.InitializeComponent();
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AdornedTextBlock = AdornedElement as TextBlock;
        _originalText = AdornedTextBlock.Text;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AdornedTextBlock = null;
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send<AddToXamlMessage>(new(AdornedTextBlock, "Text", AdornedTextBlock.Text.Replace("\"", "&quot;")));

        AdornerLayer.SetXaml(AdornedTextBlock, null);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AdornerLayer.SetXaml(AdornedTextBlock, null);

        // TODO: We may want to do our trick to check if there's a binding here?
        AdornedTextBlock.Text = _originalText;
    }
}
