using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
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
            }
            catch
            {
                // TODO: Show error in property editor...
            }
        }
    }

    public void Receive(EditorSelectedElementMessage message)
    {
        if (_coordinator.TryGetVisualElement(message.Element, out var element))
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

            // Find properties of interest...
            List<PropertyInfo> properties = new();

            // Helper Function to Add Property Values to our List (if value is set)
            void AddProperty(Type type, string propName, string? group = null)
            {
                if (XamlXmlTreeCoordinator.AttributeNameToDependencyProperty.TryGetValue(type, out var depProps)
                    && depProps.TryGetValue(propName, out var depProp))
                {
                    var value = element.ReadLocalValue(depProp);

                    if (value != DependencyProperty.UnsetValue)
                    {
                        properties.Add(new(type, depProp, propName, value, value?.GetType(), group));
                    }
                }
            }

            // TODO: Pinned...

            // First list properties we've modified in XML
            var definedAttributes = new HashSet<string>(message.Element.Attributes.Select(a => a.Name));
            foreach (var attr in definedAttributes)
            {
                AddProperty(element.GetType(), attr, "- Set in XAML -");
            }

            // Add other known properties, if their values are set.
            foreach ((var type, var propsLookup) in XamlXmlTreeCoordinator.AttributeNameToDependencyProperty)
            {
                foreach ((var key, var depProp) in propsLookup)
                {
                    if (!definedAttributes.Contains(key))
                    {
                        AddProperty(element.GetType(), key);
                    }
                }                
            }

            // TODO: Add properties we don't know about???

            ViewModel.PropertyValues = new(properties
                                           .GroupBy(static pi => pi.Group)
                                           .OrderBy(static g => g.Key));
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
}
