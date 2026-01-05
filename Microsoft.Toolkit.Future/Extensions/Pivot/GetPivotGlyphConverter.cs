// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Extensions.Future;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Pivot.Future
{
    /// <summary>
    /// Helper to retrieve the Glyph Attached Property from a PivotItem for the PivotHeaderItem Style Templates.
    /// </summary>
    [Bindable]
    public class GetPivotGlyphConverter : GetPivotItemConverter
    {
        // TODO: See if I can use GetPivotItemConverter as a base class?
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (base.Convert(value, targetType, parameter, language) is PivotItem pivotitem)
            {
                var glyph = PivotExtensions.GetGlyph(pivotitem);

                return glyph ?? string.Empty; // ""; // PivotEx.GetGlyph(pivot);
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
