using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Services;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Toolbox : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public LibraryService LibraryService => LibraryService.Instance;
        
        public Toolbox()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var t = e.ClickedItem as Type;
            if (t != null)
            {
                var text = "<";
                // TODO: Insert tag into active document...
                var xmlns = SettingsService.Instance.KnownNamespaces.FirstOrDefault(ns => ns.Path.EndsWith(t.Namespace));
                if (xmlns != null)
                {
                    text += xmlns.Name + ":";
                }

                text += t.Name + ">\n</";

                if (xmlns != null)
                {
                    text += xmlns.Name + ":";
                }

                text += t.Name + ">";

                MainViewModel.ActiveDocumentViewModel.InsertTextCommand.Execute(text);
            }

            // Clear Selection
            var listview = sender as ListView;
            if (listview != null)
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    listview.SelectedItem = null;
                });
            }
        }

        private void HyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                var lvi = fe.FindAscendant<ListViewItem>();
                if (lvi != null && lvi.Content is Type type)
                {
                    var link = LibraryService.GetLinkForType(type);
                    if (!string.IsNullOrWhiteSpace(link))
                    {
                        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Launcher.LaunchUriAsync(new Uri(link));
                        #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
        }
    }
}
