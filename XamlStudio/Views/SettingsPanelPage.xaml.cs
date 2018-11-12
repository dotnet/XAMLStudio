using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Services.Logging;
using XamlStudio.Toolkit.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    public sealed partial class SettingsPanelPage : Page
    {
        public SettingsPanelViewModel ViewModel
        {
            get { return (SettingsPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(SettingsPanelViewModel), typeof(SettingsPanelPage), new PropertyMetadata(null));

        public MainViewModel MainViewModel
        {
            get { return (MainViewModel)GetValue(MainViewModelProperty); }
            set { SetValue(MainViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainViewModelProperty =
            DependencyProperty.Register(nameof(MainViewModel), typeof(MainViewModel), typeof(SettingsPanelPage), new PropertyMetadata(null, (sender, args) =>
            {
                SettingsPanelPage document = (sender as SettingsPanelPage);
                if (document != null)
                {
                    document.ViewModel = (args.NewValue as MainViewModel).SettingsViewModel;
                }
            }));

        //// TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere

        public SettingsPanelPage()
        {
            InitializeComponent();
        }

        private async void ButtonOpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(await FileLogger.Instance.GetAppLogFolderAsync());
        }

        private async void DataGrid_RowEditEnded(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridRowEditEndedEventArgs e)
        {
            // Save Namespaces after Edit.
            await ViewModel.Settings.SaveAsync(nameof(ViewModel.Settings.KnownNamespaces));
        }

        private void AddNamespaceButton_Click(object sender, RoutedEventArgs e)
        {
            // Add new Row and begin editing
            ViewModel.Settings.KnownNamespaces.Insert(0, new Toolkit.Models.XmlnsNamespace(string.Empty, string.Empty));

            NamespaceDataGrid.SelectedIndex = 0;

            NamespaceDataGrid.ScrollIntoView(NamespaceDataGrid.SelectedItem, null);

            NamespaceDataGrid.Focus(FocusState.Keyboard);

            NamespaceDataGrid.BeginEdit();
        }

        private async void RemoveNamespaceButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.CommandParameter != null &&
                btn.CommandParameter is XmlnsNamespace xns)
            {
                ViewModel.Settings.KnownNamespaces.Remove(xns);

                // Save Namespaces after Edit.
                await ViewModel.Settings.SaveAsync(nameof(ViewModel.Settings.KnownNamespaces));
            }
        }

        private void NamespaceDataGrid_PreparingCellForEdit(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is TextBox t)
            {
                t.Focus(FocusState.Keyboard);
            }
        }
    }
}
