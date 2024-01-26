using CommunityToolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml;

namespace XamlStudio.Models;

public partial class DocumentState : ObservableObject
{
    [ObservableProperty]
    private PaneOrientation? _previewOrientation;

    [ObservableProperty]
    private ElementTheme? _previewAreaTheme;
}
