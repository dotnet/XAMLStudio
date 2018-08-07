using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using XamlStudio.Helpers;
using XamlStudio.Services;

namespace XamlStudio.ViewModels
{
    public class SettingsPanelViewModel : Observable
    {
        public Visibility FeedbackLinkVisibility => Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported() ? Visibility.Visible : Visibility.Collapsed;

        private ICommand _launchFeedbackHubCommand;

        public ICommand LaunchFeedbackHubCommand
        {
            get
            {
                if (_launchFeedbackHubCommand == null)
                {
                    _launchFeedbackHubCommand = new RelayCommand(
                        async () =>
                        {
                            // This launcher is part of the Store Services SDK https://docs.microsoft.com/en-us/windows/uwp/monetize/microsoft-store-services-sdk
                            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                            await launcher.LaunchAsync();
                        });
                }

                return _launchFeedbackHubCommand;
            }
        }

        // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }
        
        public ObservableCollection<Color> Colors { get; set; }

        public SettingsService Settings { get; } = SettingsService.Instance;

        public ICommand SwitchToggleCommand { get; private set; }
        public ICommand DelayChangedCommand { get; private set; }

        public SettingsPanelViewModel()
        {
            SwitchToggleCommand = new RelayCommand<RoutedEventArgs>(SwitchToggle);
            DelayChangedCommand = new RelayCommand<RangeBaseValueChangedEventArgs>(DelayChanged);

            VersionDescription = GetVersionDescription();

            Colors = new ObservableCollection<Color>(typeof(Colors).GetRuntimeProperties().Select((color) => (Color)color.GetValue(null)));
        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void SwitchToggle(RoutedEventArgs args)
        {
            var toggle = (args.OriginalSource as ToggleSwitch);

            Settings.Set(toggle.IsOn, toggle.Tag as string);
        }

        private void DelayChanged(RangeBaseValueChangedEventArgs args)
        {
            Settings.AutoCompileDelay = args.NewValue;
        }
    }
}
