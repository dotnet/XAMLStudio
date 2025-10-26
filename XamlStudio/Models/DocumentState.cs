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
