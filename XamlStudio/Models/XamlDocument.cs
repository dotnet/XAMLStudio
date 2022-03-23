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

    public sealed class XamlDocument: FileBackedDocument
    {
        private readonly string _id = Guid.NewGuid().ToString();

        /// <summary>
        /// Unique identifier for referencing across sessions.
        /// </summary>
        [JsonProperty]
        public string Id { get; private set; }

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
        /// Is this file actively visible/engaged in the UI.
        /// </summary>
        private bool _active;
        public bool IsActive
        {
            get { return _active; }
            set { Set(ref _active, value); }
        }

        private DataContext _dataContext = new DataContext();
        public DataContext DataContext
        {
            get { return _dataContext; }
            set { Set(ref _dataContext, value); }
        }

        // TODO: Figure out persistence strategy
        private StorageFolder _parentFolder;
        [JsonIgnore]
        public StorageFolder ParentFolder
        {
            get { return _parentFolder; }
            set { Set(ref _parentFolder, value); }
        }

        private DocumentState _docState = new DocumentState();
        public DocumentState State
        {
            get { return _docState; }
            set { Set(ref _docState, value); }
        }

        [JsonIgnore]
        public string DisplayName { get { return BackingFile.DisplayName; } }

        internal XamlDocument()
        {
            Initialize();
        }

        public XamlDocument(string title) : base(title)
        {
            Initialize();
        }

        public XamlDocument(StorageFile file) : base(file)
        {
            Initialize();
        }

        private void Initialize()
        {
            Id = _id; // for first set unless deserialized
        }

        public override string ToString()
        {
            return Title;
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
