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

        public static bool GreaterThan(int lhs, int rhs)
        {
            return lhs > rhs;
        }

        public static bool GreaterThan(double lhs, double rhs)
        {
            return lhs > rhs;
        }

        public static bool LessThan(int lhs, int rhs)
        {
            return lhs < rhs;
        }

        public static bool LessThan(double lhs, double rhs)
        {
            return lhs < rhs;
        }

        public static bool GreaterThanOrEqualTo(int lhs, int rhs)
        {
            return lhs >= rhs;
        }

        public static bool GreaterThanOrEqualTo(double lhs, double rhs)
        {
            return lhs >= rhs;
        }

        public static bool LessThanOrEqualTo(int lhs, int rhs)
        {
            return lhs <= rhs;
        }

        public static bool LessThanOrEqualTo(double lhs, double rhs)
        {
            return lhs <= rhs;
        }

        public static bool EqualTo(int lhs, int rhs)
        {
            return lhs == rhs;
        }

        public static bool EqualTo(double lhs, double rhs, double tolerance = 0.0)
        {
            return Math.Abs(lhs - rhs) <= tolerance;
        }

        public static bool NotEqualTo(int lhs, int rhs)
        {
            return lhs != rhs;
        }

        public static bool NotEqualTo(double lhs, double rhs, double tolerance = 0.0)
        {
            return !EqualTo(lhs, rhs, tolerance);
        }
    }
}
