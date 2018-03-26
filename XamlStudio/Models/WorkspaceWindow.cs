using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    public class WorkspaceWindow: Observable
    {
        private XamlDocument _activeFile;
        public XamlDocument ActiveFile
        {
            get { return _activeFile; }
            set { Set(ref _activeFile, value); }
        }

        public bool IsWorkspaceOpen { get; private set; }

        // Holds the default folder or workspace folder location
        public StorageFolder Folder { get; private set; }

        public ObservableCollection<XamlDocument> OpenFiles { get; private set; }

        // Keep track of files opened from outside our pervue, as we'll need to tokenize these separately.
        public ObservableCollection<StorageFile> NonWorkspaceFiles { get; private set; }

        // Shortcuts
        public StorageFolder ImagesFolder { get; private set; } // TODO: Create if doesn't exist and item added?

        public StorageFolder DataFolder { get; private set; }

        public WorkspaceWindow()
        {
            OpenFiles = new ObservableCollection<XamlDocument>();
            NonWorkspaceFiles = new ObservableCollection<StorageFile>();

            var welcome = XamlDocument.WelcomeDocument();

            OpenFiles.Add(welcome);
            ActiveFile = welcome;
        }

        public static WorkspaceWindow GetDefaultWorkspace()
        {
            //SettingsService.Instance.DefaultWorkspaceFolder
            // Get Settings Folder
            // IsWorkspaceOpen = false still.
            throw new NotImplementedException();
        }
    }
}
