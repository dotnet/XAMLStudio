// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml.Markup;

namespace XamlStudio.Helpers;

// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/7633
[MarkupExtensionReturnType(ReturnType = typeof(object))]
public class EnumValueExtension : MarkupExtension
{
    public Type Type { get; set; }

    public string Member { get; set; }

    protected override object ProvideValue()
    {
        return Enum.Parse(Type, Member);
    }
}
