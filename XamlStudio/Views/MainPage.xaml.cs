using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

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
            await ViewModel.SettingsViewModel.Settings.InitializeAndLoad();

            ViewModel.RegisterPropertyChangedCallback(WorkspaceWindow.ActiveFileProperty, (sender2, args) =>
            {
                DocumentTabsPivot.SelectedItem = ViewModel.ActiveFile;
            });
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Workaround for issue with binding to SelectedItem when closing tab.
            if (e.AddedItems.Count > 0)
            {
                var doc = e.AddedItems[0] as XamlDocument;
                if (doc != null && doc != ViewModel.ActiveFile)
                {
                    ViewModel.ActiveFile = doc;
                }
            }
        }
    }
}
