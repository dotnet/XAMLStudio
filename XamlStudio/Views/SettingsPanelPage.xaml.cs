// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Helpers;
using Microsoft.AppCenter.Analytics;
using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services.Logging;
using XamlStudio.Toolkit.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Views;

public sealed partial class SettingsPanelPage : Page
{
    public SettingsPanelViewModel ViewModel
    {
        get { return (SettingsPanelViewModel)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SettingsPanelViewModel), typeof(SettingsPanelPage), new PropertyMetadata(null));

    public MainViewModel MainViewModel
    {
        get { return (MainViewModel)GetValue(MainViewModelProperty); }
        set { SetValue(MainViewModelProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MainViewModel.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MainViewModelProperty =
        DependencyProperty.Register(nameof(MainViewModel), typeof(MainViewModel), typeof(SettingsPanelPage), new PropertyMetadata(null, (sender, args) =>
        {
            SettingsPanelPage document = (sender as SettingsPanelPage);
            if (document != null && args.NewValue is MainViewModel mvm)
            {
                document.ViewModel = mvm.SettingsViewModel;
            }
        }));

    public bool IsAnalyticsOn
    {
        get { return (bool)GetValue(IsAnalyticsOnProperty); }
        set { SetValue(IsAnalyticsOnProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsAnalyticsOn.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsAnalyticsOnProperty =
        DependencyProperty.Register(nameof(IsAnalyticsOn), typeof(bool), typeof(SettingsPanelPage), new PropertyMetadata(true, (sender, args) =>
        {
            SettingsPanelPage document = (sender as SettingsPanelPage);
            bool value = (bool)args.NewValue;
            if (document != null)
            {
                // Do here to catch turning off.
                if (!value)
                {
                    Analytics.TrackEvent("Settings_Toggle", new Dictionary<string, string>()
                    {
                        { "Setting", "IsAnalyticsOn" },
                        { "Value", "False" }
                    });
                }

                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                // Try and wait to send event above before we disable analytics.
                ThreadPoolTimer.CreateTimer((e) =>
                {
                    dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, async () =>
                    {
                        await Analytics.SetEnabledAsync(value);

                        // Do here to catch turning on.
                        if (value)
                        {
                            Analytics.TrackEvent("Settings_Toggle", new Dictionary<string, string>()
                            {
                                { "Setting", "IsAnalyticsOn" },
                                { "Value", "True" }
                            });
                        }
                    });
                }, TimeSpan.FromSeconds(5)); // Need to wait a long time or the last tracking event will never be submitted
            }
        }));

    //// TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere

    public SettingsPanelPage()
    {
        InitializeComponent();

        // Get current Analytics setting
        DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Normal, async () =>
        {
            IsAnalyticsOn = await AreAnalyticsOn();
        });
    }

    private async void ButtonOpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchFolderAsync(await FileLogger.Instance.GetAppLogFolderAsync());

        Analytics.TrackEvent("Open_LogFolder", new Dictionary<string, string>()
        {
            { "Location", "About" },
        });
    }

    private async void DataGrid_RowEditEnded(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowEditEndedEventArgs e)
    {
        // Save Namespaces after Edit.
        await ViewModel.Settings.SaveAsync(nameof(ViewModel.Settings.KnownNamespaces));

        var ns = e.Row.DataContext as XmlnsNamespace;
        Analytics.TrackEvent("Settings_EditNamespaces", new Dictionary<string, string> {
            { "Location", "Toolbox" },
            { "Name", ns.Name },
            { "Path", ns.Path },
        });
    }

    private void AddNamespaceButton_Click(object sender, RoutedEventArgs e)
    {
        // Ensure grid is visible
        KnownNamespacesExpander.IsExpanded = true;

        // Add new Row and begin editing
        ViewModel.Settings.KnownNamespaces.Insert(0, new Toolkit.Models.XmlnsNamespace(string.Empty, string.Empty));

        _ = CompositionTargetHelper.ExecuteAfterCompositionRenderingAsync(() =>
        {
            NamespaceDataGrid.SelectedIndex = 0;

            NamespaceDataGrid.ScrollIntoView(NamespaceDataGrid.SelectedItem, null);

            NamespaceDataGrid.Focus(FocusState.Keyboard);

            NamespaceDataGrid.BeginEdit();
        });
    }

    private async void RemoveNamespaceButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn != null && btn.CommandParameter != null &&
            btn.CommandParameter is XmlnsNamespace xns)
        {
            ViewModel.Settings.KnownNamespaces.Remove(xns);

            // Save Namespaces after Edit.
            await ViewModel.Settings.SaveAsync(nameof(ViewModel.Settings.KnownNamespaces));

            Analytics.TrackEvent("Settings_RemoveNamespace", new Dictionary<string, string> {
                { "Location", "Toolbox" },
                { "Name", xns.Name },
                { "Path", xns.Path },
            });
        }
    }

    private void NamespaceDataGrid_PreparingCellForEdit(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridPreparingCellForEditEventArgs e)
    {
        if (e.EditingElement is TextBox t)
        {
            t.Focus(FocusState.Keyboard);
        }
    }

    private async Task<bool> AreAnalyticsOn() => await Analytics.IsEnabledAsync();

    public Visibility FeedbackVisibility => StoreServicesFeedbackLauncher.IsSupported() ? Visibility.Visible : Visibility.Collapsed;

    private async void HyperlinkButtonLicense_Click(object sender, RoutedEventArgs e)
    {
        var item = (sender as FrameworkElement).DataContext as ThirdPartyInfo;

        var md = new MessageDialog(string.Join("\n", item.LicenseText), string.Format("SettingsPanel_About_License_Dialog_Header".GetLocalized(), item.Name, item.License));

        await md.ShowAsync();
    }
}
