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
        }
    }
}
