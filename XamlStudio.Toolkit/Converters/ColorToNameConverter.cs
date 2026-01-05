// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Converters;

[Bindable]
public class ColorToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return null;

        if (value is Color color)
        {
            // Look up the name and return it
            foreach (var colorProps in typeof(Colors).GetRuntimeProperties())
            {
                if (color == (Color)colorProps.GetValue(null))
                {
                    return colorProps.Name;
                }
            }

            // If this color is unknown, output the color hex
            return new StringBuilder("#")
                .Append(color.A.ToString("X2"))
                .Append(color.R.ToString("X2"))
                .Append(color.G.ToString("X2"))
                .Append(color.B.ToString("X2"))
                .ToString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
