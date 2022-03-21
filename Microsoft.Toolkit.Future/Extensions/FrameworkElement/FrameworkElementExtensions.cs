using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Extensions.Future
{
    public static class FrameworkElementExtensions
    {
        public static bool GetUpdateLiveRegionChangedOnVisible(FrameworkElement obj)
        {
            return (bool)obj.GetValue(UpdateLiveRegionChangedOnVisibleProperty);
        }

        public static void SetUpdateLiveRegionChangedOnVisible(FrameworkElement obj, bool value)
        {
            obj.SetValue(UpdateLiveRegionChangedOnVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for UpdateLiveRegionChangedOnVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateLiveRegionChangedOnVisibleProperty =
            DependencyProperty.RegisterAttached("UpdateLiveRegionChangedOnVisible", typeof(bool), typeof(FrameworkElementExtensions), new PropertyMetadata(false, UpdateLiveRegionChangedOnVisible_Changed));

        public static bool GetUpdateLiveRegionChangedOnLoad(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateLiveRegionChangedOnLoadProperty);
        }

        public static void SetUpdateLiveRegionChangedOnLoad(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateLiveRegionChangedOnLoadProperty, value);
        }

        // Using a DependencyProperty as the backing store for UpdateLiveRegionChangedOnLoad.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateLiveRegionChangedOnLoadProperty =
            DependencyProperty.RegisterAttached("UpdateLiveRegionChangedOnLoad", typeof(bool), typeof(FrameworkElementExtensions), new PropertyMetadata(false, UpdateLiveRegionChangedOnLoad_Changed));

        public static bool GetUpdateLiveRegionChildren(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateLiveRegionChildrenProperty);
        }

        public static void SetUpdateLiveRegionChildren(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateLiveRegionChildrenProperty, value);
        }

        // Using a DependencyProperty as the backing store for UpdateLiveRegionChildren.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateLiveRegionChildrenProperty =
            DependencyProperty.RegisterAttached("UpdateLiveRegionChildren", typeof(bool), typeof(FrameworkElementExtensions), new PropertyMetadata(true));

        private static void UpdateLiveRegionChangedOnVisible_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, Visibility_Changed);
            }
        }

        private static void UpdateLiveRegionChangedOnLoad_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FrameworkElement fe)
            {
                fe.Loaded -= FrameworkElement_Loaded;

                if (args.NewValue != null)
                {
                    if (fe.Parent != null)
                    {
                        FrameworkElement_Loaded(fe, null);
                    }
                    else
                    {
                        fe.Loaded += FrameworkElement_Loaded;
                    }
                }
            }
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                // Force Update
                Visibility_Changed(fe, null);
            }
        }

        private static void Visibility_Changed(DependencyObject sender, DependencyProperty dp)
        {
            if (sender is UIElement element && element.Visibility == Visibility.Visible)
            {
                if (AutomationProperties.GetLiveSetting(element) != AutomationLiveSetting.Off)
                {
                    FrameworkElementAutomationPeer.FromElement(element)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
                }

                // Also look for children underneath (as we would expect to read all visible items)
                // These still need independent AutomationProperties.LiveSetting set to be read.
                if (GetUpdateLiveRegionChildren(element))
                {
                    foreach (var child in element.FindDescendants().OfType<FrameworkElement>())
                    {
                        if (AutomationProperties.GetLiveSetting(child) != AutomationLiveSetting.Off)
                        {
                            FrameworkElementAutomationPeer.FromElement(child)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
                        }
                    }
                }
            }
        }
    }
}
