using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Models;

[Bindable(true)]
public enum DocumentType
{
    Document,
    Welcome,
    Settings
}

public sealed partial class XamlDocument : FileBackedDocument
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
    [ObservableProperty]
    private DocumentType _documentType;

    /// <summary>
    /// Is this file actively visible/engaged in the UI.
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private DataContext _dataContext = new DataContext();

    // TODO: Figure out persistence strategy
    [property: JsonIgnore]
    [ObservableProperty]
    private StorageFolder _parentFolder;

    [ObservableProperty]
    private DocumentState _state = new DocumentState();

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
