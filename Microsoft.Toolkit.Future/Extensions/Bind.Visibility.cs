// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using Windows.UI.Xaml;

namespace CommunityToolkit.WinUI.Extensions.Future;

public static partial class Bind
{
    public static Visibility AndV(bool value1, bool value2) => (value1 && value2) ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility OrV(bool value1, bool value2) => (value1 || value2) ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility NotV(bool value) => !value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility GreaterThanV(int lhs, int rhs) => lhs > rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility GreaterThanV(double lhs, double rhs) => lhs > rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility LessThanV(int lhs, int rhs) => lhs < rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility LessThanV(double lhs, double rhs) => lhs < rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility GreaterThanOrEqualToV(int lhs, int rhs) => lhs >= rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility GreaterThanOrEqualToV(double lhs, double rhs) => lhs >= rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility LessThanOrEqualToV(int lhs, int rhs) => lhs <= rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility LessThanOrEqualToV(double lhs, double rhs) => lhs <= rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility EqualToV(object lhs, object rhs) => lhs == rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility EqualToV(int lhs, int rhs) => lhs == rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility EqualToV(double lhs, double rhs, double tolerance = 0.0) => Math.Abs(lhs - rhs) <= tolerance ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility NotEqualToV(object lhs, object rhs) => lhs != rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility NotEqualToV(int lhs, int rhs) => lhs != rhs ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility NotEqualToV(double lhs, double rhs, double tolerance = 0.0) => !EqualTo(lhs, rhs, tolerance) ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility AnyV(ICollection? collection) => collection?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
}
