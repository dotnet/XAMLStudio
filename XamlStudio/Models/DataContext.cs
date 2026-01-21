// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    public partial string Uri { get; set; }

    public bool IsRemote => !string.IsNullOrWhiteSpace(Uri);

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
