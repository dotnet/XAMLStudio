// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Converters;

[Bindable]
public sealed class StringToUriConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string)
        {

            if (Uri.TryCreate(value as string, UriKind.RelativeOrAbsolute, out var result))
            {
                return result;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
