using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    public abstract class WorkspaceWindow: Observable
    {
        public XamlDocument ActiveFile
        {
            get { return (XamlDocument)GetValue(ActiveFileProperty); }
            set { SetValue(ActiveFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveFileProperty =
            DependencyProperty.Register(nameof(ActiveFile), typeof(XamlDocument), typeof(WorkspaceWindow), new PropertyMetadata(null, ActiveFile_Changed));

        public string OpenActivity
        {
            get { return (string)GetValue(OpenActivityProperty); }
            set { SetValue(OpenActivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenActivity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenActivityProperty =
            DependencyProperty.Register(nameof(OpenActivity), typeof(string), typeof(WorkspaceWindow), new PropertyMetadata("EXPLORER"));

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

            Initialize();
        }

        public abstract void Initialize();

        public static WorkspaceWindow GetDefaultWorkspace()
        {
            //SettingsService.Instance.DefaultWorkspaceFolder
            // Get Settings Folder
            // IsWorkspaceOpen = false still.
            throw new NotImplementedException();
        }

        private static void ActiveFile_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Mark child XamlDocument as Active File
            if (e.OldValue != null && e.OldValue is XamlDocument xd)
            {
                xd.IsActive = false;
            }

            if (e.NewValue != null && e.NewValue is XamlDocument xd2)
            {
                xd2.IsActive = true;
            }
        }
    }
}
