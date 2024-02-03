using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using XamlStudio.Models;

namespace XamlStudio.ViewModels;

public partial class PropertiesViewModel : ObservableObject
{
    [ObservableProperty]
    private DependencyObject _selectedElement;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParent))]
    private DependencyObject _selectedElementParent;

    public bool HasParent => SelectedElementParent != null;

    [ObservableProperty]
    private ObservableCollection<PropertyInfo> _propertyValues;
}
