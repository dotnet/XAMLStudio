using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    public sealed class DataContext : FileBackedDocument
    {
        /// <summary>
        /// Remote REST uri for a service returning json.
        /// </summary>
        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { Set(ref _uri, value); }
        }

        public bool IsRemote { get { return !string.IsNullOrWhiteSpace(_uri); } }

        internal DataContext() : base() { }

        public DataContext(string title) : base(title) { }

        public DataContext(StorageFile file) : base(file) { }

        public static async Task<DataContext> LoadFromFileAsync(StorageFile file)
        {
            var document = new DataContext(file);

            var content = await FileIO.ReadTextAsync(file);

            document.Title = file.DisplayName;
            document.Content = content;

            return document;
        }
    }
}
