// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;

namespace XamlStudio.Toolkit.Models;

public partial class VisualStateInfo : INotifyPropertyChanged
{
    public bool IsCurrent
    {
        get { return field; }
        set { field = value; OnPropertyChanged(nameof(IsCurrent)); }
    }

    public string Name { get; private set; }

    public string Group { get; private set; }

    public VisualStateInfo(string name, string group, bool isCurrent)
    {
        Name = name;
        Group = group;
        IsCurrent = isCurrent;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
