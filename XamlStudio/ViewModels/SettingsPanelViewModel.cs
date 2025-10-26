using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using XamlStudio.Models;
using XamlStudio.Services;

namespace XamlStudio.ViewModels;

public partial class SettingsPanelViewModel : ObservableObject
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
    [ObservableProperty]
    private ElementTheme _elementTheme = ThemeSelectorService.Theme;


    [ObservableProperty]
    private string _versionDescription;

    [RelayCommand]
    public async Task SwitchTheme(ElementTheme param)
    {
        ElementTheme = param;

        await ThemeSelectorService.SetThemeAsync(param);

        Analytics.TrackEvent("Settings_ChangeTheme", new Dictionary<string, string> {
                            { "Type", "App" },
                            { "Theme", "" + param },
                        });
    }

    [RelayCommand]
    public void SwitchEditorTheme(ElementTheme param)
    {
        Settings.EditorTheme = param;

        Analytics.TrackEvent("Settings_ChangeTheme", new Dictionary<string, string> {
                            { "Type", "Editor" },
                            { "Theme", "" + param },
                        });
    }

    [RelayCommand]
    public void SwitchPaneOrientation(PaneOrientation param)
    {
        Settings.DefaultPreviewPanePosition = param;

        Analytics.TrackEvent("Settings_ChangePaneOrientation", new Dictionary<string, string> {
                            { "Type", "Personalization" },
                            { "Orientation", "" + param },
                        });
    }

    public ObservableCollection<Color> Colors { get; set; }

    public SettingsService Settings { get; } = SettingsService.Instance;

    public ObservableCollection<ThirdPartyInfo> ThirdPartyLibs { get; set; } = new ObservableCollection<ThirdPartyInfo>();

    public SettingsPanelViewModel()
    {
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

    [RelayCommand]
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

    [RelayCommand]
    private void DelayChanged(RangeBaseValueChangedEventArgs args)
    {
        Settings.AutoCompileDelay = args.NewValue;

        // TODO: Need to use a ThreadPoolTimer to wait for last change?
        ////Analytics.TrackEvent("Settings_CompileDelayChanged", new Dictionary<string, string> {
        ////    { "Value", "" + args.NewValue },
        ////});
    }
}
