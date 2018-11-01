using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
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

    public sealed class XamlDocument: Observable
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
            get { return _title + (_dirty ? "*": ""); }
            set { Set(ref _title, value); }
        }

        private bool _dirty;
        public bool HasChanged
        {
            get { return _dirty; }
            set {
                Set(ref _dirty, value);
                OnPropertyChanged("Title"); // Update Title based on dirty flag
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

        /// <summary>
        /// OS File backing this document.
        /// </summary>
        public StorageFile BackingFile { get; internal set; }

        public bool CanSave { get { return this.BackingFile != null; } }

        public XamlDocument(string title)
        {
            this.Title = title;
        }

        private XamlDocument(StorageFile file)
        {
            this.BackingFile = file;
        }

        /// <summary>
        /// Save a file back to its backing location.
        /// </summary>
        /// <returns></returns>
        public IAsyncAction SaveAsync()
        {
            if (!CanSave)
            {
                throw new InvalidOperationException("Must Load or SaveAs before Save can be called.");
            }

            // TODO: Check result of Write!
            HasChanged = false;
            return FileIO.WriteTextAsync(this.BackingFile, this.Content);
        }
        
        /// <summary>
        /// Save the file in a new location (or for the first time).
        /// 
        /// The document will now point to this new location (the old location will not be preserved).
        /// </summary>
        /// <param name="newfile">New File Storage Location.</param>
        /// <returns></returns>
        public IAsyncAction SaveAsAsync(StorageFile newfile)
        {
            this.BackingFile = newfile;

            // Update Title after save.
            this.Title = newfile.DisplayName;

            return SaveAsync();
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
