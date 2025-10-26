
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace XamlStudio.Models;

/// <summary>
/// Safe to serialize Folder location that abstracts the <see cref="StorageFolder"/> via the Future Access List.
/// Akin to <see cref="FileBackedDocument"/>.
/// </summary>
public partial class FolderLocation : ObservableObject
{
    //// TODO: This is effectively private, but needs to be serialized, investigate options when switching away from Newtonsoft
    public string StorageToken { get; set; }

    /// <summary>
    /// OS File backing this document.  Internal Needed for Defer Updates, don't use.
    /// </summary>
    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInitialized))]
    public partial StorageFolder BackingFolder { get; set; }

    public bool IsInitialized { get { return BackingFolder != null; } }

    public FolderLocation() { }

    public FolderLocation(StorageFolder file) : this()
    {
        BackingFolder = file;

        // Should this be here vs. in a separate ViewModel stuff?
        if (string.IsNullOrWhiteSpace(StorageToken))
        {
            StorageToken = Guid.NewGuid().ToString();
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace(StorageToken, BackingFolder);
    }

    internal async Task RestoreFolderAsync()
    {
        if (BackingFolder == null && !string.IsNullOrWhiteSpace(StorageToken))
        {
            try
            {
                BackingFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(StorageToken);
            }
            catch (Exception)
            {
                // Probably network/intermittent issue.
                // Ignore, we'll ask user to save-as if we can't restore again at that point.
            }
        }
    }
}
