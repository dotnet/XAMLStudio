using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Services;
using XamlStudio.ViewModels;
using Windows.UI.Xaml.Controls.Primitives;
using System.Reflection;

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

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            ViewModel.KeyDownCommand.Execute(args);
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Offload from main thread to parallelize assembly loading.
            Task t = new Task(async () =>
            {
                await AppAssemblyInfo.Instance.InitializeAsync(new Assembly[] {
                    typeof(Microsoft.UI.Xaml.Controls.NavigationView).Assembly
                });
            });
            t.Start();

            if (_filesToLoad != null)
            {
                // Remove Welcome Screen
                ViewModel.OpenFiles.Clear();

                //// TODO: Show Loading Ring?
            }

            await ViewModel.SettingsViewModel.Settings.InitializeAndLoad();

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

        private void DocumentTabs_TabClosing(object sender, Microsoft.Toolkit.Uwp.UI.Controls.TabClosingEventArgs e)
        {
            ViewModel.CloseActiveDocumentCommand.Execute(e.Item);

            e.Cancel = true; // We'll remove item ourselves from collection when we're done in command, so don't have the TabView do it for us.
        }
    }
}
