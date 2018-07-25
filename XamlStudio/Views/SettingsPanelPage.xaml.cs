using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    public sealed partial class SettingsPanelPage : Page
    {
        public SettingsPanelViewModel ViewModel { get; private set; }

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
                if (document != null)
                {
                    document.ViewModel = (args.NewValue as MainViewModel).SettingsViewModel;
                }
            }));

        //// TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere

        public SettingsPanelPage()
        {
            InitializeComponent();
        }
    }
}
