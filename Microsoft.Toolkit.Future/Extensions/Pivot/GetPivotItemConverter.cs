// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Pivot.Future;

/// <summary>
/// Helper to retrieve the PivotItem from the PivotHeaderItem.
/// </summary>
[Bindable]
public class GetPivotItemConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, string language)
    {
        var pivotheader = value as PivotHeaderItem;

        var panel = pivotheader?.Parent as PivotHeaderPanel;
        var index = panel?.Children?.IndexOf(pivotheader);

        var pivot = (value as DependencyObject)?.FindAscendant<Windows.UI.Xaml.Controls.Pivot>();

        if (index != null)
        {
            return pivot?.Items[index.Value] as PivotItem;
        }

        return null;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
