// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// From: 

using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace XamlStudio.Helpers;

internal class NavItemIconHelper
{
    public static object GetSelectedIcon(NavigationViewItem obj) => obj.GetValue(SelectedIconProperty);

    public static void SetSelectedIcon(NavigationViewItem obj, object value) => obj.SetValue(SelectedIconProperty, value);

    public static readonly DependencyProperty SelectedIconProperty =
        DependencyProperty.RegisterAttached("SelectedIcon", typeof(object), typeof(NavItemIconHelper), new PropertyMetadata(null));

    /// <summary>
    /// Gets the value of <see cref="ShowNotificationDotProperty" /> for a <see cref="DependencyObject" />
    /// </summary>
    /// <returns>Returns a boolean indicating whether the notification dot should be shown.</returns>
    public static bool GetShowNotificationDot(NavigationViewItem obj) => (bool)obj.GetValue(ShowNotificationDotProperty);

    /// <summary>
    /// Sets <see cref="ShowNotificationDotProperty" /> on a <see cref="DependencyObject" />
    /// </summary>
    public static void SetShowNotificationDot(NavigationViewItem obj, bool value) => obj.SetValue(ShowNotificationDotProperty, value);

    /// <summary>
    /// An attached property that sets whether or not a notification dot should be shown on an associated <see cref="NavigationViewItem" />
    /// </summary>
    public static readonly DependencyProperty ShowNotificationDotProperty =
        DependencyProperty.RegisterAttached("ShowNotificationDot", typeof(bool), typeof(NavItemIconHelper), new PropertyMetadata(false));

    /// <summary>
    /// Gets the value of <see cref="UnselectedIconProperty"/> for a <see cref="DependencyObject"/>
    /// </summary>
    /// <returns>Returns the unselected icon as an object.</returns>
    public static object GetUnselectedIcon(NavigationViewItem obj) => (object)obj.GetValue(UnselectedIconProperty);

    /// <summary>
    /// Sets the value of <see cref="UnselectedIconProperty"/> for a <see cref="DependencyObject"/>
    /// </summary>
    public static void SetUnselectedIcon(NavigationViewItem obj, object value) => obj.SetValue(UnselectedIconProperty, value);

    /// <summary>
    /// An attached property that sets the unselected icon on an associated <see cref="NavigationViewItem" />
    /// </summary>
    public static readonly DependencyProperty UnselectedIconProperty =
        DependencyProperty.RegisterAttached("UnselectedIcon", typeof(object), typeof(NavItemIconHelper), new PropertyMetadata(null));

    public static Visibility GetStaticIconVisibility(NavigationViewItem obj) => (Visibility)obj.GetValue(StaticIconVisibilityProperty);

    public static void SetStaticIconVisibility(NavigationViewItem obj, Visibility value) => obj.SetValue(StaticIconVisibilityProperty, value);

    /// <summary>
    /// An attached property that sets the visibility of the static icon in the associated <see cref="NavigationViewItem"/>.
    /// </summary>
    public static readonly DependencyProperty StaticIconVisibilityProperty =
        DependencyProperty.RegisterAttached("StaticIconVisibility", typeof(Visibility), typeof(NavItemIconHelper), new PropertyMetadata(Visibility.Collapsed));
}
