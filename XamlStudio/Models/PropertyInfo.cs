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

    public string Group { get; private set; }

    public Type ElementType { get; private set; }

    public DependencyProperty Property { get; private set; }

    public string Name { get; private set; }

    public Type Type { get; private set; }

    public PropertyInfo(Type elementType, DependencyProperty property, string name, object value, Type type, string? group = null)
    {
        ElementType = elementType;
        Property = property;
        Name = name;
        Type = type;
        Value = value;
        Group = group ?? ElementType.Name;
    }
}
