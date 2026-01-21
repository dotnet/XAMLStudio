// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Extensions.Future;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Pivot.Future;

/// <summary>
/// Helper to retrieve the CloseCommandButton Attached Property from a Pivot for the PivotHeaderItem Style Templates.
/// </summary>
[Bindable]
public class GetPivotCloseButtonCommandConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var pivot = (value as DependencyObject)?.FindAscendant<Windows.UI.Xaml.Controls.Pivot>();

        if (pivot != null)
        {
            return PivotExtensions.GetCloseButtonCommand(pivot);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
