using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using XamlStudio.Models;
using XamlStudio.Toolkit.Services;
using XamlStudio.ViewModels;

namespace XamlStudio.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Properties : Page,
    IRecipient<EditorSelectedElementMessage>,
    IRecipient<XamlRenderedMessage>
{
    public MainViewModel MainViewModel { get; set; }

    public PropertiesViewModel ViewModel { get; private set; } = new();

    private XamlXmlTreeCoordinator _coordinator = new();

    public Properties()
    {
        this.InitializeComponent();

        Loaded += Properties_Loaded;
        Unloaded += Properties_Unloaded;
    }

    private void Properties_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.RegisterAll(this);

        // Check if there's a render and initialize our existing state
        if (MainViewModel.ActiveDocumentViewModel.HasCompiled)
        {
            Receive(new XamlRenderedMessage(MainViewModel.ActiveDocumentViewModel.Result));
        }
    }

    private void Properties_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (Parent == null)
        {
            // TODO: Not sure if this will cause problems with instability in loaded/unloaded... use check above for Parent?
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }

    private void PropertyValue_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb
            && tb.DataContext is PropertyInfo pi)
        {
            try
            {
                ViewModel.SelectedElement.SetValue(pi.Property, XamlBindingHelper.ConvertValue(pi.Type, tb.Text));
                // TODO: Localize these group names
                var group = ViewModel.PropertyValues.FirstGroupByKey("- Set In XAML -");
                if (group.Contains(pi))
                {
                    group.Remove(pi);
                    // TODO: pi.Group = "- Modified -";
                    ViewModel.PropertyValues.AddItem("- Modified -", pi);
                }
            }
            catch
            {
                // TODO: Show error in property editor...
            }
        }
    }

    private void AddProperties_HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton button)
        {
            button.ContextFlyout.ShowAt(button, new()
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            });
        }
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is PropertyInfo pi)
        {
            // TODO: Do we need to remember these modified properties somehow so if you come back later they still show as modified?
            //       Maybe with an attached property on the element to store the list of prop names?
            ViewModel.PropertyValues.AddItem("- Modified -", pi);
            ViewModel.UnsetPropertyValues.RemoveItem(pi.Group, pi);

            // TODO: Set focus to TextBox of new item inserted...
            //// DispatcherQueue...?
        }
    }

    public void Receive(EditorSelectedElementMessage message)
    {
        if (_coordinator.TryGetVisualElement(message.Element, out var element))
        {
            UpdateProperties(element, message.Element);
        }
    }

    // Method used to update state based on a given Visual element.
    private void UpdateProperties(DependencyObject element, IXmlElementSyntax xmlHint = null)
    {
        if (element != ViewModel.SelectedElement)
        {
            ViewModel.SelectedElement = element;
            if (element.FindAscendant<DependencyObject>() is DependencyObject parent)
            {
                ViewModel.SelectedElementParent = parent;
            }
            else
            {
                ViewModel.SelectedElementParent = null;
            }

            if (VisualTreeHelper.GetChildrenCount(element) is int count && count > 0)
            {
                List<DependencyObject> children = new();
                for (int i = 0; i < count; i++)
                {
                    children.Add(VisualTreeHelper.GetChild(element, i));
                }
                ViewModel.SelectedElementChildren = children.ToArray();
            }
            else
            {
                ViewModel.SelectedElementChildren = null;
            }

            // Find properties of interest...
            List<PropertyInfo> properties = new();
            List<PropertyInfo> unsetProperties = new();

            // Helper Function to Add Property Values to our List (if value is set)
            PropertyInfo AddProperty(Type type, string propName, string? group = null)
            {
                var ctype = type;
                do
                {
                    if (XamlXmlTreeCoordinator.AttributeNameToDependencyProperty.TryGetValue(ctype, out var depProps)
                        && depProps.TryGetValue(propName, out var depProp))
                    {
                        var value = element.ReadLocalValue(depProp);

                        if (value != DependencyProperty.UnsetValue)
                        {
                            properties.Add(new(ctype, depProp, propName, value, value?.GetType(), group));
                        }
                        else
                        {
                            var defaultValue = element.GetValue(depProp);
                            /// TODO: Do we need a DepProp to type map? Could also be handy to have the default values...?
                            return new(ctype, depProp, propName, defaultValue, defaultValue?.GetType() ?? typeof(string));
                        }
                    }
                    ctype = ctype.BaseType;
                } while (ctype != typeof(DependencyObject));

                return null;
            }

            // TODO: Pinned...

            HashSet<string> definedAttributes = new();

            // Check if we have an associated XML element to see what we set in our Editor text
            if (xmlHint == null
                && _coordinator.TryGetXmlElement(element, out xmlHint))
            {
                // First list properties we've modified in XML
                definedAttributes = new(xmlHint.Attributes.Select(a => a.Name));
                foreach (var attr in definedAttributes)
                {
                    AddProperty(element.GetType(), attr, "- Set in XAML -");
                }
            }

            List<string> groupOrder = new() { "- Modified -", "- Set in XAML -" };

            PropertyInfo unset = null;

            // Add other known properties, if their values are set.
            var type = element.GetType();
            do
            {
                groupOrder.Add(type.Name);
                if (XamlXmlTreeCoordinator.AttributeNameToDependencyProperty.TryGetValue(type, out var depPropsLookup))
                {
                    foreach ((var key, var depProp) in depPropsLookup)
                    {
                        if (!definedAttributes.Contains(key))
                        {
                            unset = AddProperty(type, key);

                            if (unset != null)
                            {
                                unsetProperties.Add(unset);
                            }
                        }
                    }
                }
                type = type.BaseType;
            } while (type != typeof(DependencyObject));

            // TODO: Add properties we don't know about???

            // TODO: Do we need empty Pinned and Modified groups at top empty so they stay in the sort position?
            ViewModel.PropertyValues = new(properties
                                           .GroupBy(static pi => pi.Group)
                                           .OrderBy(g => groupOrder.IndexOf(g.Key)));

            ViewModel.UnsetPropertyValues = new(unsetProperties
                                                .GroupBy(static pi => pi.Group)
                                                .OrderBy(g => groupOrder.IndexOf(g.Key)));
        }
    }

    // TODO: Have a message for selecting an element (for when we allow direct interaction with render or for clicking on Parent to go up)
    // In there we need to have a thing that doesn't move the editor cursor, but does highlight the line of the corresponding element.

    public void Receive(XamlRenderedMessage message)
    {
        // TODO: Is this recalled when document changes?
        _coordinator.Initialize(message.Context.XmlDocument, (DependencyObject)message.Context.Element);

        // TODO: Need to get current editor node/context between renders, reference won't work as will be stale Xml Node.
        // Send message? GetEditorPositionMessage?
    }

    public static string GetElementInfo(DependencyObject element)
    {
        if (element == null) return "null";

        var name = element.ReadLocalValue(FrameworkElement.NameProperty);

        return ((name != DependencyProperty.UnsetValue) ? $"\"{name}\" " : "") +
            "<" + element.GetType().Name + ">";
    }

    private void VisualTreeElement_Click(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        if (sender.FindAscendant<TextBlock>()?.DataContext is DependencyObject element)
        {
            UpdateProperties(element);
        } 
    }
}
