using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    public abstract class FileBackedDocument : SimpleObservable
    {
        /// <summary>
        /// File Content.
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
            get { return (HasChanged ? "*" : "") + _title; }
            set { Set(ref _title, value.Trim('*')); }
        }

        public string StorageToken { get; set; }

        /// <summary>
        /// OS File backing this document.  Internal Needed for Defer Updates, don't use.
        /// </summary>
        private StorageFile _backingFile;
        [JsonIgnore]
        internal StorageFile BackingFile
        {
            get { return _backingFile; }
            set
            {
                Set(ref _backingFile, value);
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private bool _dirty;
        public bool HasChanged
        {
            get { return _dirty; }
            set
            {
                Set(ref _dirty, value);
                OnPropertyChanged(nameof(Title)); // Update Title based on dirty flag
            }
        }

        public bool CanSave { get { return BackingFile != null; } }

        public FileBackedDocument() { }

        public FileBackedDocument(string title) : this()
        {
            this.Title = title;
        }

        protected FileBackedDocument(StorageFile file) : this()
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
            if (BackingFile == null && !string.IsNullOrWhiteSpace(StorageToken))
            {
                try
                {
                    BackingFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(StorageToken);
                }
                catch (Exception)
                {
                    // Probably network/intermittent issue.
                    // Ignore, we'll ask user to save-as if we can't restore again at that point.
                }
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
    }
}
