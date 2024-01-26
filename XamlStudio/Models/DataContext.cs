using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Models;

public sealed partial class DataContext : FileBackedDocument
{
    /// <summary>
    /// Remote REST uri for a service returning json.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRemote))]
    private string _uri;

    public bool IsRemote { get { return !string.IsNullOrWhiteSpace(_uri); } }

    internal DataContext() : base() { }

    public DataContext(string title) : base(title) { }

    public DataContext(StorageFile file) : base(file) { }

    public static async Task<DataContext> LoadFromFileAsync(StorageFile file)
    {
        var document = new DataContext(file);

        var content = await FileIO.ReadTextAsync(file);

        document.Title = file.DisplayName;
        document.Content = content;

        return document;
    }
}
