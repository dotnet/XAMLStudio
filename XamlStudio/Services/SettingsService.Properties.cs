using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace XamlStudio.Services
{
    public partial class SettingsService
    {
        // Add Properties as Needed
        [DefaultValue(true)]
        public bool? IsAutoCompileEnabled
        {
            get { return Task.Run(() => Get<bool?>()).Result; }
            //get { return Get<bool?>().ConfigureAwait(false).GetAwaiter().GetResult(); }
            set { Set(value); }
        }

        [DefaultValue(0.7)]
        public double? AutoCompileDelay
        {
            get { return Task.Run(() => Get<double?>()).Result; }
            //get { return Get<double?>().ConfigureAwait(false).GetAwaiter().GetResult(); }
            set { Set(value); }
        }

        [DefaultValue(true)]
        public bool? IsPowerBindingDebuggingEnabled
        {
            get { return Task.Run(() => Get<bool?>()).Result; }
            //get { return Get<bool?>().ConfigureAwait(false).GetAwaiter().GetResult(); }
            set { Set(value); }
        }

        public string DefaultWorkspaceFolderToken
        {
            get { return Task.Run(() => Get<string>()).Result; }
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
