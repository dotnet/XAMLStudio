using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
using Windows.Web.Http;
using XamlStudio.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace XamlStudio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataSources : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public DataSources()
        {
            this.InitializeComponent();
        }

        private async void LiveDataSourceUri_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri = new Uri(LiveDataSourceUri.Text);

                var http = new HttpClient();

                var response = await http.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();

                MainViewModel.ActiveDocumentViewModel.DataContext = JsonConvert.DeserializeObject<ExpandoObject>(body);

                DataPayload.Text = body;

                // TODO: Just swap out root element's data context.
                // Re-render with new context.
                MainViewModel.ActiveDocumentViewModel.HasCompiled = false; // force
                MainViewModel.ActiveDocumentViewModel.UpdateXamlCommand.Execute(null);
            }
            catch (Exception e2)
            {
                DataPayload.Text = e2.Message;
            }            
        }
    }
}
