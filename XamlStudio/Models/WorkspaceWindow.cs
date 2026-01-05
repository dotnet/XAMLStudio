// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace XamlStudio.Models;

public abstract partial class WorkspaceWindow : ObservableObject
{
    [ObservableProperty]
    public partial XamlDocument ActiveFile { get; set; }

    public event EventHandler<XamlDocument> ActiveFileChanged;

    // TODO: Make enum?
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingDocument))]
    public partial string OpenActivity { get; set; } = "WELCOME";

    public bool IsEditingDocument => OpenActivity != "SETTINGS" && OpenActivity != "WELCOME";

    [ObservableProperty]
    public partial bool IsWorkspaceOpen { get; set; }

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
