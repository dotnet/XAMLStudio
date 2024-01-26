using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace XamlStudio.Models;

public abstract partial class WorkspaceWindow: ObservableObject
{
    [ObservableProperty]
    private XamlDocument _activeFile;

    public event EventHandler<XamlDocument> ActiveFileChanged;

    [ObservableProperty]
    private string _openActivity = "EXPLORER";

    [ObservableProperty]
    private bool _isWorkspaceOpen;

    // Holds the default folder or workspace folder location
    [ObservableProperty]
    private StorageFolder _workspaceFolder;

    public ObservableCollection<XamlDocument> OpenFiles { get; private set; }

    // Keep track of files opened from outside our pervue, as we'll need to tokenize these separately.
    public ObservableCollection<StorageFile> NonWorkspaceFiles { get; private set; }

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

    public void SetupWorkspace(StorageFolder folder)
    {
        WorkspaceFolder = folder;
        IsWorkspaceOpen = folder != null;
    }

    partial void OnActiveFileChanged(XamlDocument oldValue, XamlDocument newValue)
    {
        // Mark child XamlDocument as Active File
        if (oldValue != null)
        {
            oldValue.IsActive = false;
        }

        if (newValue != null)
        {
            newValue.IsActive = true;
        }

        ActiveFileChanged?.Invoke(this, newValue);
    }

    partial void OnOpenActivityChanged(string value)
    {
        WeakReferenceMessenger.Default.Send<OpenActivityChangedMessaged>(new(value));
    }
}
