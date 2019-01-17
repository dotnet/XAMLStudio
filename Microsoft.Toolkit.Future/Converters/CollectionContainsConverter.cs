using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Future.Converters
{
    [Bindable]
    public class CollectionContainsConverter : DependencyObject, IValueConverter // BoolToObjectConverter???
    {
        public IEnumerable<object> Collection
        {
            get { return (IEnumerable<object>)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Collection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(IEnumerable<object>), typeof(CollectionContainsConverter), new PropertyMetadata(Array.Empty<object>()));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (bool.TryParse(parameter as string, out bool result))
            {
                return Collection?.Contains(value) == true ? Visibility.Collapsed : Visibility.Visible;
            }

            return Collection?.Contains(value) == true ? Visibility.Visible : Visibility.Collapsed;
            //return base.Convert(Collection?.Contains(value), targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
