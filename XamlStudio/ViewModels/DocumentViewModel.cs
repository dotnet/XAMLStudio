using Collections.Generic;
using Monaco;
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
using XamlStudio.Services;

namespace XamlStudio.ViewModels
{
    public class DocumentViewModel : Observable
    {
        private ThreadPoolTimer _autocompileTimer;

        public event EventHandler Compiled;

        private XamlDocument _document;
        public XamlDocument Document
        {
            get { return _document; }
            set { Set(ref _document, value); }
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
                    ////_keyDownCommand = new RelayCommand<WebKeyEventArgs>(KeyDown);
                }

                return _keyDownCommand;
            }
        }

        ////private XamlRenderService xamlRenderer { get; } = new XamlRenderService();

        public DocumentViewModel()
        {
            ////this.Document = document;

            ////xamlRenderer.ImageRoot = SettingsService.Instance.SampleFolder;
            ////xamlRenderer.DataRoot = SettingsService.Instance.SampleFolder;
        }

        private async void UpdateXaml(RoutedEventArgs args)
        {
            // Check if nothing to do
            /*if (this.Document.Content == null || this.Document.Content.Length == 0 || HasCompiled)
            {
                return;
            }

            LineDecorations.Clear(); // Clear out old errors

            xamlRenderer.IsBindingDebuggingEnabled = SettingsService.Instance.IsPowerBindingDebuggingEnabled.Value;

            // Pre-parse            
            var content = Document.Content;

            UIElement element = await xamlRenderer.Render(content);

            if (element == null)
            {
                // Highlight Errors
                foreach (var error in xamlRenderer.Errors)
                {
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(error.StartLine, error.StartColumn, error.EndLine, error.EndColumn),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            ClassName = this._errorStyle,
                            HoverMessage = new string[]
                            {
                                error.Message
                            }
                        }));
                }
            }
            else
            {
                CreateBindingDecorations();
            }

            // Only Update if we have a new well-parsed element.
            if (element != null)
            {
                // Add element to main panel
                XamlRoot.Children.Clear();
                XamlRoot.Children.Add(element);
            }

            HasCompiled = true;
            Compiled?.Invoke(this, new EventArgs());*/
        }

        /*private void BindingUpdated(XamlBindingInfo binding, object newvalue)
        {
            LineDecorations.Clear();
            this.CreateBindingDecorations();
            Compiled?.Invoke(this, new EventArgs());
        }

        private void CreateBindingDecorations()
        {
            // Highlight Bindings
            foreach (var binding in XamlBindingWrapperManager.Instance.GetBindings(xamlRenderer.Id))
            {
                // TODO: Does monaco-editor support inplace updating?  If so, should look into plumbing that
                switch (binding.LastKnownBindingState)
                {
                    case XamlBindingInfo.XamlBindingState.NotBound:
                        LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                            new IModelDecorationOptions()
                            {
                                IsWholeLine = false,
                                ClassName = this._bindingStyleUnbound,
                                HoverMessage = new string[]
                                    {
                                        "Binding not Triggered Yet."
                                    }
                            }));
                        break;
                    case XamlBindingInfo.XamlBindingState.Successful:
                        LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                            new IModelDecorationOptions()
                            {
                                IsWholeLine = false,
                                ClassName = this._bindingStyleSuccess,
                                HoverMessage = new string[]
                                    {
                                        "Last Binding Value: " + binding.LastConvertedResultOrValue.ToString(),
                                        "Total Hit Count: " + binding.BindingCount
                                    }
                            }));
                        break;
                    case XamlBindingInfo.XamlBindingState.ConversionError:
                        LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                            new IModelDecorationOptions()
                            {
                                IsWholeLine = false,
                                ClassName = this._bindingStyleError,
                                HoverMessage = new string[]
                                    {
                                        binding.LastExceptionMessage
                                    }
                            }));
                        break;
                }

                // Register for changes
                binding.BindingUpdated -= BindingUpdated;
                binding.BindingUpdated += BindingUpdated;
            }
        }

        private static readonly int[] NonCharacterCodes = new int[] {
            // Modifier Keys
            16, 17, 18, 20, 91,
            // Esc / Page Keys / Home / End / Insert
            27, 33, 34, 35, 36, 45,
            // Arrow Keys
            37, 38, 39, 40,
            // Function Keys
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123
        };

        private void KeyDown(WebKeyEventArgs args)
        {
            // Handle Shortcuts.
            // Ctrl+Enter or F5 Update // TODO: Do we need this in the app handler too? (Thinking no)
            if ((args.KeyCode == 13 && args.CtrlKey) ||
                 args.KeyCode == 116)
            {
                UpdateXaml(null);

                // Eat key stroke
                args.Handled = true;
            } else if (args.CtrlKey)
            {
                // Need to duplicate this here from ShellViewModel as Control eats CoreWindow event.
                switch (args.KeyCode)
                {
                    case 78: // N
                        (Application.Current as App).ViewModel.NewDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 79: // O
                        (Application.Current as App).ViewModel.OpenDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 83: // S
                        (Application.Current as App).ViewModel.SaveDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 87: // W
                    case 115: // F4
                        (Application.Current as App).ViewModel.CloseDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 9: // TAB
                        if (args.ShiftKey)
                        {
                            (Application.Current as App).ViewModel.PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            (Application.Current as App).ViewModel.NextDocumentCommand.Execute(null);
                        }
                        args.Handled = true;
                        break;
                }
            }

            // Ignore as a change to the document if we handle it as a shortcut above or it's a special char.
            if (!args.Handled && Array.IndexOf(NonCharacterCodes, args.KeyCode) == -1)
            {
                // TODO: Filter out non-display characters or look for text change...
                this.Document.HasChanged = true; // Mark Dirty
                this.HasCompiled = false;

                // Setup Time for Auto-Compile
                if (SettingsService.Instance.IsAutoCompileEnabled.Value)
                {
                    this._autocompileTimer?.Cancel(); // Stop Old Timer
                                                      // Create Compile Timer
                    this._autocompileTimer = ThreadPoolTimer.CreateTimer(async (e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            UpdateXaml(null);
                        });
                    }, TimeSpan.FromSeconds(SettingsService.Instance.AutoCompileDelay.Value));
                }
            }
        }*/
    }
}
