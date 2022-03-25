using Collections.Generic;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Monaco.Editor;
using Monaco.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.ViewModels
{
    public partial class DocumentViewModel : Observable
    {
        private ThreadPoolTimer _autocompileTimer;

        public event EventHandler<XamlRenderResultContext> Compiled;

        private XamlDocument _document;
        public XamlDocument Document
        {
            get { return _document; }
            set {
                Set(ref _document, value);
                HasCompiled = false; // TODO: Reset compiled flag so we can re-render, probably want to cache elements previously rendered in XamlDocument, so we can re-add on document switch
            }
        }

        /// <summary>
        /// Selected Text in the Editor
        /// </summary>
        private string _selection;
        public string SelectedText
        {
            get { return _selection; }
            set { Set(ref _selection, value); }
        }

        private XamlRenderResultContext _result;
        public XamlRenderResultContext Result
        {
            get { return _result; }
            set { Set(ref _result, value); }
        }

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

        public bool HasCompiled { get; set; }

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

        public IAsyncCommand UpdateXamlCommand { get; internal set; }

        public ICommand ForceRefreshCommand { get; private set; }

        public ICommand KeyDownCommand { get; private set; }

        public IAsyncCommand RefreshLiveDataContextCommand { get; private set; }

        public ICommand ParseDataContextCommand { get; private set; }

        public ICommand RotatePaneOrientationCommand { get; private set; }

        public ICommand TogglePreviewThemeCommand { get; private set; }

        /// <summary>
        /// Text for error message when refreshing from a live data source.
        /// </summary>
        private string _liveDataContextRefreshError;
        public string LiveDataContextRefreshError
        {
            get { return _liveDataContextRefreshError; }
            set { Set(ref _liveDataContextRefreshError, value); }
        }

        public SettingsService Settings { get; } = SettingsService.Instance;

        public ICommand NavigateToLineCommand { get; internal set; }

        public ICommand InsertTextCommand { get; internal set; }

        public XamlRenderService XamlRenderer { get; } = new XamlRenderService();

        public MainViewModel MainViewModel { get; internal set; }

        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register(nameof(DataContext), typeof(object), typeof(DocumentViewModel), new PropertyMetadata(null, DataContextPropertyChanged));

        public static void DataContextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var vm = obj as DocumentViewModel;
            if (vm.XamlRoot != null && vm.XamlRoot.Children.FirstOrDefault() is FrameworkElement fwe)
            {
                fwe.DataContext = args.NewValue;
            }
        }

        public DocumentViewModel()
        {
            // Placeholder
            Result = new Toolkit.Models.XamlRenderResultContext(string.Empty);

            ////xamlRenderer.ImageRoot = SettingsService.Instance.SampleFolder;
            ////xamlRenderer.DataRoot = SettingsService.Instance.SampleFolder;

            //UpdateXamlCommand = new RelayCommand<RoutedEventArgs>(UpdateXaml);
            KeyDownCommand = new RelayCommand<WebKeyEventArgs>(KeyDown);
            ForceRefreshCommand = new RelayCommand<RoutedEventArgs>(ForceRefresh);
            RefreshLiveDataContextCommand = new AsyncRelayCommand<RoutedEventArgs>(RefreshLiveDataContext);
            ParseDataContextCommand = new RelayCommand<RoutedEventArgs>(ParseDataContext);
            RotatePaneOrientationCommand = new RelayCommand<RoutedEventArgs>(RotatePaneOrientation);
            TogglePreviewThemeCommand = new RelayCommand<RoutedEventArgs>(TogglePreviewTheme);
        }
    }
}
