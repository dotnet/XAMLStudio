using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Controls.Future;
using Microsoft.Language.Xml;
using Monaco;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using XamlStudio.Controls;
using XamlStudio.Models;
using XamlStudio.Toolkit.Extensions;

namespace XamlStudio.Views;

public partial class Document :
    IRecipient<EditorSelectedElementMessage>,
    IRecipient<SelectedVisualElementMessage>,
    IRecipient<AddToXamlMessage>
{
    private DesignerMode _designerMode = DesignerMode.View;

    private void DesignerModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RemoveAdorner();

        var si = e.AddedItems.FirstOrDefault() as SegmentedItem;
        switch (si?.Tag)
        {
            case "View":
                _designerMode = DesignerMode.View;
                break;
            case "Add": // TODO: Do we need Add Mode vs. Modify (one for manipulating panels/containers for new item, and one for manipulating existing elements?)
                _designerMode = DesignerMode.Add;
                break;
            case "Modify":
                _designerMode = DesignerMode.Modify;
                break;
            // TODO: Probably need 'Delete' as well...
            case "Highlight":
                _designerMode = DesignerMode.Highlight;
                break;
        }

        if (_designerMode == DesignerMode.Modify)
        {
            foreach (FrameworkElement element in ViewModel.XamlCoordinator.GetVisualElements().Where((e) => e is FrameworkElement))
            {
                AdornerLayer.SetXaml(element, new ModifySelectorAdorner(element, ViewModel.MainViewModel));
            }
        }
        else if (_designerMode != DesignerMode.View)
        {
            AttachAdorner(ViewModel.HighlightedElement);
        }
    }

    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        base.OnPointerMoved(e);

        return;

        // Note: Leaving this hear for future usage, heuristic needed for selecting the 'right' type of element we want.

        // TODO: This doesn't work with Runs...
        if (_designerMode == DesignerMode.Modify
            && e.OriginalSource is FrameworkElement source
            && source != ViewModel.HighlightedElement)
        {
            // Detect if Source in document...
            // TODO: Need to figure out differences if our Custom ResourceViewer root...
            var root = IsSpecificPreviewSize ? XamlRootSpecific : XamlRoot;

            // Question: Do we want to prioritize the Elements in Position or walking the ascendents of the visual tree?
            UIElement attachElement = source;
            bool found = false;

            // Check and try and find closest matching Xml Element to something in the coordinate space
            var point = e.GetCurrentPoint(root).Position;
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(point, root, true);
            foreach (var element in elements)
            {
                Debug.WriteLine("Checking Host Element: " + element);
                if (ViewModel.XamlCoordinator.TryGetXmlElement(element, out _))
                {
                    Debug.WriteLine("Found Matching Xml Parent Visual: " + element);
                    attachElement = element;
                    found = true;
                    break;
                }
            }

            // We didn't find a different element in our Xml, try the visual tree (also checks we're within the preview area)
            if (!found)
            {
                foreach (var element in source.FindAscendants())
                {
                    Debug.WriteLine("Checking Parent Element: " + element);
                    if (ViewModel.XamlCoordinator.TryGetXmlElement(element, out _))
                    {
                        Debug.WriteLine("Found Matching Xml Parent Visual: " + element);
                        attachElement = element as FrameworkElement;
                        found = true;
                        break;
                    }
                }
            }

            /* var parent = source.FindAscendant<Grid>((element) => element == root);

            // Did we end up finding we had the XamlRoot as a parent?
            if (parent is not null*/

            if (found
                && attachElement != ViewModel.HighlightedElement)
            {
                Debug.WriteLine("Attached Original Source: " + attachElement);
                AttachAdorner(attachElement as FrameworkElement);
            }
            else
            {
                Debug.WriteLine("NA Original Source: " + e.OriginalSource);
            }
        }
        else
        {
            Debug.WriteLine("NF Original Source: " + e.OriginalSource);
        }
    }

    public void Receive(SelectedVisualElementMessage message)
    {
        RemoveAdorner();

        AttachAdorner(message.Element as FrameworkElement);
    }

    public void Receive(EditorSelectedElementMessage message)
    {
        RemoveAdorner();

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
        else if (ViewModel.HighlightedElement != null)
        {
            // TODO: In the future, we could maybe support selecting multiple elements for comparison/measuring?
            // Remove prior adorner before attaching a new one.
            RemoveAdorner();
        }

        ViewModel.HighlightedElement = element;

        if (_designerMode == DesignerMode.Highlight)
        {
            AdornerLayer.SetXaml(ViewModel.HighlightedElement, new SurroundingAdorner(ViewModel.HighlightedElement, ViewModel.HighlightedElement.CoordinatesFrom((UIElement)ViewModel.Result.Element)));
        }
        else if (_designerMode == DesignerMode.Modify)
        {
            // TODO: This is the case where we'll have selected something, so we probably want to have a more specific 'editing' adorener here for the specific types of controls
            // e.g for an image it could have a button which opens a file picker for the workspace (or can detect dragged image from there or something)
            // for a textblock it can have a textbox for the contents
            // for a button it can have the behavior for navigation, etc...

            if (_editorAdornerTypeMap.TryGetValue(ViewModel.HighlightedElement.GetType(), out var adornerType))
            {
                // All editor adorners just accept a FrameworkElement
                ConstructorInfo constructor = adornerType.GetConstructor(new[] { typeof(FrameworkElement) });

                AdornerLayer.SetXaml(ViewModel.HighlightedElement, constructor.Invoke(new[] { ViewModel.HighlightedElement }) as FrameworkElement);
            }
            else
            {
                AdornerLayer.SetXaml(ViewModel.HighlightedElement, new ModifySelectorAdorner(ViewModel.HighlightedElement, ViewModel.MainViewModel));
            }
        }
    }

    private void RemoveAdorner()
    {
        if (_designerMode == DesignerMode.Modify)
        {
            // Remove all adorners from other elements we had added above for selection
            foreach (FrameworkElement element in ViewModel.XamlCoordinator.GetVisualElements().Where((e) => e is FrameworkElement))
            {
                AdornerLayer.SetXaml(element, null);
            }
        }

        if (ViewModel?.HighlightedElement == null) return;

        AdornerLayer.SetXaml(ViewModel.HighlightedElement, null);
    }

    public async void Receive(AddToXamlMessage message)
    {
        // TODO: We need to be modifying the Xml Document syntax and using that to modify text vs. text itself...
        var text = CodeEditor.Text;
        if (ViewModel.XamlCoordinator.TryGetXmlElement(message.Element, out var node)
            && node is XmlNodeSyntax xmlNode)
        {
            var loc = text.GetLineColumnIndex(xmlNode.Span.Start);

            if (xmlNode is IXmlElementSyntax xmlElement)
            {
                var attribute = xmlElement.Attributes.FirstOrDefault((attr) => attr.Name == message.Property);

                if (attribute != null)
                {
                    loc = text.GetLineColumnIndex(attribute.Span.Start);
                }
            }

            await CodeEditor.RevealPositionInCenterAsync(new Position((uint)loc.Line, (uint)loc.Column));

            var lines = text.Split(Environment.NewLine);

            var targetLine = lines[loc.Line - 1];
            if (targetLine.Contains(message.Property))
            {
                var sp = targetLine.IndexOf(message.Property + "=") + message.Property.Length + 2;
                lines[loc.Line - 1] = targetLine.Substring(0, sp) + $"{message.Value}" + targetLine.Substring(targetLine.IndexOf("\"", sp + 1));
            }
            else if (targetLine.Trim().EndsWith("/>"))
            {
                lines[loc.Line - 1] = targetLine.Substring(0, targetLine.Length - 2) + $" {message.Property}=\"{message.Value}\"/>";
            }
            else if (targetLine.Trim().EndsWith(">"))
            {
                lines[loc.Line - 1] = targetLine.Substring(0, targetLine.Length - 1) + $" {message.Property}=\"{message.Value}\">";
            }
            else
            {
                lines[loc.Line - 1] = targetLine + $" {message.Property}=\"{message.Value}\"";
            }

            CodeEditor.Text = string.Join(Environment.NewLine, lines);
        }
    }

    private enum DesignerMode
    {
        View,
        Add,
        Modify,
        Highlight,
    }

    private Dictionary<Type, Type> _editorAdornerTypeMap = new()
    {
        { typeof(TextBlock), typeof(TextBlockEditAdorner) },
    };
}
