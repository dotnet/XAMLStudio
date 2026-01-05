// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Models;

public sealed partial class XamlDocument : FileBackedDocument
{
    private readonly string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// Unique identifier for referencing across sessions.
    /// </summary>
    [JsonProperty]
    public string Id { get; private set; }

    /// <summary>
    /// Is this file actively visible/engaged in the UI.
    /// </summary>
    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial DataContext DataContext { get; set; } = new DataContext();

    // TODO: Figure out persistence strategy
    [JsonIgnore]
    [ObservableProperty]
    public partial StorageFolder ParentFolder { get; set; }

    [ObservableProperty]
    public partial DocumentState State { get; set; } = new DocumentState();

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
}
