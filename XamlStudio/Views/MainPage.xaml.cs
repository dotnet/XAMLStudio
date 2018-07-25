using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    public sealed partial class MainPage : Page, IFileOpener
    {
        public MainViewModel ViewModel { get; }

        private IStorageItem[] _filesToLoad;

        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            Loaded += MainPage_Loaded;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown; ;
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            ViewModel.KeyDownCommand.Execute(args);
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_filesToLoad != null)
            {
                // Remove Welcome Screen
                ViewModel.OpenFiles.Clear();

                //// TODO: Show Loading Ring?
            }

            await ViewModel.SettingsViewModel.Settings.InitializeAndLoad();

            ViewModel.RegisterPropertyChangedCallback(WorkspaceWindow.ActiveFileProperty, (sender2, args) =>
            {
                DocumentTabsPivot.SelectedItem = ViewModel.ActiveFile;
            });

            if (_filesToLoad != null)
            {
                // Load Files
                OpenFileItems(_filesToLoad);
                _filesToLoad = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Code to handle case when activating during launch.
            base.OnNavigatedTo(e);

            if (e.Parameter is IStorageItem[] files)
            {
                _filesToLoad = files;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Workaround for issue with binding to SelectedItem when closing tab.
            if (e.AddedItems.Count > 0)
            {
                XamlDocument doc = e.AddedItems[0] as XamlDocument;
                if (doc != null && doc != ViewModel.ActiveFile)
                {
                    ViewModel.ActiveFile = doc;
                }
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            XamlDocument settings = ViewModel.OpenFiles.FirstOrDefault(f => f.DocumentType == DocumentType.Settings);
            if (settings != null)
            {
                ViewModel.ActiveFile = settings;
            }
            else
            {
                ViewModel.OpenFiles.Add(XamlDocument.SettingsDocument());
                ViewModel.ActiveFile = ViewModel.OpenFiles.Last();
            }
        }
    }
}
