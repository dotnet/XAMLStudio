using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Controls.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using XamlStudio.Controls;
using XamlStudio.Models;

namespace XamlStudio.Views;

public partial class Document :
    IRecipient<EditorSelectedElementMessage>
{
    private bool _isDesignEnabled = false;
    private FrameworkElement? _highlightedElement;

    private void HighlightElement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton button)
        {
            _isDesignEnabled = button.IsChecked == true;

            if (_isDesignEnabled)
            {
                AttachAdorner();                
            }
            else
            {
                RemoveAdorner();
            }
        }
    }

    public void Receive(EditorSelectedElementMessage message)
    {
        if (_isDesignEnabled
            && ViewModel.XamlCoordinator.TryGetVisualElement(message.Element, out var uie)
            && uie is FrameworkElement fwe)
        {
            RemoveAdorner();

            _highlightedElement = fwe;

            AttachAdorner();
        }
    }

    private void AttachAdorner()
    {
        if (_highlightedElement == null) return;

        AdornerLayer.SetXaml(_highlightedElement, new DesignerAdorner(_highlightedElement));
    }

    private void RemoveAdorner()
    {
        if (_highlightedElement == null) return;

        AdornerLayer.SetXaml(_highlightedElement, null);

        _highlightedElement = null;
    }
}
