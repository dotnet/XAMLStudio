using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Collections;
using Windows.UI.Xaml;
using XamlStudio.Models;
using System;
using System.Collections.ObjectModel;

namespace XamlStudio.ViewModels;

public partial class PropertiesViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasElement))]
    private DependencyObject _selectedElement;

    public bool HasElement => SelectedElement != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParent))]
    private DependencyObject _selectedElementParent;

    public bool HasParent => SelectedElementParent != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChildren))]
    private DependencyObject[] _selectedElementChildren;

    public bool HasChildren => SelectedElementChildren?.Length > 0;

    [ObservableProperty]
    private ObservableGroupedCollection<string, PropertyInfo> _propertyValues;

    [ObservableProperty]
    private ObservableGroupedCollection<string, PropertyInfo> _unsetPropertyValues;
}
