using Microsoft.AppCenter.Analytics;
using Monaco;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataSources : Page
    {
        private ThreadPoolTimer _autocompileTimer;

        public MainViewModel MainViewModel { get; set; }

        public DataContext ActiveDataContext
        {
            get { return (DataContext)GetValue(ActiveDataContextProperty); }
            set { SetValue(ActiveDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveDataContextProperty =
            DependencyProperty.Register(nameof(ActiveDataContext), typeof(DataContext), typeof(DataSources), new PropertyMetadata(null));

        private XamlDocument _activeDocument;

        public DataSources()
        {
            this.InitializeComponent();

            Loaded += DataSources_Loaded;
        }

        private void DataSources_Loaded(object sender, RoutedEventArgs e)
        {
            ActiveDocumentViewModel_Changed(MainViewModel, null);

            MainViewModel.RegisterPropertyChangedCallback(MainViewModel.ActiveDocumentViewModelProperty, ActiveDocumentViewModel_Changed);
        }

        private void ActiveDocumentViewModel_Changed(DependencyObject sender, DependencyProperty dp)
        {
            // Update are shadow-copy based on MainViewModel
            _activeDocument = MainViewModel.ActiveDocumentViewModel.Document;
            ActiveDataContext = _activeDocument.DataContext;

            // Toggle showing the panel if we have a remote source, though not explicitly bound as we initially open it with no url.
            RemoteDataSourceButton.IsChecked = ActiveDataContext.IsRemote;
        }

        private void LiveDataSourceUri_LostFocus(object sender, RoutedEventArgs e)
        {
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Refresh after UI pass so we can get updated text.
                await MainViewModel.ActiveDocumentViewModel.RefreshLiveDataContextCommand.ExecuteAsync(null);

                // Update Data Context
                MainViewModel.ActiveDocumentViewModel.ParseDataContextCommand.Execute(null);

                Analytics.TrackEvent("DataSources_Url_LostFocus");
            });
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void SetDataContext(DataContext context)
        {
            _activeDocument.DataContext = context;

            // Update Data Context
            MainViewModel.ActiveDocumentViewModel.ParseDataContextCommand.Execute(null);

            ActiveDocumentViewModel_Changed(MainViewModel, null);
        }

        private void Clear_DataSource(object sender, RoutedEventArgs e)
        {
            // Setup a new Data Context to reset everything
            SetDataContext(new DataContext());

            MainViewModel.ActiveDocumentViewModel.DataContext = null;
            MainViewModel.ActiveDocumentViewModel.LiveDataContextRefreshError = null;

            Analytics.TrackEvent("DataSources_Clear");
        }

        private async void Open_DataSource(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".json");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var doc = await XamlStudio.Models.DataContext.LoadFromFileAsync(file);

                SetDataContext(doc);

                Analytics.TrackEvent("DataSources_Open");
            }
        }

        private async void Save_DataSource(object sender, RoutedEventArgs e)
        {
            StorageFile file = null;

            // Ensure if we can restore the file if it wasn't available on load
            await _activeDocument.DataContext.RestoreFileAsync();

            // Save As
            if (!_activeDocument.DataContext.CanSave)
            {
                file = await SaveFileDialog("New Document");
            }
            else
            {
                // Resave to Existing File
                file = _activeDocument.DataContext.BackingFile;
            }

            if (file != null)
            {
                await SaveFile(_activeDocument.DataContext, file);

                // Error detect?
            }
        }

        private static async Task<bool> SaveFile(DataContext document, StorageFile file)
        {
            // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(file);

            // Update/Save Document
            var result = await document.SaveAsAsync(file);

            // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
            // Completing updates may require Windows to ask for user input.
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                if (result)
                {
                    Analytics.TrackEvent("DataSources_Save", new Dictionary<string, string>()
                    {
                        { "Success", "True" }
                    });

                    return true;
                }
            }

            // Show error about saving
            var messageDialog = new MessageDialog(String.Format("Application_SaveError".GetLocalized(), document.Title.TrimEnd('*')));
            await messageDialog.ShowAsync();

            Analytics.TrackEvent("DataSources_Save", new Dictionary<string, string>()
            {
                { "Success", "False" }
            });

            return false;
        }

        private async Task<StorageFile> SaveFileDialog(string documentName)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("JavaScript Object Notation", new List<string>() { ".json" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = documentName;

            return await savePicker.PickSaveFileAsync();
        }

        private static readonly int[] NonCharacterCodes = new int[] {
            // Modifier Keys
            16, 17, 18, 20, 91,
            // Esc / Page Keys / Home / End / Insert
            27, 33, 34, 35, 36, 45,
            // Arrow Keys
            37, 38, 39, 40,
            // Function Keys
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123
        };

        // Consolidate with Document
        private void CodeEditor_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
        {
            // Handle Shortcuts. https://keycode.info/
            // Ctrl+Enter or F5 Update // TODO: Do we need this in the app handler too? (Thinking no)
            if ((args.KeyCode == 13 && args.CtrlKey) ||
                 args.KeyCode == 116)
            {
                MainViewModel.ActiveDocumentViewModel.ParseDataContextCommand.Execute(null);

                // Eat key stroke
                args.Handled = true;
            }
            else if (args.CtrlKey)
            {
                if (args.ShiftKey)
                {
                    switch (args.KeyCode)
                    {
                        // E - Open Explorer
                        case 69:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("EXPLORER");
                            break;
                        // C - Open Data Context
                        case 67:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("DATASOURCES");
                            break;
                        // B - Open Binding Debugger
                        case 66:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("DEBUG");
                            break;
                        // T - Open Toolbox
                        case 84:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("TOOLBOX");
                            break;
                    }
                }
                // Need to duplicate this here from ShellViewModel as Control eats CoreWindow event.
                switch (args.KeyCode)
                {
                    case 73: // I
                        MainViewModel.OpenSettingsCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 78: // N
                        MainViewModel.NewDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 79: // O
                        MainViewModel.OpenDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 83: // S
                        if (args.ShiftKey)
                        {
                            MainViewModel.SaveDocumentAsCommand.Execute(MainViewModel.ActiveFile);
                        }
                        else
                        {
                            MainViewModel.SaveDocumentCommand.Execute(MainViewModel.ActiveFile);
                        }
                        args.Handled = true;
                        break;
                    case 87: // W
                    case 115: // F4
                        MainViewModel.CloseActiveDocumentCommand.Execute(MainViewModel.ActiveFile);
                        args.Handled = true;
                        break;
                    case 9: // TAB
                        if (args.ShiftKey)
                        {
                            MainViewModel.PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            MainViewModel.NextDocumentCommand.Execute(null);
                        }
                        args.Handled = true;
                        break;
                }
            }

            if (args.Handled)
            {
                Analytics.TrackEvent("Key_Shortcut", new Dictionary<string, string>()
                {
                    { "Location", "DataSources" },
                    { "Action", args.Handled.ToString() },
                    { "Ctrl", args.CtrlKey.ToString() },
                    { "Shift", args.ShiftKey.ToString() },
                    { "Code", args.KeyCode.ToString() }
                });
            }

            // Ignore as a change to the document if we handle it as a shortcut above or it's a special char.
            if (!args.Handled && Array.IndexOf(NonCharacterCodes, args.KeyCode) == -1)
            {
                // TODO: Filter out non-display characters or look for text change...

                // Setup Time for Auto-Compile
                //if (SettingsService.Instance.IsAutoCompileEnabled.Value)
                //{
                    this._autocompileTimer?.Cancel(); // Stop Old Timer
                                                      // Create Compile Timer
                    this._autocompileTimer = ThreadPoolTimer.CreateTimer(async (e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            MainViewModel.ActiveDocumentViewModel.ParseDataContextCommand.Execute(null);
                        });
                    }, TimeSpan.FromSeconds(SettingsService.Instance.AutoCompileDelay.Value));
                //}
            }
        }

        private void RemoteDataSourceButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Clear Uri when closing Remote Data Source Mini-Panel.
            ActiveDataContext.Uri = null;
        }
    }
}
