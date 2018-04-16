using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Converters
{
    [Bindable]
    public sealed class StringToUriConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                Uri result;

                if (Uri.TryCreate(value as string, UriKind.RelativeOrAbsolute, out result))
                {
                    return result;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
