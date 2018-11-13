using Monaco;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using XamlStudio.ViewModels;

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

            DataContextJson.RegisterPropertyChangedCallback(CodeEditor.TextProperty, DataContextJson_TextChanged);
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

                // Update DataContext
                ////MainViewModel.ActiveDocumentViewModel.DataContext = JsonConvert.DeserializeObject<ExpandoObject>(body);

                DataContextJson.Text = body;
            }
            catch (Exception e2)
            {
                DataContextJson.Text = e2.Message;
            }            
        }

        private void DataContextJson_TextChanged(DependencyObject sender, DependencyProperty dp)
        {
            // TODO: Consolidate with XamlRender parsing method.
            // TODO: Consolidate with Document and it's auto-compile timer logic, go back to KeyDown?
            // TODO: Is there a better way to consolidate this logic?  Maybe array and loop?
            object result = null;
            try
            {
                result = JsonConvert.DeserializeObject<ExpandoObject>(DataContextJson.Text);
            }
            catch (Exception)
            {
            }

            if (result == null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<ExpandoObject>>(DataContextJson.Text);
                }
                catch (Exception)
                {
                }
            }

            if (result == null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<object>>(DataContextJson.Text);
                }
                catch (Exception)
                {
                }
            }

            if (result == null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<object>(DataContextJson.Text);
                }
                catch (Exception)
                {
                }
            }

            if (result != null)
            {
                // Update DataContext if we have something.
                MainViewModel.ActiveDocumentViewModel.DataContext = result;
            }
        }
    }
}
