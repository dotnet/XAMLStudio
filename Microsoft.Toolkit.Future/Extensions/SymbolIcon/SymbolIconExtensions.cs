// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Future
{
    /// <summary>
    /// Set of extensions for the <see cref="NavigationView"/> control.
    /// </summary>
    [Bindable]
    public class SymbolIconExtensions
    {
        public static double GetFontSize(SymbolIcon obj)
        {
            return (double)obj.GetValue(FontSizeProperty);
        }

        public static void SetFontSize(SymbolIcon obj, double value)
        {
            obj.SetValue(FontSizeProperty, value);
        }

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
}