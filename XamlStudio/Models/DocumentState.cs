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
    [property: JsonIgnore]
    [ObservableProperty]
    private SyncStatus _renderState;

    [ObservableProperty]
    private PaneOrientation? _previewOrientation;

    [ObservableProperty]
    private ElementTheme? _previewAreaTheme;
}
