// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml;

namespace XamlStudio.Models;

public partial class PropertyInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDefault))]
    public partial object Value { get; set; }

    private bool IsDefault => Value == DependencyProperty.UnsetValue;

    [ObservableProperty]
    public partial string Group { get; set; }

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
