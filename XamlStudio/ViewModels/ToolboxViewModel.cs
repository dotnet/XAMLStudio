using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.Toolkit.Future.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Data;
using XamlStudio.Helpers;
using XamlStudio.Services;

namespace XamlStudio.ViewModels;

public partial class ToolboxViewModel : ObservableObject
{
    public LibraryService LibraryService => LibraryService.Instance;

    // TODO: Need to implement grouping for AdvancedCollectionView in the toolkit so I can do both filtering and grouping!
    public CollectionViewSource LibraryView { get; } = new CollectionViewSource()
    {
        IsSourceGrouped = true
    };

    public ObservableCollection<Type> Favorites { get; } = new ObservableCollection<Type>();

    [ObservableProperty]
    private string _filter = string.Empty;

    private IEnumerable<IGrouping<string, Type>> _groupedSource;

    public ToolboxViewModel()
    {
    }

    private void Favorites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Save
        SettingsService.Instance.FavoriteTypes = Favorites.Select(t => t.AssemblyQualifiedName).ToList();

        OnFilterChanged(Filter);
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
                try
                {
                    var t = Type.GetType(typename, false, false);
                    if (t != null)
                    {
                        Favorites.Add(t);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        // Only listen for changes after loading.
        Favorites.CollectionChanged += Favorites_CollectionChanged;

        _groupedSource = LibraryService.Libraries
            .SelectMany(lib => LibraryService.GetTypesForNamespace(lib.Namespace))
            .OrderBy(api => api.Name)
            .ToGroup(api => api.Namespace)
            .OrderBy(group => group.Key);

        LibraryView.Source =
            _groupedSource.Prepend(
                Favorites
                .OrderBy(t => t.Name)
                .ToGroup("Toolbox_Favorites_Header".GetLocalized()));
    }

    partial void OnFilterChanged(string value)
    {
        LibraryView.Source = _groupedSource
            .SelectMany(group => group)
            .Where(t => t.Name.Contains(value, StringComparison.OrdinalIgnoreCase) || t.Namespace.Contains(value, StringComparison.OrdinalIgnoreCase) || t.BaseType.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
            .ToGroup(api => api.Namespace)
            .Prepend(
                Favorites
                .Where(t => t.Name.Contains(value, StringComparison.OrdinalIgnoreCase) || t.Namespace.Contains(value, StringComparison.OrdinalIgnoreCase) || t.BaseType.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.Name)
                .ToGroup("Toolbox_Favorites_Header".GetLocalized()));
    }
}
