using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Future
{
    public static partial class Bind
    {
        public static Visibility AndV(bool value1, bool value2)
        {
            return (value1 && value2) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility OrV(bool value1, bool value2)
        {
            return (value1 || value2) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility NotV(bool value)
        {
            return !value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GreaterThanV(int lhs, int rhs)
        {
            return lhs > rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GreaterThanV(double lhs, double rhs)
        {
            return lhs > rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility LessThanV(int lhs, int rhs)
        {
            return lhs < rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility LessThanV(double lhs, double rhs)
        {
            return lhs < rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GreaterThanOrEqualToV(int lhs, int rhs)
        {
            return lhs >= rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GreaterThanOrEqualToV(double lhs, double rhs)
        {
            return lhs >= rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility LessThanOrEqualToV(int lhs, int rhs)
        {
            return lhs <= rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility LessThanOrEqualToV(double lhs, double rhs)
        {
            return lhs <= rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility EqualToV(int lhs, int rhs)
        {
            return lhs == rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility EqualToV(double lhs, double rhs, double tolerance = 0.0)
        {
            return Math.Abs(lhs - rhs) <= tolerance ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility NotEqualToV(int lhs, int rhs)
        {
            return lhs != rhs ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility NotEqualToV(double lhs, double rhs, double tolerance = 0.0)
        {
            return !EqualTo(lhs, rhs, tolerance) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
