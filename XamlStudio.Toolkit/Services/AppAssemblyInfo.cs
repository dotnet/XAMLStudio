using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using XamlStudio.Toolkit.Helpers;

namespace XamlStudio.Toolkit.Services
{
    public class AppAssemblyInfo
    {
        public static AppAssemblyInfo Instance => Singleton<AppAssemblyInfo>.Instance;

        private readonly AsyncLock _mutex = new AsyncLock();
        public bool IsLoaded { get; private set; }

        public IReadOnlyList<Assembly> LoadedAssemblies { get; private set; }
        public IReadOnlyList<Type> KnownTypes { get; private set; }
        public IReadOnlyDictionary<string, ReadOnlyCollection<Type>> TypesByNamespace { get; private set; }

        public async Task InitializeAsync()
        {
            using (await _mutex.LockAsync())
            {
                if (!IsLoaded)
                {
                    await LoadAssembliesAsync();
                    FindTypes();
                    MapTypes();

                    IsLoaded = true;
                }
            }
        }

        /// <summary>
        /// Get all extra assemblies.
        /// </summary>
        /// <returns></returns>
        private async Task LoadAssembliesAsync()
        {
            var assemblies = new List<Assembly>
            {
                // Add Windows Assemblies
                typeof(FrameworkElement).GetTypeInfo().Assembly, // Windows.UI.Xaml
                typeof(Button).GetTypeInfo().Assembly, // Windows.UI.Xaml.Controls
                typeof(Brush).GetTypeInfo().Assembly, // Windows.UI.Xaml.Media
                typeof(Line).GetTypeInfo().Assembly // Windows.UI.Xaml.Shapes
            };

            // Add Other Assemblies
            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            LoadedAssemblies = assemblies.AsReadOnly();
        }

        /// <summary>
        /// Get all known types from loaded assemblies.
        /// </summary>
        /// <returns></returns>
        private void FindTypes()
        {
            var types = new List<Type>();
            foreach (var assem in LoadedAssemblies)
            {
                types.AddRange(assem.GetExportedTypes());
                ////types.AddRange(assem.GetTypes());
            }

            KnownTypes = types.AsReadOnly();
        }

        private void MapTypes()
        {
            var mapping = new Dictionary<string, List<Type>>();
            foreach (var type in KnownTypes)
            {
                if (type.Namespace == null)
                {
                    continue;
                }
                else if (!mapping.ContainsKey(type.Namespace))
                {
                    mapping[type.Namespace] = new List<Type>();
                }

                mapping[type.Namespace].Add(type);
            }

            // Make Read Only
            TypesByNamespace = new ReadOnlyDictionary<string, ReadOnlyCollection<Type>>(mapping.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly()));
        }
    }
}
