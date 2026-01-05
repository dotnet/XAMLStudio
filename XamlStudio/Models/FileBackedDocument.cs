// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace XamlStudio.Models;

public abstract partial class FileBackedDocument : ObservableObject
{
    /// <summary>
    /// File Content.
    /// </summary>
    [ObservableProperty]
    public partial string Content { get; set; }

    /// <summary>
    /// File Title to Display in UI Tab.
    /// </summary>
    public string Title
    {
        get { return (HasChanged ? "*" : "") + field; }
        set { SetProperty(ref field, value.Trim('*')); }
    }

    //// TODO: This is effectively private, but needs to be serialized, investigate options when switching away from Newtonsoft
    public string StorageToken { get; set; }

    /// <summary>
    /// OS File backing this document.  Internal Needed for Defer Updates, don't use.
    /// </summary>
    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    public partial StorageFile BackingFile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool HasChanged { get; set; }

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
    /// <param name="newFile">New File Storage Location.</param>
    /// <returns></returns>
    public IAsyncOperation<bool> SaveAsAsync(StorageFile newFile)
    {
        return SaveAsAsyncInternal(newFile).AsAsyncOperation();
    }

    private async Task<bool> SaveAsAsyncInternal(StorageFile newFile)
    {
        // Call save if this is the same file.
        if (BackingFile?.Equals(newFile) == true)
        {
            return await SaveAsyncInternal();
        }

        var original = this.BackingFile;
        BackingFile = newFile;

        if (await SaveAsyncInternal())
        {
            // Update Title after save.
            Title = newFile.DisplayName;

            // Save new token to access list.
            StorageToken = Guid.NewGuid().ToString();
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(StorageToken, BackingFile);

            return true;
        }
        else
        {
            // Restore original backing file.
            BackingFile = original;

            return false;
        }
    }
}
