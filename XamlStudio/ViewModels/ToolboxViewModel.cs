using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Future.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XamlStudio.Helpers;
using XamlStudio.Services;
using System.Collections.ObjectModel;

namespace XamlStudio.ViewModels
{
    public class ToolboxViewModel : Observable
    {
        public LibraryService LibraryService => LibraryService.Instance;

        // TODO: Need to implement grouping for AdvancedCollectionView in the toolkit so I can do both filtering and grouping!
        public CollectionViewSource LibraryView { get; } = new CollectionViewSource()
        {
            IsSourceGrouped = true
        };

        public ObservableCollection<Type> Favorites { get; } = new ObservableCollection<Type>();

        private string _filter = string.Empty;
        public string Filter
        {
            get { return _filter; }
            set {
                Set(ref _filter, value);
                FilterSource();
            }
        }

        private IEnumerable<IGrouping<string, Type>> _groupedSource;

        public ToolboxViewModel()
        {
        }

        private void Favorites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Save
            SettingsService.Instance.FavoriteTypes = Favorites.Select(t => t.AssemblyQualifiedName).ToList();

            FilterSource();
        }

        public async void Initialize()
        {
            // Ensure we've loaded
            await LibraryService.InitializeAsync();

            await SettingsService.Instance.InitializeAsync();

            // Load Favorites
            if (SettingsService.Instance.FavoriteTypes != null)
            {
                foreach (string typename in SettingsService.Instance.FavoriteTypes)
                {
                    var t = Type.GetType(typename, false, false);
                    if (t != null)
                    {
                        Favorites.Add(t);
                    }
                }
            }

            // Only listen for changes after loading.
            Favorites.CollectionChanged += Favorites_CollectionChanged;

            _groupedSource = LibraryService.Libraries
                .SelectMany(lib => LibraryService.GetTypesForNamespace(lib.Namespace))
                .OrderBy(api => api.Name)
                .GroupBy(api => api.Namespace)
                .OrderBy(group => group.Key);

            LibraryView.Source =
                _groupedSource.Prepend(
                    Favorites
                    .OrderBy(t => t.Name)
                    .ToGroup("Toolbox_Favorites_Header".GetLocalized()));
        }

        private void FilterSource()
        {
            LibraryView.Source = _groupedSource
                .SelectMany(group => group)
                .Where(t => t.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase) || t.Namespace.Contains(_filter, StringComparison.OrdinalIgnoreCase) || t.BaseType.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                .GroupBy(api => api.Namespace)
                .Prepend(
                    Favorites
                    .Where(t => t.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase) || t.Namespace.Contains(_filter, StringComparison.OrdinalIgnoreCase) || t.BaseType.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Name)
                    .ToGroup("Toolbox_Favorites_Header".GetLocalized()));
        }
    }
}
