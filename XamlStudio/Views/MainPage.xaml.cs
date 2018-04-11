using System;

using Windows.UI.Xaml.Controls;

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
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.SettingsViewModel.Settings.InitializeAndLoad();
        }
    }
}
