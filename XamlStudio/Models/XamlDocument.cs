using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    [Bindable(true)]
    public enum DocumentType
    {
        Document,
        Welcome,
        Settings
    }

    public sealed class XamlDocument: SimpleObservable
    {
        /// <summary>
        /// Dummy for switching to Welcome Screen.
        /// </summary>
        private DocumentType _documentType;
        public DocumentType DocumentType
        {
            get { return _documentType; }
            set { Set(ref _documentType, value);  }
        }

        /// <summary>
        /// Text Contents of this Xaml Document.
        /// </summary>
        private string _content;
        public string Content
        {
            get { return _content; }
            set { Set(ref _content, value); }
        }

        /// <summary>
        /// File Title to Display in UI Tab.
        /// </summary>
        private string _title;
        public string Title
        {
            get { return (_dirty ? "*": "") + _title; }
            set { Set(ref _title, value.Trim('*')); }
        }

        private bool _dirty;
        public bool HasChanged
        {
            get { return _dirty; }
            set {
                Set(ref _dirty, value);
                OnPropertyChanged(nameof(Title)); // Update Title based on dirty flag
            }
        }

        /// <summary>
        /// Is this file actively visible/engaged in the UI.
        /// </summary>
        private bool _active;
        public bool IsActive
        {
            get { return _active; }
            set { Set(ref _active, value); }
        }

        public string StorageToken { get; set; }

        /// <summary>
        /// OS File backing this document.  Needed for Defer Updates, don't use.
        /// </summary>
        [JsonIgnore]
        internal StorageFile BackingFile { get; set; }

        [JsonIgnore]
        public string DisplayName { get { return BackingFile.DisplayName; } }

        public bool CanSave { get { return BackingFile != null; } }

        internal XamlDocument() { }

        public XamlDocument(string title)
        {
            this.Title = title;
        }

        private XamlDocument(StorageFile file)
        {
            this.BackingFile = file;

            // Should this be here vs. in a separate ViewModel stuff?
            if (string.IsNullOrWhiteSpace(StorageToken))
            {
                StorageToken = Guid.NewGuid().ToString();
            }

            StorageApplicationPermissions.FutureAccessList.AddOrReplace(StorageToken, BackingFile);
        }

        internal async Task RestoreFileAsync()
        {
            if (!string.IsNullOrWhiteSpace(StorageToken))
            {
                BackingFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(StorageToken);
            }
        }

        /// <summary>
        /// Save a file back to its backing location.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<bool> SaveAsync()
        {
            return SaveAsyncInternal().AsAsyncOperation();
        }

        private async Task<bool> SaveAsyncInternal()
        { 
            if (!CanSave)
            {
                throw new InvalidOperationException("Must Load or SaveAs before Save can be called.");
            }

            try
            {
                await FileIO.WriteTextAsync(this.BackingFile, this.Content);
            }
            catch (Exception)
            {
                return false;
            }

            // We made it here without an exception, assume write success, update flag.
            HasChanged = false;
            return true;
        }

        /// <summary>
        /// Save the file in a new location (or for the first time).
        /// 
        /// The document will now point to this new location (the old location will not be preserved).
        /// </summary>
        /// <param name="newfile">New File Storage Location.</param>
        /// <returns></returns>
        public IAsyncOperation<bool> SaveAsAsync(StorageFile newfile)
        {
            return SaveAsAsyncInternal(newfile).AsAsyncOperation();
        }

        private async Task<bool> SaveAsAsyncInternal(StorageFile newfile)
        {
            // Call save if this is the same file.
            if (BackingFile?.Equals(newfile) == true)
            {
                return await SaveAsyncInternal();
            }

            var original = this.BackingFile;
            this.BackingFile = newfile;

            if (await SaveAsyncInternal())
            {
                // Update Title after save.
                this.Title = newfile.DisplayName;

                // Save new token to access list.
                StorageToken = Guid.NewGuid().ToString();
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(StorageToken, BackingFile);

                return true;
            }
            else
            {
                // Restore original backing file.
                this.BackingFile = original;

                return false;
            }
        }

        /// <summary>
        /// Create a XamlDocument from an existing location.
        /// </summary>
        /// <param name="file">Storage Location.</param>
        /// <returns></returns>
        public static async Task<XamlDocument> LoadFromFileAsync(StorageFile file)
        {
            var document = new XamlDocument(file);

            var content = await FileIO.ReadTextAsync(file);

            document.Title = file.DisplayName;
            document.Content = content;

            return document;
        }

        public static XamlDocument WelcomeDocument()
        {
            return new XamlDocument("Welcome")
            {
                DocumentType = DocumentType.Welcome
            };
        }

        public static XamlDocument SettingsDocument()
        {
            return new XamlDocument("Settings")
            {
                DocumentType = DocumentType.Settings
            };
        }
    }
}
