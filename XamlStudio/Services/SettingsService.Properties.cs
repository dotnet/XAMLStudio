using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using XamlStudio.Models;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Services
{
    public partial class SettingsService
    {
        // Add Properties as Needed
        [DefaultValue(true)]
        public bool? IsAutoCompileEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(1.0)]
        public double? AutoCompileDelay
        {
            get { return Get<double?>(); }
            set { Set(value); }
        }

        ////[DefaultValue(false)]
        ////public bool? IsCompileSelectionEnabled
        ////{
        ////    get { return Get<bool?>(); }
        ////    set { Set(value); }
        ////}

        [DefaultValue(false)]
        public bool? IsPowerBindingDebuggingEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(false)]
        public bool? IsContentUpdatedWithSuggested
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(false)]
        public bool? IsLiveDataContextRefreshedOnRender
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue("#00FFFFFF")] // Transparent
        public string PreviewAreaBackgroundColor
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsPreviewClippingEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsAlignmentGridEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(24d)]
        public double AlignmentGridHorizontalStep
        {
            get { return Get<double>(); }
            set { Set(value); }
        }

        [DefaultValue(24d)]
        public double AlignmentGridVerticalStep
        {
            get { return Get<double>(); }
            set { Set(value); }
        }

        [DefaultValue(0.1d)]
        public double AlignmentGridOpacity
        {
            get { return Get<double>(); }
            set { Set(value); }
        }

        [DefaultValue("#FF5F9EA0")] // CadetBlue
        public string AlignmentGridColor
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [DefaultValue(ElementTheme.Default)]
        public ElementTheme EditorTheme
        {
            get { return Get<ElementTheme>(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsPreviewDocked
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [DefaultValue(PaneOrientation.HorizontalPreviewTop)]
        public PaneOrientation DefaultPreviewPanePosition
        {
            get { return Get<PaneOrientation>(); }
            set { Set(value); }
        }

        [DefaultValue("ms-appx:///Strings/xmlns.json", LoadFromUri = true)]
        public ObservableCollection<XmlnsNamespace> KnownNamespaces
        {
            get { return Get<ObservableCollection<XmlnsNamespace>>(); }
            set { Set(value); }
        }

        public List<string> FavoriteTypes
        {
            get { return Get<List<string>>(); }
            set { Set(value); }
        }

        ////public string DefaultWorkspaceFolderToken
        ////{
        ////    get { return Get<string>(); }
        ////    set { Set(value); }
        ////}

        ////public StorageFolder DefaultWorkspaceFolder
        ////{
        ////    get
        ////    {
        ////        var foldertoken = DefaultWorkspaceFolderToken;
        ////        if (String.IsNullOrEmpty(foldertoken))
        ////        {
        ////            return null;
        ////        }

        ////        return Task.Run(async () => {
        ////            try
        ////            {
        ////                return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(foldertoken);
        ////            }
        ////            catch (Exception)
        ////            {
        ////                return null;
        ////            }
        ////        }).Result;
        ////    }
        ////}
    }
}
