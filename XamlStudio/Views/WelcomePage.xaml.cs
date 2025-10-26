using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Services;
using XamlStudio.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : Page
    {
        public MainViewModel MainViewModel
        {
            get { return (MainViewModel)GetValue(MainViewModelProperty); }
            set { SetValue(MainViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainViewModelProperty =
            DependencyProperty.Register("MainViewModel", typeof(MainViewModel), typeof(WelcomePage), new PropertyMetadata(null));

        public ObservableCollection<StorageFile> RecentFiles { get; set; } = new ObservableCollection<StorageFile>();

        public WelcomePage()
        {
            this.InitializeComponent();

            Loaded += WelcomePage_Loaded;

            SettingsService.Instance.RecentFilesChanged += Instance_RecentFilesChanged;
        }

        private async void WelcomePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (RecentFiles.Count == 0)
            {
                foreach (var file in await SettingsService.Instance.GetRecentFilesAsync(5))
                {
                    RecentFiles.Add(file);
                }
            }
        }

        private void Instance_RecentFilesChanged(object sender, StorageFile file)
        {
            if (RecentFiles.Contains(file))
            {
                // Remove existing file, as we'll move it to the top now.
                RecentFiles.Remove(file);
            }
            else if (RecentFiles.Count == 5)
            {
                // Only want 5 so take off bottom.
                RecentFiles.RemoveAt(4);
            }

            // Insert touched file at top
            RecentFiles.Insert(0, file);
        }
    }
}
