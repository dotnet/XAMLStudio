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

        public event EventHandler Compiled;

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

        public ObservableVector<IModelDeltaDecoration> LineDecorations { get; } = new ObservableVector<IModelDeltaDecoration>();

        private CssLineStyle _errorStyle = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush("#FFFEB9CE".ToColor())
        };

        private CssLineStyle _bindingStyleUnbound = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush("#FFB4EBEF".ToColor())
        };

        private CssLineStyle _bindingStyleSuccess = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush("#FFB9FEC1".ToColor())
        };

        private CssLineStyle _bindingStyleError = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush("#FFFFF689".ToColor())
        };

        private ICommand _updateXaml;
        public ICommand UpdateXamlCommand
        {
            get
            {
                if (_updateXaml == null)
                {
                    _updateXaml = new RelayCommand<RoutedEventArgs>(UpdateXaml);
                }

                return _updateXaml;
            }
        }

        private ICommand _keyDownCommand;
        public ICommand KeyDownCommand
        {
            get
            {
                if (_keyDownCommand == null)
                {
                    _keyDownCommand = new RelayCommand<WebKeyEventArgs>(KeyDown);
                }

                return _keyDownCommand;
            }
        }

        public SettingsService Settings { get; } = SettingsService.Instance;

        public ICommand NavigateToLineCommand { get; internal set; }

        public ICommand InsertTextCommand { get; internal set; }

        public XamlRenderService XamlRenderer { get; } = new XamlRenderService();

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
            if (vm.XamlRoot.Children.FirstOrDefault() is FrameworkElement fwe)
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
        }
    }
}
