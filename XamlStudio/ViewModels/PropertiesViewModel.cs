// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Windows.UI.Xaml;
using XamlStudio.Models;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.ViewModels;

public partial class PropertiesViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasElement))]
    public partial DependencyObject SelectedElement { get; set; }

    public bool HasElement => SelectedElement != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParent))]
    public partial DependencyObject SelectedElementParent { get; set; }

    public bool HasParent => SelectedElementParent != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChildren))]
    public partial DependencyObject[] SelectedElementChildren { get; set; }

    public bool HasChildren => SelectedElementChildren?.Length > 0;

    [ObservableProperty]
    public partial ObservableGroupedCollection<string, PropertyInfo> PropertyValues { get; set; }

    [ObservableProperty]
    public partial ObservableGroupedCollection<string, PropertyInfo> UnsetPropertyValues { get; set; }

    [ObservableProperty]
    public partial Dictionary<string, VisualStateInfo[]> VisualStates { get; set; }
}
