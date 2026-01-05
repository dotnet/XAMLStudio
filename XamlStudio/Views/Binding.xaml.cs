// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Models;
using XamlStudio.Toolkit.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Binding : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public XamlBindingInfo HistoryFilter
        {
            get { return (XamlBindingInfo)GetValue(HistoryFilterProperty); }
            set { SetValue(HistoryFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HistoryFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HistoryFilterProperty =
            DependencyProperty.Register(nameof(HistoryFilter), typeof(XamlBindingInfo), typeof(Binding), new PropertyMetadata(null));

        public Binding()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            XamlBindingInfo xbi = null;

            if (e.ClickedItem is ConversionRecord cr)
            {
                xbi = cr.Parent;
            }
            else if (e.ClickedItem is XamlBindingInfo bi)
            {
                xbi = bi;

                HistoryFilter = xbi;

                BindingViewMode.SelectedValue = "History";
            }

            if (xbi != null)
            {
                // TODO: Include MainViewModel.ActiveDocumentViewModel?
                WeakReferenceMessenger.Default.Send<NavigateToLineMessage>(new(xbi.Line));
            }
        }

        private void Button_ClearHistoryFilter_Click(object sender, RoutedEventArgs e)
        {
            HistoryFilter = null;
        }
    }
}
