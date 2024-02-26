using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls.Future;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using XamlStudio.Controls;
using XamlStudio.Models;

namespace XamlStudio.Views;

public partial class Document :
    IRecipient<EditorSelectedElementMessage>,
    IRecipient<SelectedVisualElementMessage>
{
    private bool _isHighlightEnabled = false;
    private FrameworkElement? _highlightedElement;

    private void HighlightElement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton button)
        {
            _isHighlightEnabled = button.IsChecked == true;

            if (_isHighlightEnabled)
            {
                AttachAdorner(_highlightedElement);                
            }
            else
            {
                RemoveAdorner();
            }
        }
    }

    public void Receive(SelectedVisualElementMessage message)
    {
        AttachAdorner(message.Element as FrameworkElement);
    }

    public void Receive(EditorSelectedElementMessage message)
    {
        if (ViewModel.XamlCoordinator.TryGetVisualElement(message.Element, out var uie)
            && uie is FrameworkElement fwe)
        {
            AttachAdorner(fwe);
        }
    }

    private void AttachAdorner(FrameworkElement element)
    {
        if (element == null)
        {
            return;
        }
        else if (_highlightedElement != null)
        {
            // TODO: In the future, we could maybe support selecting multiple elements for comparison/measuring?
            // Remove prior adorner before attaching a new one.
            RemoveAdorner();
        }

        _highlightedElement = element;

        if (_isHighlightEnabled)
        {
            AdornerLayer.SetXaml(_highlightedElement, new SurroundingAdorner(_highlightedElement, _highlightedElement.CoordinatesFrom((UIElement)ViewModel.Result.Element)));
        }
    }

    private void RemoveAdorner()
    {
        if (_highlightedElement == null) return;

        AdornerLayer.SetXaml(_highlightedElement, null);
    }
}
