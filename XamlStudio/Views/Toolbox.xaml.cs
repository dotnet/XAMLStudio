using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Toolbox : Page
    {
        public ToolboxViewModel ViewModel { get; } = new ToolboxViewModel();

        public MainViewModel MainViewModel { get; set; }

        public Toolbox()
        {
            this.InitializeComponent();

            ViewModel.Initialize();

            TypeList.Loaded += TypeList_Loaded;
        }

        private void TypeList_Loaded(object sender, RoutedEventArgs e)
        {
            ClearSelection();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var t = e.ClickedItem as Type;
            if (t != null && MainViewModel.ActiveDocumentViewModel != null)
            {
                // Insert tag into active document.
                var text = "<";
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

                // TODO: Include MainViewModel.ActiveDocumentViewModel?
                WeakReferenceMessenger.Default.Send<InsertTextMessage>(new(text));

                Analytics.TrackEvent("InsertCode", new Dictionary<string, string> {
                    { "Location", "Toolbox" },
                    { "Type", t.FullName },
                });
            }

            ClearSelection();
        }

        private void ClearSelection()
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                TypeList.SelectedItem = null;
            });
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                var lvi = fe.FindAscendant<ListViewItem>();
                if (lvi != null && lvi.Content is Type type)
                {
                    if (ViewModel.Favorites.Contains(type))
                    {
                        ViewModel.Favorites.Remove(type);

                        Analytics.TrackEvent("Toolbox_Favorite", new Dictionary<string, string> {
                            { "Operation", "Remove" },
                            { "Type", type.FullName },
                            { "Number", ViewModel.Favorites.Count.ToString() }
                        });
                    }
                    else
                    {
                        ViewModel.Favorites.Add(type);

                        Analytics.TrackEvent("Toolbox_Favorite", new Dictionary<string, string> {
                            { "Operation", "Add" },
                            { "Type", type.FullName },
                            { "Number", ViewModel.Favorites.Count.ToString() }
                        });
                    }
                }
            }

            ClearSelection();
        }

        private void HyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                var lvi = fe.FindAscendant<ListViewItem>();
                if (lvi != null && lvi.Content is Type type)
                {
                    var link = ViewModel.LibraryService.GetLinkForType(type);
                    if (!string.IsNullOrWhiteSpace(link))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Launcher.LaunchUriAsync(new Uri(link));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        Analytics.TrackEvent("Open_Docs", new Dictionary<string, string> {
                            { "Location", "Toolbox" },
                            { "Type", type.FullName },
                            { "Uri", link },
                        });
                    }
                }
            }

            ClearSelection();
        }

        //private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        //{
        //    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        //    {
        //        // TODO: After have placeholder delay to slow this down, add analytics on more complete query rather than each stroke?
        //    }
        //}
    }
}
