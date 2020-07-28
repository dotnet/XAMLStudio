using Microsoft.AppCenter.Analytics;
using Microsoft.Graphics.Canvas;
using Monaco;
using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Services;
using XamlStudio.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XamlStudio.Views
{
    public sealed partial class Document : UserControl
    {
        private string[] _decorations = Array.Empty<string>();

        private Type _lastHoverType;

        private object _initializeLock = new object();

        public MainViewModel MainViewModel
        {
            get { return (MainViewModel)GetValue(MainViewModelProperty); }
            set { SetValue(MainViewModelProperty, value); }
        }

        public static readonly DependencyProperty MainViewModelProperty =
            DependencyProperty.Register(nameof(MainViewModel), typeof(MainViewModel), typeof(Document), new PropertyMetadata(null, (sender, args) =>
            {
                var document = (sender as Document);
                lock (document._initializeLock)
                {
                    if (document != null && document.MainViewModel != null &&
                        document.LoadedDocument != null && document.ViewModel == null)
                    {
                        // Get ViewModel from MainViewModel creation.
                        document.InitializeViewModel(document.MainViewModel.DocumentViewModels[document.LoadedDocument]);
                    }
                }
            }));

        public DocumentViewModel ViewModel
        {
            get { return (DocumentViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DocumentViewModel), typeof(Document), new PropertyMetadata(null));

        public XamlDocument LoadedDocument
        {
            get { return (XamlDocument)GetValue(LoadedDocumentProperty); }
            set { SetValue(LoadedDocumentProperty, value); }
        }

        public static readonly DependencyProperty LoadedDocumentProperty =
            DependencyProperty.Register(nameof(LoadedDocument), typeof(XamlDocument), typeof(Document), new PropertyMetadata(null, (sender, args) =>
            {
                var document = (sender as Document);
                lock (document._initializeLock)
                {
                    if (document != null && document.MainViewModel != null &&
                        document.ViewModel == null)
                    {
                        var xd = args.NewValue as XamlDocument;

                        // Get ViewModel from MainViewModel creation.
                        document.InitializeViewModel(document.MainViewModel.DocumentViewModels[xd]);
                    }
                }
            }));

        public Document()
        {
            this.InitializeComponent();

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        private void InitializeViewModel(DocumentViewModel model)
        {
            ViewModel = model;

            // Pass Reference to our Control so we can 'render' to it.
            ViewModel.XamlRoot = XamlRoot;

            // Listen for Line Highlighting Changes and Update our Editor
            ViewModel.Compiled += (sender2, args2) =>
            {
                CodeEditor.Decorations = ViewModel.LineDecorations;
            };

            CodeEditor.Options.Folding = true;

            ViewModel.NavigateToLineCommand = new RelayCommand<uint>(NavigateToLine);
            ViewModel.InsertTextCommand = new RelayCommand<string>(InsertText);
            ViewModel.UpdateXamlCommand = new AsyncRelayCommand<RoutedEventArgs>(UpdateXaml);

            SetPaneOrientation();

            LoadedDocument.State.PropertyChanged += DocumentState_PropertyChanged;

            SettingsService.Instance.PropertyChanged += DocumentState_PropertyChanged;

            // RenderAsync XAML if enabled by default
            if (SettingsService.Instance.IsAutoCompileEnabled == true)
            {
                ViewModel.UpdateXamlCommand.Execute(null);
            }
            else
            {
                XamlRoot.Children.Add(new TextBlock()
                {
                    Text = "Document_Compile".GetLocalized(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }

        private async void CodeEditor_Loading(object sender, RoutedEventArgs e)
        {
            var libserv = LibraryService.Instance;

            var languages = new Monaco.LanguagesHelper(CodeEditor);

            await languages.RegisterCompletionItemProviderAsync("xml", new XamlLanguageProvider()
            {
                KnownNamespaces = SettingsService.Instance.KnownNamespaces.ToList()
            });

            await languages.RegisterHoverProviderAsync("xml", (model, position) =>
            {
                return AsyncInfo.Run(async delegate (CancellationToken cancelationToken)
                {
                    var word = await model.GetWordAtPositionAsync(position);

                    if (word != null && !string.IsNullOrWhiteSpace(word.Word) &&
                        XamlRenderService.GetTypeFromName(word.Word) is Type type &&
                        libserv.LibrariesByNamespace.TryGetValue(type.Namespace, out LibraryInfo info))
                    {
                        _lastHoverType = type;
                        return new Hover(new string[]
                        {
                            "**" + word.Word + "** - [" + type.FullName + "](" +
                                libserv.GetLinkForType(type, info)
                            + ")"
                        }, new Range(position.LineNumber, word.StartColumn, position.LineNumber, word.EndColumn));
                    }

                    return null;
                });
            });
        }

        private async void NavigateToLine(uint line)
        {
            await CodeEditor.RevealLineInCenterIfOutsideViewportAsync(line);
        }

        private void InsertText(string text)
        {
            CodeEditor.SelectedText = text;
            
            DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                ViewModel.HasCompiled = false;
                ViewModel.UpdateXamlCommand.Execute(null);
            });
        }

        private async Task UpdateXaml(RoutedEventArgs args)
        {
            // Check if nothing to do
            if (ViewModel.Document.Content == null || ViewModel.Document.Content.Length == 0 || ViewModel.HasCompiled)
            {
                return;
            }

            var keepcontent = !SettingsService.Instance.IsContentUpdatedWithSuggested.Value;

            var newcontent = await ViewModel.InternalRenderXamlAsync(ViewModel.Document.Content, 0, keepcontent);

            if (!keepcontent && newcontent.HasSuggestion)
            {
                var pos = await CodeEditor.GetPositionAsync();

                // Update our document with suggested changes.
                ViewModel.Document.Content = newcontent.SuggestedContent;

                // Restore cursor to where it was.
                await CodeEditor.SetPositionAsync(pos);
            }

            ViewModel.HasCompiled = true;
        }

        private void CodeEditor_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
        {
            // Workaround to https://github.com/hawkerm/monaco-editor-uwp/issues/6
            // as SelectedText doesn't support modifications it can't be binded to from the CodeEditor itself.
            // That or the PropertyChanged isn't firing for somereason on SelectedText change.
            if (args.KeyCode == 116) // F5
            {
                ViewModel.SelectedText = CodeEditor.SelectedText;
            }

            // Now pass onto VM now that we have SelectedText set.
            ViewModel.KeyDownCommand.Execute(args);
        }

        private void CodeEditor_OpenLinkRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            Analytics.TrackEvent("Open_Docs", new Dictionary<string, string> {
                { "Location", "CodeEditor" },
                { "Type", _lastHoverType?.FullName ?? "Unknown" },
                { "Uri", args.Uri.ToString() },
            });
        }

        private void DocumentState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentState.PreviewOrientation) ||
                e.PropertyName == nameof(SettingsService.DefaultPreviewPanePosition))
            {
                SetPaneOrientation();
            }
        }

        private void SetPaneOrientation()
        {
            var orientation = LoadedDocument.State.PreviewOrientation;

            // If we're set to default, go grab that value to start from
            if (orientation == null)
            {
                orientation = SettingsService.Instance.DefaultPreviewPanePosition;
            }

            switch (orientation.Value)
            {
                case PaneOrientation.HorizontalPreviewTop:
                    VisualStateManager.GoToState(this, "HorizontalPreviewTop", false);
                    VisualStateManager.GoToState(ShareButton, "Horizontal", false);
                    break;
                case PaneOrientation.VerticalPreviewRight:
                    VisualStateManager.GoToState(this, "VerticalPreviewRight", false);
                    VisualStateManager.GoToState(ShareButton, "Vertical", false);
                    break;
                case PaneOrientation.HorizontalPreviewBottom:
                    VisualStateManager.GoToState(this, "HorizontalPreviewBottom", false);
                    VisualStateManager.GoToState(ShareButton, "Horizontal", false);
                    break;
                case PaneOrientation.VerticalPreviewLeft:
                    VisualStateManager.GoToState(this, "VerticalPreviewLeft", false);
                    VisualStateManager.GoToState(ShareButton, "Vertical", false);
                    break;
            }
        }

        #region Share Button Code
        private readonly Lazy<CanvasDevice> _device = new Lazy<CanvasDevice>(InitCanvas);

        private static CanvasDevice InitCanvas()
        {
            return CanvasDevice.GetSharedDevice();
        }

        private CanvasBitmap _screenshotImage;

        private async void ShareButton_Click(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
        {
            _screenshotImage = await GetAppScreenshot();

            DataTransferManager.ShowShareUI();
        }

        private void ShareMenuEntireWindow_Click(object sender, RoutedEventArgs e)
        {
            ShareButton_Click(null, null);
        }

        private async void ShareMenuPreviewOnly_Click(object sender, RoutedEventArgs e)
        {
            _screenshotImage = await GetPreviewScreenshot();

            DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            // Provide updated bitmap data using delayed rendering
            if (_screenshotImage != null)
            {
                var deferral = args.Request.GetDeferral();

                args.Request.Data.Properties.Title = "XAML Studio - " + LoadedDocument.Title;

                InMemoryRandomAccessStream inMemoryStream = new InMemoryRandomAccessStream();

                await _screenshotImage.SaveAsync(inMemoryStream, CanvasBitmapFileFormat.Png); // TODO: Have Option for quality?

                args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromStream(inMemoryStream));

                deferral.Complete();
            }
        }

        private async Task<CanvasBitmap> GetAppScreenshot()
        {
            var renderTarget = new RenderTargetBitmap();
            var displayInfo = DisplayInformation.GetForCurrentView();
            var scale = displayInfo.RawPixelsPerViewPixel;
            var scaleWidth = (int)Math.Ceiling(Window.Current.Bounds.Width / scale);
            var scaleHeight = (int)Math.Ceiling(Window.Current.Bounds.Height / scale);
            await renderTarget.RenderAsync(Window.Current.Content, scaleWidth, scaleHeight);
            var pixels = await renderTarget.GetPixelsAsync();
            return CanvasBitmap.CreateFromBytes(_device.Value, pixels, renderTarget.PixelWidth, renderTarget.PixelHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }

        private async Task<CanvasBitmap> GetPreviewScreenshot()
        {
            var renderTarget = new RenderTargetBitmap();
            var displayInfo = DisplayInformation.GetForCurrentView();
            var scale = displayInfo.RawPixelsPerViewPixel;
            var scaleWidth = (int)Math.Ceiling(XamlRoot.ActualWidth / scale);
            var scaleHeight = (int)Math.Ceiling(XamlRoot.ActualHeight / scale);
            await renderTarget.RenderAsync(XamlRoot, scaleWidth, scaleHeight);
            var pixels = await renderTarget.GetPixelsAsync();
            return CanvasBitmap.CreateFromBytes(_device.Value, pixels, renderTarget.PixelWidth, renderTarget.PixelHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }
        #endregion
    }
}
