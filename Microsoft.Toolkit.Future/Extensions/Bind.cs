// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace CommunityToolkit.WinUI.Extensions.Future;

public static partial class Bind
{
    private static ResourceLoader _resLoader = ResourceLoader.GetForCurrentView();

    public static string LocalizedString(string resourceKey) => _resLoader.GetString(resourceKey);

    public static Visibility NotVisible(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility Visible(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
}
