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
        public static double Add(double value, double plus, double max = double.MaxValue, double min = double.MinValue)
        {
            var result = value + plus;

            if (max != double.MaxValue)
            {
                result = Math.Min(result, max);
            }

            if (min != double.MinValue)
            {
                result = Math.Max(result, min);
            }

            return result;
        }

        public static double Subtract(double value, double minus, double min = double.MinValue, double max = double.MaxValue)
        {
            var result = value - minus;

            if (min != double.MinValue)
            {
                result = Math.Max(result, min);
            }

            if (max != double.MaxValue)
            {
                result = Math.Min(result, max);
            }

            return result;
        }

        public static double Multiply(double value, double factor, double max = double.MaxValue, double min = double.MinValue)
        {
            var result = value * factor;

            if (max != double.MaxValue)
            {
                result = Math.Min(result, max);
            }

            if (min != double.MinValue)
            {
                result = Math.Max(result, min);
            }

            return result;
        }

        public static double Divide(double value, double factor)
        {
            return value / factor;
        }

        public static double Clamp(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}
