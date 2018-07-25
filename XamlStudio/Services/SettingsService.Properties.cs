using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI;
using Windows.UI.Xaml.Media;
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

        [DefaultValue(0.8)]
        public double? AutoCompileDelay
        {
            get { return Get<double?>(); }
            set { Set(value); }
        }

        [DefaultValue(false)]
        public bool? IsCompileSelectionEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsPowerBindingDebuggingEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsContentUpdatedWithSuggested
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(false)]
        public bool? IsAlignmentGridEnabled
        {
            get { return Get<bool?>(); }
            set { Set(value); }
        }

        [DefaultValue(4d)]
        public double AlignmentGridHorizontalStep
        {
            get { return Get<double>(); }
            set { Set(value); }
        }

        [DefaultValue(4d)]
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

        [DefaultValue("ms-appx:///Strings/xmlns.json", LoadFromUri = true)]
        public List<XmlnsNamespace> KnownNamespaces
        {
            get { return Get<List<XmlnsNamespace>>(); }
            set { Set(value); }
        }

        public string DefaultWorkspaceFolderToken
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public StorageFolder DefaultWorkspaceFolder
        {
            get
            {
                var foldertoken = DefaultWorkspaceFolderToken;
                if (String.IsNullOrEmpty(foldertoken))
                {
                    return null;
                }

                return Task.Run(async () => {
                    try
                    {
                        return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(foldertoken);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }).Result;
            }
        }
    }
}
