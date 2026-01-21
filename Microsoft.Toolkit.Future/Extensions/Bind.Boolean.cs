// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.WinUI.Extensions.Future;

public static partial class Bind
{
    public static bool And(bool value1, bool value2) => value1 && value2;

    public static bool Or(bool value1, bool value2) => value1 || value2;

    public static bool Not(bool value) => !value;

    public static bool GreaterThan(int lhs, int rhs) => lhs > rhs;

    public static bool GreaterThan(double lhs, double rhs) => lhs > rhs;

    public static bool LessThan(int lhs, int rhs) => lhs < rhs;

    public static bool LessThan(double lhs, double rhs) => lhs < rhs;

    public static bool GreaterThanOrEqualTo(int lhs, int rhs) => lhs >= rhs;

    public static bool GreaterThanOrEqualTo(double lhs, double rhs) => lhs >= rhs;

    public static bool LessThanOrEqualTo(int lhs, int rhs) => lhs <= rhs;

    public static bool LessThanOrEqualTo(double lhs, double rhs) => lhs <= rhs;

    public static bool EqualTo(int lhs, int rhs) => lhs == rhs;

    public static bool EqualTo(double lhs, double rhs, double tolerance = 0.0) => Math.Abs(lhs - rhs) <= tolerance;

    public static bool NotEqualTo(int lhs, int rhs) => lhs != rhs;

    public static bool NotEqualTo(double lhs, double rhs, double tolerance = 0.0) => !EqualTo(lhs, rhs, tolerance);
}
