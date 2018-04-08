using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Future
{
    public static class Bind
    {
        private static ResourceLoader _resLoader = ResourceLoader.GetForCurrentView();

        public static string LocalizedString(this string resourceKey)
        {
            return _resLoader.GetString(resourceKey);
        }

        /*public static T Ternary<T>(bool expression, object trueObj, object falseObj)
        {
            return (T)(expression ? trueObj : falseObj);
        }*/

        public static bool And(bool value1, bool value2)
        {
            return value1 && value2;
        }

        public static bool Or(bool value1, bool value2)
        {
            return value1 || value2;
        }

        public static bool Not(bool value)
        {
            return !value;
        }

        public static Visibility NotVisible(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility Visible(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static int Add(int value, int plus)
        {
            return value + plus;
        }

        public static int Subtract(int value, int minus)
        {
            return value - minus;
        }

        public static double Multiply(double value, double factor)
        {
            return value * factor;
        }

        public static double Divide(double value, double factor)
        {
            return value / factor;
        }
    }
}
