// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CommunityToolkit.WinUI.Extensions.Future;

/// <summary>
/// Set of extensions for the Pivot control.
/// </summary>
public partial class PivotExtensions
{
    /// <summary>
    /// Gets the <see cref="PivotHeaderItem"/> <see cref="Style"/> instance assocaited with the specified <see cref="Windows.UI.Xaml.Controls.Pivot"/>
    /// </summary>
    /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.Pivot"/> from which to get the associated <see cref="Style"/> instance</param>
    /// <returns>The <see cref="Style"/> instance associated with the the <see cref="Windows.UI.Xaml.Controls.Pivot"/> or null</returns>
    #if UNO
    public static Style GetPivotHeaderItemStyle(Microsoft.UI.Xaml.Controls.Pivot obj)
    #else
    public static Style GetPivotHeaderItemStyle(Windows.UI.Xaml.Controls.Pivot obj)
    #endif
    {
        return (Style)obj.GetValue(PivotHeaderItemStyleProperty);
    }

    /// <summary>
    /// Sets the <see cref="PivotHeaderItem"/> <see cref="Style"/> instance assocaited with the specified <see cref="Windows.UI.Xaml.Controls.Pivot"/>
    /// </summary>
    /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.Pivot"/> to associated the <see cref="Style"/> instance to</param>
    /// <param name="value">The <see cref="Style"/> instance to bind to the <see cref="Windows.UI.Xaml.Controls.Pivot"/></param>
    #if UNO
    public static void SetPivotHeaderItemStyle(Microsoft.UI.Xaml.Controls.Pivot obj, Style value)
    #else
    public static void SetPivotHeaderItemStyle(Windows.UI.Xaml.Controls.Pivot obj, Style value)
    #endif
    {
        obj.SetValue(PivotHeaderItemStyleProperty, value);
    }

    /// <summary>
    /// Attached <see cref="DependencyProperty"/> for binding a <see cref="PivotHeaderItem"/> <see cref="Style"/> as an alternate template to a <see cref="Windows.UI.Xaml.Controls.Pivot"/>
    /// </summary>
    public static readonly DependencyProperty PivotHeaderItemStyleProperty =
        DependencyProperty.RegisterAttached("PivotHeaderItemStyle", typeof(Style), typeof(PivotExtensions), new PropertyMetadata(null, InitPivotStyle));

    /// <summary>
    /// Gets the Glyph attached property value.
    /// </summary>
    /// <param name="obj">PivotItem to get value from.</param>
    /// <returns>String value for FontIcon.</returns>
    public static string GetGlyph(PivotItem obj)
    {
        return (string)obj.GetValue(GlyphProperty);
    }

    /// <summary>
    /// Sets the attached property value for Glyph.
    /// </summary>
    /// <param name="obj">PivotItem to set value for.</param>
    /// <param name="value">String value for FontIcon.</param>
    public static void SetGlyph(PivotItem obj, string value)
    {
        obj.SetValue(GlyphProperty, value);
    }

    /// <summary>
    /// Used with the Pivot Styles to specify a FontIcon Glyph using the Segoe MDL2 Assets font.
    /// </summary>
    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.RegisterAttached("Glyph", typeof(string), typeof(PivotExtensions), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets the ImageSource attached property value.
    /// </summary>
    /// <param name="obj">PivotItem to get value from.</param>
    /// <returns>Resource to use for an Image in the MSEdgeTabStyle.</returns>
    public static ImageSource GetImageSource(PivotItem obj)
    {
        return (ImageSource)obj.GetValue(ImageSourceProperty);
    }

    /// <summary>
    /// Sets the attached property value for ImageSource.
    /// </summary>
    /// <param name="obj">PivotItem to set value for.</param>
    /// <param name="value">Resource to use for an Image in the MSEdgeTabStyle.</param>
    public static void SetImageSource(PivotItem obj, ImageSource value)
    {
        obj.SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Used with the Pivot Styles to specify an ImageSource for the MSEdgeTabStyle.
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.RegisterAttached("ImageSource", typeof(ImageSource), typeof(PivotExtensions), new PropertyMetadata(null));

    /// <summary>
    /// Gets the CloseButtonCommand attached property value.
    /// </summary>
    /// <param name="obj">Pivot to get value from.</param>
    /// <returns>Command value for the MSEdgeTabStyle Close Button.</returns>
    #if UNO
    public static ICommand GetCloseButtonCommand(Microsoft.UI.Xaml.Controls.Pivot obj)
    #else
    public static ICommand GetCloseButtonCommand(Windows.UI.Xaml.Controls.Pivot obj) // TODO: Should this be per PivotHeaderItem? Thinking no?
    #endif
    {
        return (ICommand)obj.GetValue(CloseButtonCommandProperty);
    }

    /// <summary>
    /// Sets the attached property value for CloseButtonCommand.
    /// </summary>
    /// <param name="obj">Pivot to set value for.</param>
    /// <param name="value">Command value for the MSEdgeTabStyle Close Button.</param>
    #if UNO
    public static void SetCloseButtonCommand(Microsoft.UI.Xaml.Controls.Pivot obj, ICommand value)
    #else
    public static void SetCloseButtonCommand(Windows.UI.Xaml.Controls.Pivot obj, ICommand value)
    #endif
    {
        obj.SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>
    /// Used with the MSEdgeTab Pivot Styles to specify a Close Command for the Tab's Close Button.
    /// </summary>
    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.RegisterAttached("CloseButtonCommand", typeof(ICommand), typeof(PivotExtensions), new PropertyMetadata(null));

    /// <summary>
    /// Gets the IsContentVisible attached property value.
    /// </summary>
    /// <param name="obj">Pivot to get value from.</param>
    /// <returns>False is content will be hidden.</returns>
    #if UNO
    public static bool GetIsContentVisible(Microsoft.UI.Xaml.Controls.Pivot obj)
    #else
    public static bool GetIsContentVisible(Windows.UI.Xaml.Controls.Pivot obj)
    #endif
    {
        return (bool)obj.GetValue(IsContentVisibleProperty);
    }

    /// <summary>
    /// Sets the attached property value for IsContentVisible.
    /// </summary>
    /// <param name="obj">Pivot to set value for.</param>
    /// <param name="value">Set to false to hide the PivotItemsPresenter.</param>
    #if UNO
    public static void SetIsContentVisible(Microsoft.UI.Xaml.Controls.Pivot obj, bool value)
    #else
    public static void SetIsContentVisible(Windows.UI.Xaml.Controls.Pivot obj, bool value)
    #endif
    {
        obj.SetValue(IsContentVisibleProperty, value);
    }

    /// <summary>
    /// Used to hide the content of a Pivot to just use the Pivot Header Items.
    /// </summary>
    public static readonly DependencyProperty IsContentVisibleProperty =
        DependencyProperty.RegisterAttached("IsContentVisible", typeof(bool), typeof(PivotExtensions), new PropertyMetadata(true, IsContentVisible_Changed));
}
