using Collections.Generic;
using Monaco.Editor;
using Monaco.Helpers;
using System;
using System.Windows.Input;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlStudio.Helpers;
using XamlStudio.Models;
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

        public bool HasCompiled { get; private set; }

        public Panel XamlRoot { get; set; }

        public ObservableVector<IModelDeltaDecoration> LineDecorations { get; } = new ObservableVector<IModelDeltaDecoration>();

        private CssLineStyle _errorStyle = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush(Colors.DarkRed)
        };

        private CssLineStyle _bindingStyleUnbound = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush(Colors.Indigo)
        };

        private CssLineStyle _bindingStyleSuccess = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush(Colors.LawnGreen)
        };

        private CssLineStyle _bindingStyleError = new CssLineStyle()
        {
            BackgroundColor = new SolidColorBrush(Colors.DarkRed)
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

        public XamlRenderService XamlRenderer { get; } = new XamlRenderService();

        public DocumentViewModel()
        {
            ////this.Document = document;

            ////xamlRenderer.ImageRoot = SettingsService.Instance.SampleFolder;
            ////xamlRenderer.DataRoot = SettingsService.Instance.SampleFolder;
        }
    }
}
