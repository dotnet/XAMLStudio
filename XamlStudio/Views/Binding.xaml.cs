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

        public Binding()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var line = (e.ClickedItem as ConversionRecord)?.Parent.Line;

            if (line != null && line.HasValue)
            {
                MainViewModel.ActiveDocumentViewModel.NavigateToLineCommand.Execute(line.Value);
            }
        }
    }
}
