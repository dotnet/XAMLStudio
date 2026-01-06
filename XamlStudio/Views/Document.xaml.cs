// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AppCenter.Analytics;
using Microsoft.Graphics.Canvas;
using Microsoft.Language.Xml;
using Monaco;
using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Controls;
using XamlStudio.Toolkit.Extensions;
using XamlStudio.Toolkit.Services;
using XamlStudio.ViewModels;

using Range = Monaco.Range;

namespace XamlStudio.Views;

public sealed partial class Document : UserControl,
    IRecipient<InsertTextMessage>,
    IRecipient<NavigateToLineMessage>,
    IRecipient<RenderXamlMessage>,
    IRecipient<XamlCompiledMessage>
{
    private string[] _decorations = Array.Empty<string>();

    private Type _lastHoverType;

    public DocumentViewModel ViewModel
    {
        get { return (DocumentViewModel)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(DocumentViewModel), typeof(Document), new PropertyMetadata(null, (sender, args) =>
        {
            if (sender is Document document)
            {
                var xd = args.NewValue as DocumentViewModel;

                // Unload our old one if it's not the same as our current
                if (args.OldValue is DocumentViewModel oldModel && xd != args.OldValue)
                {
                    document.UnloadViewModel(oldModel);
                }

                // Setup our new one (if it's not the same as our current)
                if (args.OldValue == null || xd != args.OldValue)
                {
                    document.InitializeViewModel(xd);
                }
            }
        }));

    public XamlDocument LoadedDocument
    {
        get { return (XamlDocument)GetValue(LoadedDocumentProperty); }
        set { SetValue(LoadedDocumentProperty, value); }
    }

    public static readonly DependencyProperty LoadedDocumentProperty =
        DependencyProperty.Register(nameof(LoadedDocument), typeof(XamlDocument), typeof(Document), new PropertyMetadata(null));

    public bool IsSpecificPreviewSize
    {
        get { return (bool)GetValue(IsSpecificPreviewSizeProperty); }
        set { SetValue(IsSpecificPreviewSizeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsSpecificPreviewSize.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSpecificPreviewSizeProperty =
        DependencyProperty.Register(nameof(IsSpecificPreviewSize), typeof(bool), typeof(Document), new PropertyMetadata(false));

    public ObservableCollection<BreadcrumbInfo> Breadcrumbs = new();

    public Document()
    {
        this.InitializeComponent();

        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

        dataTransferManager.DataRequested += DataTransferManager_DataRequested;

        Loaded += Document_Loaded;
        Unloaded += Document_Unloaded;

        CodeEditor.RegisterPropertyChangedCallback(CodeEditor.SelectedRangeProperty, CodeEditor_SelectedRangeChanged);
    }

    private void Document_Loaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.RegisterAll(this);

        // HACK: TODO: Workaround for Monaco editor not updating it's content?
        ViewModel.Document.Content = ViewModel.Document.Content + " ";
        ViewModel.Document.Content = ViewModel.Document.Content.Substring(0, ViewModel.Document.Content.Length - 1);

        _ = UpdateBreadcrumbs();

        SetPaneOrientation();
    }

    private void Document_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void UnloadViewModel(DocumentViewModel model)
    {
        ViewModel.PreviewerXamlRoot = null;

        WeakReferenceMessenger.Default.UnregisterAll(this);

        ViewModel.Document.State.PropertyChanged -= DocumentState_PropertyChanged;
    }

    private void InitializeViewModel(DocumentViewModel model)
    {
        var previewerRoot = IsSpecificPreviewSize ? PreviewRootSpecific : PreviewXamlRoot;

        LoadedDocument = model.Document;

        CodeEditor.Options.Folding = true;

        SetPaneOrientation();
        SetPreviewAreaTheme();

        LoadedDocument.State.PropertyChanged += DocumentState_PropertyChanged;

        SettingsService.Instance.PropertyChanged -= DocumentState_PropertyChanged;
        SettingsService.Instance.PropertyChanged += DocumentState_PropertyChanged;

        // RenderAsync XAML if enabled by default
        if (SettingsService.Instance.IsAutoCompileEnabled == true)
        {
            Receive(new RenderXamlMessage());
        }
        else
        {
            previewerRoot.Children.Add(new TextBlock()
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

    public async void Receive(NavigateToLineMessage message)
    {
        await CodeEditor.RevealLineInCenterIfOutsideViewportAsync(message.Line);
    }

    public void Receive(InsertTextMessage message)
    {
        CodeEditor.SelectedText = message.Text;

        DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            ViewModel.HasCompiled = false;
            Receive(new RenderXamlMessage());
        });
    }

    public async void Receive(RenderXamlMessage message)
    {
        // Check if nothing to do
        if (ViewModel.Document.Content == null
            || ViewModel.Document.Content.Length == 0
            || ViewModel.HasCompiled
            || !ViewModel.Document.IsActive)
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

        if (!string.IsNullOrWhiteSpace(newcontent.DataContextSource))
        {
            // TODO: We loaded a d:DesignData data source, we should show this instead in the DataSources tab
            // Along with an InfoBar to say the file has been loaded directly.
            WeakReferenceMessenger.Default.Send<DataSourceSetInFileMessage>(new(newcontent.DataContextSource));
        }
        else
        {
            WeakReferenceMessenger.Default.Send<DataSourceSetInFileMessage>(new(null));
        }
    }

    public void Receive(XamlCompiledMessage message)
    {
        // TODO: Is there a better way for us to manage that only the active document cares about this message
        // TODO: We should probably do a better job of managing the listening of messages based on active document
        // TODO: Then we can use StrongReferenceMessenger instead?
        if (!ViewModel.Document.IsActive) return;

        var result = message.Context;
        if (result.Element != null)
        {
            // Remove any existing Adorner before we re-render
            RemoveAdorner();

            var cleanPanel = IsSpecificPreviewSize ? PreviewXamlRootSpecific : PreviewXamlRoot;

            // Clean-up existing XAML content
            if (cleanPanel.Children.Count > 0)
            {
                foreach (var child in cleanPanel.Children)
                {
                    // TODO: Check if this helps with the MediaPlayer issue?
                    //// See WinUI Issue with Expander/AnimatedIcon: https://github.com/microsoft/microsoft-ui-xaml/issues/9278
                    //// VisualTreeHelper.DisconnectChildrenRecursive(child);
                }
                cleanPanel.Children.Clear();
            }

            var specificSize = false;
            var fe = result.Element as FrameworkElement;

            // Setup a specific area if requested.
            if (result.RequestedWidth != null)
            {
                PreviewRootSpecific.Width = result.RequestedWidth.Value;
                if (fe != null && fe.ReadLocalValue(WidthProperty) != DependencyProperty.UnsetValue)
                {
                    PreviewXamlRootSpecific.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else
                {
                    PreviewXamlRootSpecific.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
                specificSize = true;
            }
            else if (fe != null && fe.ReadLocalValue(WidthProperty) != DependencyProperty.UnsetValue)
            {
                PreviewRootSpecific.Width = fe.Width;
                PreviewXamlRootSpecific.HorizontalAlignment = HorizontalAlignment.Center;
                specificSize = true;
            }
            else
            {
                PreviewRootSpecific.ClearValue(WidthProperty);
                PreviewXamlRootSpecific.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            if (result.RequestedHeight != null)
            {
                PreviewRootSpecific.Height = result.RequestedHeight.Value;
                if (fe != null && fe.ReadLocalValue(HeightProperty) != DependencyProperty.UnsetValue)
                {
                    PreviewXamlRootSpecific.VerticalAlignment = VerticalAlignment.Center;
                }
                else
                {
                    PreviewXamlRootSpecific.VerticalAlignment = VerticalAlignment.Stretch;
                }
                specificSize = true;
            }
            else if (fe != null && fe.ReadLocalValue(HeightProperty) != DependencyProperty.UnsetValue)
            {
                PreviewRootSpecific.Height = fe.Height;
                PreviewXamlRootSpecific.VerticalAlignment = VerticalAlignment.Center;
                specificSize = true;
            }
            else
            {
                PreviewRootSpecific.ClearValue(HeightProperty);
                PreviewXamlRootSpecific.VerticalAlignment = VerticalAlignment.Stretch;
            }

            // TODO: Have a set of specific device/resolution sizes for testing, HD, etc...
            IsSpecificPreviewSize = specificSize;

            var targetPanel = IsSpecificPreviewSize ? PreviewXamlRootSpecific : PreviewXamlRoot;

            var element = result.Element;
            if (result.IsResourceDictionary)
            {
                element = new ResourceViewer()
                {
                    ResourceDictionary = element as ResourceDictionary,
                    XmlDocument = result.Document
                };
            }

            // Only Update if we have a new well-parsed element.
            if (element != null && element is UIElement)
            {
                // Add element to main panel
                targetPanel.Children.Add(element as UIElement);
            }

            // If we're a content dialog then show it in-place
            if (result.Element is ContentDialog contentDialog)
            {
                // TODO: We may want to extract the ContentDialog's content/properties and show it in a mocked-up control wrapper, as this may interfere with XAML Studio's own dialogs...
                _ = contentDialog.ShowAsync(ContentDialogPlacement.InPlace);
            }

            // Pass Reference to our Control so we can 'render' to it.
            ViewModel.PreviewerXamlRoot = targetPanel;
            ViewModel.ActualTheme = ActualTheme;

            ViewModel.HasCompiled = true;
            ViewModel.Document.State.RenderState = SyncStatus.Synced;

            //// TODO: Only do this if property panel is open or design mode is active?
            ViewModel.XamlCoordinator.Initialize(result.XmlDocument, (DependencyObject)result.Element);

            WeakReferenceMessenger.Default.Send<XamlRenderedMessage>(new(result));
        }
        else
        {
            ViewModel.HasCompiled = true;
        }
    }

    private ThreadPoolTimer _autocompileTimer;

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

    private void CodeEditor_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
    {
        // Workaround to https://github.com/hawkerm/monaco-editor-uwp/issues/6
        // as SelectedText doesn't support modifications it can't be binded to from the CodeEditor itself.
        // That or the PropertyChanged isn't firing for somereason on SelectedText change.
        if (args.KeyCode == 116) // F5
        {
            ViewModel.SelectedText = CodeEditor.SelectedText;
        }

        // Handle Shortcuts. https://keycode.info/
        // Ctrl+Enter or F5 Update // TODO: Do we need this in the app handler too? (Thinking no)
        if ((args.KeyCode == 13 && args.CtrlKey) ||
             args.KeyCode == 116)
        {
            ////if (args.KeyCode == 116 &&
            ////    SettingsService.Instance.IsCompileSelectionEnabled.Value == true &&
            ////    !string.IsNullOrWhiteSpace(SelectedText))
            ////{
            ////    SelectiveRenderXaml(SelectedText);
            ////}
            ////else
            ////{
            ViewModel.ForceRefreshCommand?.Execute(null);
            ////}

            // Eat key stroke
            args.Handled = true;
        }

        if (args.Handled)
        {
            Analytics.TrackEvent("Key_Shortcut", new Dictionary<string, string>()
            {
                { "Location", "Document" },
                { "Action", args.Handled.ToString() },
                { "Ctrl", args.CtrlKey.ToString() },
                { "Shift", args.ShiftKey.ToString() },
                { "Code", args.KeyCode.ToString() }
            });
        }

        args.Handled = WeakReferenceMessenger.Default.Send<KeyDownMessage>(new(args.CtrlKey, args.ShiftKey, args.KeyCode));

        // Ignore as a change to the document if we handle it as a shortcut above or it's a special char.
        if (!args.Handled && Array.IndexOf(NonCharacterCodes, args.KeyCode) == -1)
        {
            // TODO: Filter out non-display characters or look for text change...
            ViewModel.Document.HasChanged = true; // Mark Dirty
            ViewModel.HasCompiled = false;
            ViewModel.Document.State.RenderState = SyncStatus.OutOfSync;

            // Setup Time for Auto-Compile
            if (SettingsService.Instance.IsAutoCompileEnabled.Value)
            {
                _autocompileTimer?.Cancel(); // Stop Old Timer
                                             // Create Compile Timer
                                             // TODO: Clean up and use debounce method from toolkit instead?
                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                _autocompileTimer = ThreadPoolTimer.CreateTimer((e) =>
                {
                    dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                    {
                        WeakReferenceMessenger.Default.Send<RenderXamlMessage>();
                    });
                }, TimeSpan.FromSeconds(SettingsService.Instance.AutoCompileDelay.Value));
            }
        }
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
        else if (e.PropertyName == nameof(DocumentState.PreviewAreaTheme))
        {
            SetPreviewAreaTheme();
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

    private void SetPreviewAreaTheme()
    {
        var theme = LoadedDocument.State.PreviewAreaTheme ?? ThemeSelectorService.Theme;

        PreviewArea.RequestedTheme = theme;
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
        var targetPanel = IsSpecificPreviewSize ? PreviewXamlRootSpecific : PreviewXamlRoot;

        var renderTarget = new RenderTargetBitmap();
        var displayInfo = DisplayInformation.GetForCurrentView();
        var scale = displayInfo.RawPixelsPerViewPixel;
        var scaleWidth = (int)Math.Ceiling(targetPanel.ActualWidth / scale);
        var scaleHeight = (int)Math.Ceiling(targetPanel.ActualHeight / scale);
        await renderTarget.RenderAsync(targetPanel, scaleWidth, scaleHeight); // TODO: Bug need to get specific for specific size ones...
        var pixels = await renderTarget.GetPixelsAsync();
        return CanvasBitmap.CreateFromBytes(_device.Value, pixels, renderTarget.PixelWidth, renderTarget.PixelHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
    }
    #endregion

    #region BreadcrumbBar Events
    private async void BreadcrumbBar_ItemClicked(Microsoft.UI.Xaml.Controls.BreadcrumbBar sender, Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Item is BreadcrumbInfo info)
        {
            await CodeEditor.RevealPositionInCenterAsync(info.Location);
        }
    }

    private void CodeEditor_SelectedRangeChanged(DependencyObject sender, DependencyProperty dp)
    {
        _ = UpdateBreadcrumbs();
    }

    private async Task UpdateBreadcrumbs()
    {
        var text = CodeEditor.Text; // This is hopefully in-sync so we don't round-trip again.
        var position = await CodeEditor.GetPositionAsync(); // TODO: Should we just monitor and keep track of this vs. polling?
        if (position == null) return;

        var index = text.GetCharacterIndex((int)position.LineNumber, (int)position.Column);

        if (index == -1)
        {
            return;
        }

        // TODO: This is expensive, we should be doing this on a debounce and more globally as the document updates to be used elsewhere for visual tree sync, etc... as part of render loop
        var _xmlRoot = ViewModel.HasCompiled ? ViewModel.Result.XmlDocument : Parser.ParseText(text);

        Breadcrumbs.Clear();

        var current = _xmlRoot.FindNode(index + 1);

        WeakReferenceMessenger.Default.Send<EditorSelectedElementMessage>(new(current.ParentElement));

        foreach (var node in current.AncestorNodesAndSelf())
        {
            if (node is IXmlElementSyntax element)
            {
                var loc = text.GetLineColumnIndex(node.Span.Start);
                Breadcrumbs.Insert(0, new BreadcrumbInfo()
                {
                    Name = element.Name,
                    Location = new Position((uint)loc.Line, (uint)loc.Column),
                    // TODO: Put child sister nodes in a list so that can have drop-down to navigate within document?
                });
            }
        }

        // Didn't want to re-write style yet, Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/7213
        Breadcrumbs.Add(new BreadcrumbInfo()
        {
            Name = $"Caret @ L{position.LineNumber} Ch{position.Column}",
            Location = position,
        });
    }
    #endregion

    /// <summary>
    /// Converts the requested (setting) and actual theme into a glyph for display.
    /// </summary>
    /// <param name="requestedTheme">Settings Value of the default requested theme</param>
    /// <param name="actualTheme">ActualTheme from a root element for the app</param>
    /// <returns></returns>
    public string GetAppliedThemeGlyph(ElementTheme? requestedTheme, ElementTheme actualTheme)
    {
        var theme = actualTheme;

        if (requestedTheme != null && requestedTheme != ElementTheme.Default)
        {
            theme = requestedTheme.Value;
        }

        return theme switch
        {
            ElementTheme.Dark => "\uE708",
            ElementTheme.Light => "\uE793",
            _ => "\uE706;",
        };
    }
}

public class BreadcrumbInfo
{
    public string Name { get; set; }

    public Position Location { get; set; }
}
