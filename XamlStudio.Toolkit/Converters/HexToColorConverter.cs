using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Converters
{
    [Bindable]
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;

            if (value is string hex)
            {
                // Invalid
                if (!hex.StartsWith("#"))
                {
                    return null;
                }

                // Add alpha value if not present
                if (hex.Length == 7)
                {
                    hex = hex.Insert(1, "ff");
                }

                // #AARRGGBB
                byte a = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);

                return Color.FromArgb(a, r, g, b);
            }
            else if (value is Color color)
            {
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
            return Convert(value, targetType, parameter, language);
        }
    }
}
