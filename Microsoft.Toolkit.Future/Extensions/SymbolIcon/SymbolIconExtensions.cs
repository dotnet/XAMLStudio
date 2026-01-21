// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Future;

/// <summary>
/// Set of extensions for the <see cref="NavigationView"/> control.
/// </summary>
[Bindable]
public class SymbolIconExtensions
{
    public static double GetFontSize(SymbolIcon obj) => (double)obj.GetValue(FontSizeProperty);

    public static void SetFontSize(SymbolIcon obj, double value) => obj.SetValue(FontSizeProperty, value);

    // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(SymbolIconExtensions), new PropertyMetadata(null, OnFontSizeChanged));

    private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && d is SymbolIcon si)
        {
            var tb = si.FindDescendant<TextBlock>();
            if (tb != null)
            {
                tb.FontSize = (double)e.NewValue;
            }
            else
            {
                // Icon hasn't loaded yet, so we'll hook in there
                si.Loaded += SymbolIcon_Loaded;
            }
        }
    }

    private static void SymbolIcon_Loaded(object sender, RoutedEventArgs e)
    {
        SymbolIcon si = (SymbolIcon)sender;

        si.Loaded -= SymbolIcon_Loaded;

        var tb = si.FindDescendant<TextBlock>();
        if (tb != null)
        {
            tb.FontSize = GetFontSize(si);
        }
    }
}