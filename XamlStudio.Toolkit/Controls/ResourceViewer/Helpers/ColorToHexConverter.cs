// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Helpers;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Controls;

public class ColorToHexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // TODO: Have dual converter to lookup if known color value first?.
        if (value is Color color)
        {
            return color.ToHex();
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
