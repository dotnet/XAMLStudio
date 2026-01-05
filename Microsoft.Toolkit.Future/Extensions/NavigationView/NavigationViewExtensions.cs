// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Future
{
    /// <summary>
    /// Set of extensions for the <see cref="NavigationView"/> control.
    /// </summary>
    [Bindable]
    public class NavigationViewExtensions
    {
        // Name of Content area in NavigationView Template.
        private const string CONTENT_GRID = "ContentGrid";

        /// <summary>
        /// Gets the index of the selected <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <returns>The selected index.</returns>
        public static int GetSelectedIndex(NavigationView obj)
        {
            return (int)obj.GetValue(SelectedIndexProperty);
        }

        /// <summary>
        /// Sets the index of the selected <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <param name="value">The index to select.</param>
        public static void SetSelectedIndex(NavigationView obj, int value)
        {
            obj.SetValue(SelectedIndexProperty, value);
        }

        /// <summary>
        /// Attached <see cref="DependencyProperty"/> for binding the selected index of a <see cref="NavigationView"/>.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.RegisterAttached("SelectedIndex", typeof(int), typeof(NavigationViewExtensions), new PropertyMetadata(-1, OnSelectedIndexChanged));

        /// <summary>
        /// Gets a value representing if the settings page is selected for the <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <returns>True if the settings page is selected.</returns>
        public static bool GetIsSettingsSelected(NavigationView obj)
        {
            return (bool)obj.GetValue(IsSettingsSelectedProperty);
        }

        /// <summary>
        /// Sets a value representing if the settings page is selected for the <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <param name="value">Set to True to select the settings page.</param>
        public static void SetIsSettingsSelected(NavigationView obj, bool value)
        {
            obj.SetValue(IsSettingsSelectedProperty, value);
        }

        /// <summary>
        /// Attached <see cref="DependencyProperty"/> for selecting the Settings Page of a <see cref="NavigationView"/>.
        /// </summary>
        public static readonly DependencyProperty IsSettingsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSettingsSelected", typeof(bool), typeof(NavigationViewExtensions), new PropertyMetadata(false, OnIsSettingsSelectedChanged));

        /// <summary>
        /// Gets the behavior to collapse the content when clicking the already selected <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <returns>True if the feature is on.</returns>
        public static bool GetCollapseOnClick(NavigationView obj)
        {
            return (bool)obj.GetValue(CollapseOnClickProperty);
        }

        /// <summary>
        /// Sets the behavior to collapse the content when clicking the already selected <see cref="NavigationViewItem"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.NavigationView"/>.</param>
        /// <param name="value">True to turn on this feature.</param>
        public static void SetCollapseOnClick(NavigationView obj, bool value)
        {
            obj.SetValue(CollapseOnClickProperty, value);
        }

        /// <summary>
        /// Attached <see cref="DependencyProperty"/> for enabling the behavior to collapse the <see cref="NavigationView"/> content when the same selected item is invoked again (click or tap).
        /// </summary>
        public static readonly DependencyProperty CollapseOnClickProperty =
            DependencyProperty.RegisterAttached("CollapseOnClick", typeof(bool), typeof(NavigationViewExtensions), new PropertyMetadata(false, OnCollapseOnClickChanged));

        private static void OnCollapseOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // This should always be a NavigationView.
            var navview = (NavigationView)d;

            navview.ItemInvoked -= Navview_ItemInvoked;

            if ((bool?)e.NewValue == true)
            {
                // Listen for clicks on navigation items
                navview.ItemInvoked += Navview_ItemInvoked;
            }
            else
            {
                // Make sure we're visible if we toggle this off.
                var content = navview.FindDescendant(CONTENT_GRID);

                if (content != null)
                {
                    content.Visibility = Visibility.Visible;
                }
            }
        }

        private static void Navview_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var content = sender.FindDescendant(CONTENT_GRID);

            if (content != null)
            {
                // If we click the item we already have selected, we want to collapse our content
                /* Bug with NavView fires twice for Settings, so we can't use this logic until fixed...
                 * (GetIsSettingsSelected(sender) ?
                    args.InvokedItem == sender.SelectedItem :
                    args.InvokedItem.Equals(((NavigationViewItem)sender.SelectedItem).Content))
                 */
                if (sender.SelectedItem != null &&
                    args.InvokedItem.Equals(((NavigationViewItem)sender.SelectedItem).Content))
                {
                    // We need to dispatch this so the underlying selection event from our invoke processes.
                    // Otherwise, we just end up back where we started.  We don't care about waiting for this to finish.
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () =>
                    {
                        sender.SelectedItem = null;
                    });

                    content.Visibility = Visibility.Collapsed;
                }
                else
                {
                    content.Visibility = Visibility.Visible;
                }
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navview = (NavigationView)d;

            navview.Loaded -= Navview_Loaded;
            Navview_Loaded(d, null); // For changes
            navview.Loaded += Navview_Loaded;

            navview.SelectionChanged -= Obj_SelectionChanged;
            navview.SelectionChanged += Obj_SelectionChanged;
        }

        private static void OnIsSettingsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navview = (NavigationView)d;

            if (e.NewValue.Equals(true))
            {
                navview.SelectedItem = navview.SettingsItem;
            }
            else if (navview.SelectedItem == navview.SettingsItem)
            {
                navview.SelectedItem = null;
            }
        }

        private static void Navview_Loaded(object sender, RoutedEventArgs e)
        {
            var navview = (NavigationView)sender;

            int value = GetSelectedIndex(navview);

            if (value >= 0 && value < navview.MenuItems.Count)
            {
                // Only update if we need to.
                if (navview.SelectedItem == null || !navview.SelectedItem.Equals(navview.MenuItems[value] as NavigationViewItem))
                {
                    navview.SelectedItem = navview.MenuItems[value];
                }
            }
            else if (GetIsSettingsSelected(navview))
            {
                navview.SelectedItem = navview.SettingsItem;
            }
            else
            {
                navview.SelectedItem = null;
            }
        }

        private static void Obj_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // Store state of settings selected.
            SetIsSettingsSelected(sender, args.IsSettingsSelected);

            if (!args.IsSettingsSelected && args.SelectedItem != null)
            {
                var index = sender.MenuItems.IndexOf(args.SelectedItem);
                if (index != GetSelectedIndex(sender))
                {
                    SetSelectedIndex(sender, index);
                }
            }
            else
            {
                SetSelectedIndex(sender, -1);
            }
        }
    }
}