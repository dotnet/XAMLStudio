using Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using CommunityToolkit.WinUI.Helpers;
using Monaco.Editor;
using Monaco.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.ViewModels;

public partial class DocumentViewModel : ObservableObject
{
    [ObservableProperty]
    public partial XamlDocument Document { get; set; }

    partial void OnDocumentChanged(XamlDocument value)
    {
        HasCompiled = false; // TODO: Reset compiled flag so we can re-render, probably want to cache elements previously rendered in XamlDocument, so we can re-add on document switch
    }

    /// <summary>
    /// Selected Text in the Editor
    /// </summary>
    [ObservableProperty]
    public partial string SelectedText { get; set; }

    /// <summary>
    /// Visual Element that has focus for design mode elements, either through user selection or caret navigation in editor.
    /// </summary>
    [ObservableProperty]
    public partial FrameworkElement HighlightedElement { get; set; }

    [ObservableProperty]
    public partial XamlRenderResultContext Result { get; set; }

    [ObservableProperty]
    public partial XamlXmlTreeCoordinator XamlCoordinator { get; set; } = new();

    private ObservableCollection<ConversionRecord> _bindingHistory = new ObservableCollection<ConversionRecord>();
    public AdvancedCollectionView BindingHistory
    {
        get
        {
            var acv = new AdvancedCollectionView(_bindingHistory);
            acv.SortDescriptions.Add(new SortDescription("TimeStamp", SortDirection.Descending));

            return acv;
        }
    }

    [ObservableProperty]
    public partial bool HasCompiled { get; set; }

    public Panel XamlRoot { get; set; }

    public ElementTheme ActualTheme { get; set; }

    public ObservableVector<IModelDeltaDecoration> LineDecorations { get; private set; } = new ObservableVector<IModelDeltaDecoration>();

    private static CssLineStyle _errorLineStyle = new CssLineStyle()
    {
        BackgroundColor = new SolidColorBrush("#FFCA416A".ToColor())
    };

    private static CssInlineStyle _errorStyle = new CssInlineStyle()
    {
        BackgroundColor = new SolidColorBrush("#FFCA416A".ToColor()),
        ForegroundColor = new SolidColorBrush("#FFFFFFFF".ToColor()),
        FontWeight = FontWeights.SemiBold
    };

    private static CssInlineStyle _bindingStyleUnbound = new CssInlineStyle()
    {
        BackgroundColor = new SolidColorBrush("#FFB4EBEF".ToColor()),
        ForegroundColor = new SolidColorBrush("#FF333333".ToColor())
    };

    private static CssInlineStyle _bindingStyleSuccess = new CssInlineStyle()
    {
        BackgroundColor = new SolidColorBrush("#FFB9FEC1".ToColor()),
        ForegroundColor = new SolidColorBrush("#FF333333".ToColor())
    };

    private static CssInlineStyle _bindingStyleError = new CssInlineStyle()
    {
        BackgroundColor = new SolidColorBrush("#FFFFF689".ToColor()),
        ForegroundColor = new SolidColorBrush("#FF663333".ToColor()),
        FontWeight = FontWeights.SemiBold
    };

    /// <summary>
    /// Text for error message when refreshing from a live data source.
    /// </summary>
    [ObservableProperty]
    public partial string LiveDataContextRefreshError { get; set; }

    public SettingsService Settings { get; } = SettingsService.Instance;

    public XamlRenderService XamlRenderer { get; } = new XamlRenderService();

    public MainViewModel MainViewModel { get; internal set; }

    [ObservableProperty]
    public partial object DataContext { get; set; }

    partial void OnDataContextChanged(object value)
    {
        if (XamlRoot != null && XamlRoot.Children.FirstOrDefault() is FrameworkElement fwe)
        {
            fwe.DataContext = value;
        }
    }

    public DocumentViewModel()
    {
        // Placeholder
        Result = new Toolkit.Models.XamlRenderResultContext(string.Empty);

        ////xamlRenderer.ImageRoot = SettingsService.Instance.SampleFolder;
        ////xamlRenderer.DataRoot = SettingsService.Instance.SampleFolder;
    }
}
