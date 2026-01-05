// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Windows.UI.Xaml;

namespace XamlStudio.Models;

/// <summary>
/// Keeps track of options/elements specific to the document.
/// </summary>
public partial class DocumentState : ObservableObject
{
    /// <summary>
    /// Keeps track of the status between the document and the rendered preview.
    /// </summary>
    [JsonIgnore]
    [ObservableProperty]
    public partial SyncStatus RenderState { get; set; }

    [ObservableProperty]
    public partial PaneOrientation? PreviewOrientation { get; set; }

    [ObservableProperty]
    public partial ElementTheme? PreviewAreaTheme { get; set; }
}
