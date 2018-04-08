using System;
using System.Windows.Input;

using Windows.ApplicationModel;
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

        public SettingsService Settings { get; } = SettingsService.Instance;

        public ICommand SwitchPowerBindingCommand { get; private set; }
        public ICommand SwitchAutoCompileCommand { get; private set; }
        public ICommand DelayChangedCommand { get; private set; }

        public SettingsPanelViewModel()
        {
            SwitchAutoCompileCommand = new RelayCommand<RoutedEventArgs>(SwitchAutoCompile);
            SwitchPowerBindingCommand = new RelayCommand<RoutedEventArgs>(SwitchPowerBinding);
            DelayChangedCommand = new RelayCommand<RangeBaseValueChangedEventArgs>(DelayChanged);

            VersionDescription = GetVersionDescription();
        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private void SwitchAutoCompile(RoutedEventArgs args)
        {
            Settings.IsAutoCompileEnabled = (args.OriginalSource as ToggleSwitch).IsOn;
        }

        private void SwitchPowerBinding(RoutedEventArgs args)
        {
            Settings.IsPowerBindingDebuggingEnabled = (args.OriginalSource as ToggleSwitch).IsOn;
        }

        private void DelayChanged(RangeBaseValueChangedEventArgs args)
        {
            Settings.AutoCompileDelay = args.NewValue;
        }
    }
}
