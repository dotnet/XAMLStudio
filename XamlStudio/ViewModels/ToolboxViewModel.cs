using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XamlStudio.Helpers;
using XamlStudio.Services;

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

        private IEnumerable<IGrouping<string, Type>> _groupedSource;

        public ToolboxViewModel()
        {
        }

        public async void Initialize()
        {
            // Ensure we've loaded
            await LibraryService.InitializeAsync();

            _groupedSource = LibraryService.Libraries.SelectMany(lib => LibraryService.GetTypesForNamespace(lib.Namespace)).OrderBy(api => api.Name).GroupBy(api => api.Namespace).OrderBy(group => group.Key);

            LibraryView.Source = _groupedSource;
        }

        public void Filter(string text)
        {
            LibraryView.Source = _groupedSource.SelectMany(group => group).Where(t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) || t.Namespace.Contains(text, StringComparison.OrdinalIgnoreCase) || t.BaseType.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).GroupBy(api => api.Namespace);
        }
    }
}
