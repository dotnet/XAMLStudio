using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace XamlStudio.Models;

public abstract partial class WorkspaceWindow : ObservableObject
{
    [ObservableProperty]
    private XamlDocument _activeFile;

    public event EventHandler<XamlDocument> ActiveFileChanged;

    [ObservableProperty]
    private string _openActivity = "EXPLORER";

    [ObservableProperty]
    private bool _isWorkspaceOpen;

    public ObservableCollection<FolderLocation> WorkspaceFolders { get; private set; } = new();

    public ObservableCollection<XamlDocument> OpenFiles { get; private set; } = new();

    public WorkspaceWindow()
    {
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
        WorkspaceFolders.Clear(); // TODO: Add support for multiples
        if (folder != null)
        {
            WorkspaceFolders.Add(new(folder));
            IsWorkspaceOpen = true;
        }
        else
        {
            IsWorkspaceOpen = false;
        }
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
        WeakReferenceMessenger.Default.Send<OpenActivityChangedMessage>(new(value));
    }
}
