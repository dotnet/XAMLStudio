using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlStudio.Models;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Services
{
    public class LibraryService
    {
        public static LibraryService Instance => Singleton<LibraryService>.Instance;

        public ObservableCollection<LibraryInfo> Libraries { get; private set; } = new ObservableCollection<LibraryInfo>();

        public Dictionary<string, LibraryInfo> LibrariesByNamespace { get; private set; }

        private readonly AsyncLock _initializeMutex = new AsyncLock();
        private bool isInitialized = false;

        public LibraryService()
        {
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeAsync();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task InitializeAsync()
        {
            using (await _initializeMutex.LockAsync())
            {
                if (!isInitialized)
                {
                    await AppAssemblyInfo.Instance.InitializeAsync();

                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Strings/libs.json"));

                    var text = await FileIO.ReadTextAsync(file);

                    JsonConvert.DeserializeObject<LibraryInfo[]>(text).ToList().ForEach(item => Libraries.Add(item));

                    LibrariesByNamespace = Libraries.ToDictionary(item => item.Namespace);

                    isInitialized = true;
                }
            }
        }

        public List<Type> GetTypesForNamespace(string ns)
        {
            var items = new List<Type>();

            if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(ns, out var types))
            {
                foreach (var t in types.Where(t => t.IsSubclassOf(typeof(DependencyObject))))
                {
                    items.Add(t);
                }
            }

            return items;
        }

        public string GetLinkForType(Type type, LibraryInfo info = null)
        {
            if (info != null || LibrariesByNamespace.TryGetValue(type.Namespace, out info))
            {
                return info.DocumentationRoot
                           .Replace("<typefull>", type.FullName)
                           .Replace("<type>", type.Name);
            }

            return string.Empty;
        }
    }
}
