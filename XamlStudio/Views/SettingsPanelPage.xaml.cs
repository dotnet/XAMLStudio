using System;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    public sealed partial class SettingsPanelPage : Page
    {
        public SettingsPanelViewModel ViewModel { get; } = new SettingsPanelViewModel();

        //// TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere

        public SettingsPanelPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Initialize();
        }
    }
}
