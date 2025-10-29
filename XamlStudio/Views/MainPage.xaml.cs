using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.AppCenter.Analytics;
using Microsoft.Services.Store.Engagement;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Services;
using XamlStudio.ViewModels;

using MUXC = Microsoft.UI.Xaml.Controls;

namespace XamlStudio.Views
{
    public sealed partial class MainPage : Page, IFileOpener,
        IRecipient<OpenActivityChangedMessage>,
        IRecipient<KeyDownMessage>
    {
        public MainViewModel ViewModel { get; }

        private IStorageItem[] _filesToLoad;
        private SuspensionState _restoreState;
        private DateTime _sessionStart;
        private long? _activityTime = null;

        private bool _loaded = false;

        public MainPage()
        {
            ViewModel = new MainViewModel();

            InitializeComponent();

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            BackdropMaterial.SetApplyToRootOrPageBackground(this, true);

            Loaded += MainPage_Loaded;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            Singleton<SuspendAndResumeService>.Instance.OnBackgroundEntering += Instance_OnBackgroundEntering;

            WeakReferenceMessenger.Default.RegisterAll(this);

            // Set XAML element as a drag region.
            Window.Current.SetTitleBar(AppTitleBarSection);
            AppTitleTextBlock.Text = "AppPackageDisplayName".GetLocalized();
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls and set padding.
            ////LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(sender.SystemOverlayRightInset);
        }

        private void Instance_OnBackgroundEntering(object sender, OnBackgroundEnteringEventArgs e)
        {
            // Save State of Documents here
            e.SuspensionState.OpenFiles = ViewModel.OpenFiles.ToArray();
            e.SuspensionState.OpenWorkspaces = ViewModel.WorkspaceFolders.ToArray();

            // Save Open Drawer, null = closed.
            e.SuspensionState.OpenActivity = (NavMenu.SelectedItem as MUXC.NavigationViewItem)?.Tag?.ToString();

            if (_loaded && !e.IsOutsideSuspend)
            {
                var props = new Dictionary<string, string> {
                    { "NumberFiles", ViewModel.OpenFiles.Count.ToString() },
                    { "UnsavedFiles", ViewModel.OpenFiles.Count(file => file.HasChanged).ToString() },
                    { "WelcomeOpen", ViewModel.OpenFiles.Any(file => file.DocumentType == DocumentType.Welcome).ToString() },
                    { "SettingsOpen", ViewModel.OpenFiles.Any(file => file.DocumentType == DocumentType.Settings).ToString() },
                    { "Activity", e.SuspensionState.OpenActivity ?? "Closed" },
                    { "SessionTimeMin", ((TimeSpan)(DateTime.Now - _sessionStart)).TotalMinutes.ToString() }
                };
                Analytics.TrackEvent("Background", props);
            }
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var ctrl = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            var shift = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Receive(new KeyDownMessage(ctrl, shift, (int)args.VirtualKey));
        }

        /// <summary>
        /// Handle all global keyboard shortcuts.
        /// </summary>
        /// <param name="keyInfo"></param>
        public void Receive(KeyDownMessage keyInfo)
        {
            var active = false;
            if (keyInfo.Ctrl)
            {
                if (keyInfo.Shift)
                {
                    // Quick Shortcuts for Things
                    switch ((VirtualKey)keyInfo.KeyCode)
                    {
                        // Open Explorer
                        case VirtualKey.E:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute("EXPLORER");
                            break;
                        // Open Data Context
                        case VirtualKey.C:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute("DATASOURCES");
                            break;
                        // Open Binding Debugger
                        case VirtualKey.B:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute("DEBUG");
                            break;
                        // Live Properties
                        case VirtualKey.P:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute("PROPERTIES");
                            break;
                        // Open Toolbox
                        case VirtualKey.T:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute("TOOLBOX");
                            break;
                    }
                }
                else
                {
                    switch ((VirtualKey)keyInfo.KeyCode)
                    {
                        // Open Settings
                        case VirtualKey.I:
                            active = true;
                            ViewModel.OpenActivityPanelCommand.Execute(null);
                            break;
                        // New
                        case VirtualKey.N:
                            active = true;
                            ViewModel.NewDocumentCommand.Execute(null);
                            break;
                        // Open
                        case VirtualKey.O:
                            active = true;
                            ViewModel.OpenDocumentCommand.Execute(null);
                            break;
                        // Save
                        case VirtualKey.S:
                            if (keyInfo.Shift)
                            {
                                ViewModel.SaveDocumentAsCommand.Execute(ViewModel.ActiveFile);
                            }
                            else
                            {
                                ViewModel.SaveDocumentCommand.Execute(ViewModel.ActiveFile);
                            }
                            active = true;
                            break;
                        // Close
                        case VirtualKey.W:
                        case VirtualKey.F4:
                            active = true;
                            ViewModel.CloseActiveDocumentCommand.Execute(ViewModel.ActiveFile);
                            break;
                        // Prev/Next Document
                        case VirtualKey.Tab:
                            if (keyInfo.Shift)
                            {
                                active = true;
                                ViewModel.PreviousDocumentCommand.Execute(null);
                            }
                            else
                            {
                                active = true;
                                ViewModel.NextDocumentCommand.Execute(null);
                            }
                            break;
                    }
                }

                if (active)
                {
                    Analytics.TrackEvent("Key_Shortcut", new Dictionary<string, string>()
                    {
                        { "Location", "MainView" },
                        { "Action", active.ToString() },
                        { "Ctrl", keyInfo.Ctrl.ToString() },
                        { "Shift", keyInfo.Shift.ToString() },
                        { "Code", keyInfo.KeyCode.ToString() }
                    });
                }
            }

            keyInfo.Reply(active);
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Offload from main thread to parallelize assembly loading.
            Task t = new Task(async () =>
            {
                // TODO: Clean-up these initialize calls to make sure this list is centralized...
                await AppAssemblyInfo.Instance.InitializeAsync(new Assembly[] {
                    typeof(Microsoft.UI.Xaml.Controls.NavigationView).Assembly,
                    typeof(CommunityToolkit.WinUI.Controls.GridSplitter).Assembly,
                    typeof(CommunityToolkit.WinUI.Controls.DockPanel).Assembly,
                    typeof(CommunityToolkit.WinUI.Converters.BoolToVisibilityConverter).Assembly,
                    typeof(Microsoft.Xaml.Interactivity.DataTriggerBehavior).Assembly,
                });
            });
            t.Start();

            if (_filesToLoad != null)
            {
                // Remove Welcome Screen
                ViewModel.OpenFiles.Clear();

                //// TODO: Show Loading Ring?
            }

            await ViewModel.SettingsViewModel.Settings.InitializeAsync();

            if (_restoreState != null)
            {
                if (!string.IsNullOrWhiteSpace(_restoreState.OpenActivity))
                {
                    NavMenu.SelectedItem = NavMenu.MenuItems.FirstOrDefault(item => (item as MUXC.NavigationViewItem).Tag.ToString() == _restoreState.OpenActivity);
                }
                else
                {
                    NavMenu.SelectedItem = null;
                }

                if (_restoreState.OpenWorkspaces != null && _restoreState.OpenWorkspaces.Length > 0)
                {
                    foreach (var folderLocation in _restoreState.OpenWorkspaces)
                    {
                        await folderLocation.RestoreFolderAsync();
                        ViewModel.WorkspaceFolders.Add(folderLocation);
                    }
                    ViewModel.IsWorkspaceOpen = true;
                }

                if (_restoreState.OpenFiles != null && _restoreState.OpenFiles.Length > 0)
                {
                    if (_restoreState.FromRender)
                    {
                        // We encountered an error while rendering and crashed.
                        SettingsService.Instance.IsAutoCompileEnabled = false;
                    }

                    await ViewModel.RestoreWorkspaceAsync(_restoreState.OpenFiles);

                    if (!string.IsNullOrWhiteSpace(_restoreState.LastRenderedId))
                    {
                        var document = ViewModel.DocumentViewModels.Values.FirstOrDefault(doc => doc.Document.Id == _restoreState.LastRenderedId);
                        if (document != null)
                        {
                            ViewModel.ActiveFile = document.Document;

                            // Store/Retrieve error...
                            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("lastexception.json");
                            UnhandledException exception = null;
                            if (file != null)
                            {
                                var text = await FileIO.ReadTextAsync(file as StorageFile);
                                try
                                {
                                    exception = JsonConvert.DeserializeObject<UnhandledException>(text);
                                }
                                catch (Exception ex)
                                {
                                    exception = new($"Error Deserializing Last Exception Information:\n{text}", ex);
                                }

                                // TODO: Used to work, doesn't now?
                                document.Result.Errors.Add(new Toolkit.Models.XamlExceptionRange(exception?.Message, exception?.Exception, 1, 1, 1, 1));
                            }

                            var messageDialog = new MessageDialog(string.Format("MainPage_UnhandledException_Message".GetLocalized(), exception?.Message), "MainPage_UnhandledException_Title".GetLocalized());
                            messageDialog.Commands.Add(new UICommand("MainPage_UnhandledException_Continue".GetLocalized()));
                            var openfeedback = new UICommand("MainPage_UnhandledException_OpenFeedbackHub".GetLocalized());
                            messageDialog.Commands.Add(openfeedback);
                            messageDialog.DefaultCommandIndex = 1;
                            messageDialog.CancelCommandIndex = 0;

                            if (openfeedback.Equals(await messageDialog.ShowAsync()))
                            {
                                // This launcher is part of the Store Services SDK https://docs.microsoft.com/en-us/windows/uwp/monetize/microsoft-store-services-sdk
                                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                                {
                                    // 1809 'workaround' for now. BUG 19698552
                                    await Launcher.LaunchUriAsync(new Uri("windows-feedback:?contextid=143"));
                                }
                                else
                                {
                                    var launcher = StoreServicesFeedbackLauncher.GetDefault();
                                    await launcher.LaunchAsync(new Dictionary<string, string>()
                                    {
                                        { "error", exception?.Message ?? "Unknown Runtime Error" },
                                        { "stacktrace", exception?.Exception?.StackTrace?.ToString() ?? "" },
                                        { "xaml", document.Document.Content }
                                    });
                                }

                                Analytics.TrackEvent("Open_FeedbackHub", new Dictionary<string, string>()
                                {
                                    { "Location", "Restart" },
                                    { "error", exception?.Message ?? "Unknown Runtime Error" },
                                    { "stacktrace", exception?.Exception?.StackTrace?.ToString() ?? "" }
                                });
                            }
                        }
                    }

                    // Clean-up suspend state reference.
                    _restoreState = null;
                }
            }

            if (_filesToLoad != null)
            {
                // Load Files
                OpenFileItems(_filesToLoad);
                _filesToLoad = null;
            }

            _sessionStart = DateTime.Now;
            _loaded = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Code to handle case when activating during launch.
            base.OnNavigatedTo(e);

            // TODO: Handle both restoring workspace and opening files?
            if (e.Parameter is IStorageItem[] files)
            {
                _filesToLoad = files;
            }
            else if (e.Parameter is SuspensionState state)
            {
                _restoreState = state;
            }
        }

        public void OpenFileItems(IStorageItem[] files)
        {
            foreach (IStorageItem file in files)
            {
                if (file.IsOfType(StorageItemTypes.File))
                {
                    ViewModel.OpenFileCommand.Execute(file as StorageFile);
                }
            }
        }

        private void DocumentTabs_TabClosing(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            ViewModel.CloseActiveDocumentCommand.Execute(args.Item);
        }

        private void NavMenu_ItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer is MUXC.NavigationViewItem navitem)
            {
                if (navitem.Tag?.ToString() == "SETTINGS")
                {
                    ViewModel.OpenSettingsPageCommand.Execute(null);
                }
                else if (navitem.Tag?.ToString() == "CONTRIBUTE")
                {
                    _ = Launcher.LaunchUriAsync(new Uri("SettingsPanel_UsefulLinks_GitHub/NavigateUri".GetLocalized()));
                }
                // Close panel if we invoke the currently open activity (Note: NavigationView.SelectedItem is already set here, so compare against VM)
                else if (navitem.Tag?.ToString() == ViewModel.OpenActivity)
                {
                    sender.SelectedItem = null;
                }
            }
        }

        private void NavMenu_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (_loaded)
            {
                var dict = new Dictionary<string, string>();

                if (_activityTime != null)
                {
                    dict.Add("TimeOpenSec", Math.Round((DateTime.UtcNow.Ticks - _activityTime.Value) / 10000000d, 2).ToString());
                }

                if (NavMenu.SelectedItem == null)
                {
                    dict.Add("Name", "Closed");
                    _activityTime = null;
                }
                else
                {
                    dict.Add("Name", (args.SelectedItem as MUXC.NavigationViewItem)?.Tag.ToString());
                    _activityTime = DateTime.UtcNow.Ticks;
                }
                dict.Add("Previous", ViewModel.OpenActivity);
                Analytics.TrackEvent("Activity", dict);
            }
            else
            {
                if (args.SelectedItem != null)
                {
                    _activityTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    _activityTime = null;
                }
            }

            // Sync ViewModel
            if (ViewModel != null)
            {
                if (_activityTime == null)
                {
                    ViewModel.OpenActivity = null;
                }
                else
                {
                    ViewModel.OpenActivity = (NavMenu.SelectedItem as MUXC.NavigationViewItem).Tag.ToString();
                }
            }
        }

        public void Receive(OpenActivityChangedMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.NewActivity))
            {
                // Deselect
                NavMenu.SelectedItem = null;
                return;
            }

            // Sync from ViewModel to UI
            foreach (var item in NavMenu.MenuItems)
            {
                if (item is MUXC.NavigationViewItem lbi && lbi.Tag.ToString() == message.NewActivity)
                {
                    NavMenu.SelectedItem = item;

                    // Set Focus to Menu
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Normal, () =>
                    {
                        var content = ToolboxPresenter.FindDescendant<ContentPresenter>()?.Content as Control;
                        if (content != null)
                        {
                            content.Focus(FocusState.Keyboard);

                            // This seems to work better? need to test with proper TabIndex in toolbox pages...
                            var focusable = FocusManager.FindNextFocusableElement(FocusNavigationDirection.Next);
                            if (focusable is Control control)
                            {
                                control.Focus(FocusState.Keyboard);
                            }
                        }
                    });
                    break;
                }
            }
        }

        private void DocumentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Fix selection issue.
            if (DocumentTabs.SelectedItem == null && ViewModel.OpenFiles.Count >= 1)
            {
                ViewModel.ActiveFile = ViewModel.OpenFiles.First();
            }
        }

        public static Visibility IsVisibleIfDocument(DocumentType type) => type == DocumentType.Document ? Visibility.Visible : Visibility.Collapsed;
    }
}
