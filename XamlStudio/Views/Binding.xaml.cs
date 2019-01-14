using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XamlStudio.Toolkit.Models;
using XamlStudio.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Binding : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public SettingsPanelViewModel SettingsViewModel { get; set; }

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
                MainViewModel.ActiveDocumentViewModel.NavigateToLineCommand.Execute(xbi.Line);
            }
        }

        private void Button_ClearHistoryFilter_Click(object sender, RoutedEventArgs e)
        {
            HistoryFilter = null;
        }
    }
}
