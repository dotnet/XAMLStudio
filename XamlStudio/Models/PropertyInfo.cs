using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml;

namespace XamlStudio.Models;

public partial class PropertyInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDefault))]
    private object _value;

    private bool IsDefault => Value == DependencyProperty.UnsetValue;

    public string Name { get; private set; }

    public Type Type { get; private set; }

    public PropertyInfo(string name, object value, Type type)
    {
        Name = name;
        Type = type;
        Value = value;
    }
}
