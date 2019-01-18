using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        private bool _isInitialized = false;

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
                if (!_isInitialized)
                {
                    // TODO: Clean-up these initialize calls to make sure this list is centralized... (MainPage, XamlRenderService)
                    await AppAssemblyInfo.Instance.InitializeAsync(new Assembly[] {
                        typeof(Microsoft.UI.Xaml.Controls.NavigationView).Assembly,
                        typeof(Microsoft.Toolkit.Uwp.UI.Controls.TabView).Assembly,
                        typeof(Microsoft.Toolkit.Uwp.UI.Controls.DataGrid).Assembly,
                        typeof(Microsoft.Toolkit.Uwp.UI.Converters.BoolToVisibilityConverter).Assembly,
                        typeof(Microsoft.Xaml.Interactions.Core.DataTriggerBehavior).Assembly,
                        typeof(Telerik.UI.Xaml.Controls.Input.RadAutoCompleteBox).Assembly,
                        typeof(Telerik.UI.Xaml.Controls.Primitives.RadExpanderControl).Assembly
                    });

                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Strings/libs.json"));

                    var text = await FileIO.ReadTextAsync(file);

                    JsonConvert.DeserializeObject<LibraryInfo[]>(text).ToList().ForEach(item => Libraries.Add(item));

                    LibrariesByNamespace = Libraries.ToDictionary(item => item.Namespace);

                    _isInitialized = true;
                }
            }
        }

        public List<Type> GetTypesForNamespace(string ns)
        {
            var items = new List<Type>();

            // TODO: Don't realy need this now anymore, but helps to optimize listing... thoughts?  May need specific doc link hints for Telerik
            if (LibrariesByNamespace.TryGetValue(ns, out LibraryInfo lib) && lib.TypeHints != null && lib.TypeHints.Count > 0)
            {
                foreach (var tname in lib.TypeHints)
                {
                    var t = Type.GetType(tname, false, false);
                    if (t != null)
                    {
                        items.Add(t);
                    }
                }
            }
            else if (AppAssemblyInfo.Instance.TypesByNamespace.TryGetValue(ns, out var types))
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
