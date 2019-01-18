using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;

namespace XamlStudio.ViewModels
{
    public class SettingsPanelViewModel : Observable
    {
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

                            Analytics.TrackEvent("Settings_ChangeTheme", new Dictionary<string, string> {
                                { "Type", "App" },
                                { "Theme", "" + param },
                            });
                        });
                }

                return _switchThemeCommand;
            }
        }

        private ICommand _switchEditorThemeCommand;

        public ICommand SwitchEditorThemeCommand
        {
            get
            {
                if (_switchEditorThemeCommand == null)
                {
                    _switchEditorThemeCommand = new RelayCommand<ElementTheme>(
                        (param) =>
                        {
                            Settings.EditorTheme = param;

                            Analytics.TrackEvent("Settings_ChangeTheme", new Dictionary<string, string> {
                                { "Type", "Editor" },
                                { "Theme", "" + param },
                            });
                        });
                }

                return _switchEditorThemeCommand;
            }
        }

        public ObservableCollection<Color> Colors { get; set; }

        public SettingsService Settings { get; } = SettingsService.Instance;

        public ICommand SwitchToggleCommand { get; private set; }
        public ICommand DelayChangedCommand { get; private set; }

        public ObservableCollection<ThirdPartyInfo> ThirdPartyLibs { get; set; } = new ObservableCollection<ThirdPartyInfo>();

        public SettingsPanelViewModel()
        {
            SwitchToggleCommand = new RelayCommand<RoutedEventArgs>(SwitchToggle);
            DelayChangedCommand = new RelayCommand<RangeBaseValueChangedEventArgs>(DelayChanged);

            VersionDescription = GetVersionDescription();

            Colors = new ObservableCollection<Color>(typeof(Colors).GetRuntimeProperties().Select((color) => (Color)color.GetValue(null)));

            LoadThirdPartyInfo();
        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            if (version.Revision != 0)
            {
                return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            else
            {
                return $"v{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        private async void LoadThirdPartyInfo()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Strings/thirdparty.json"));

            var text = await FileIO.ReadTextAsync(file);

            JsonConvert.DeserializeObject<ThirdPartyInfo[]>(text).ToList().ForEach(item => ThirdPartyLibs.Add(item));
        }

        private void SwitchToggle(RoutedEventArgs args)
        {
            var toggle = (args.OriginalSource as ToggleSwitch);

            Settings.Set(toggle.IsOn, toggle.Tag as string);

            Analytics.TrackEvent("Settings_Toggle", new Dictionary<string, string>()
            {
                { "Setting", toggle.Tag as string },
                { "Value", toggle.IsOn.ToString() }
            });
        }

        private void DelayChanged(RangeBaseValueChangedEventArgs args)
        {
            Settings.AutoCompileDelay = args.NewValue;

            // TODO: Need to use a ThreadPoolTimer to wait for last change?
            ////Analytics.TrackEvent("Settings_CompileDelayChanged", new Dictionary<string, string> {
            ////    { "Value", "" + args.NewValue },
            ////});
        }
    }
}
