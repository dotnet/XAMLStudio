// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.WinUI.Extensions.Future;

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
