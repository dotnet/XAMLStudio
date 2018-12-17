using Microsoft.AppCenter.Analytics;
using Monaco;
using Monaco.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            DependencyProperty.Register("LoadedDocument", typeof(XamlDocument), typeof(Document), new PropertyMetadata(null, (sender, args) =>
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

            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                ViewModel.HasCompiled = false;
                ViewModel.UpdateXamlCommand.Execute(null);
            });
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void CodeEditor_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
        {
            // Workaround to https://github.com/hawkerm/monaco-editor-uwp/issues/6
            // as SelectedText doesn't support modifications it can't be binded to from the CodeEditor itself.
            // That or the PropertyChanged isn't firing for somereason on SelectedText change.
            if (args.KeyCode == 116)
            {
                ViewModel.SelectedText = CodeEditor.SelectedText;
            }

            // TODO: Figure out way to translate and pass to our main key-shortcut router.
            if (args.CtrlKey)
            {
                // Need to duplicate this here from MainViewModel as Control eats CoreWindow event.
                switch (args.KeyCode)
                {
                    case 78: // N
                        MainViewModel.NewDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 79: // O
                        MainViewModel.OpenDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 83: // S
                        if (args.ShiftKey)
                        {
                            MainViewModel.SaveDocumentAsCommand.Execute(LoadedDocument);
                        }
                        else
                        {
                            MainViewModel.SaveDocumentCommand.Execute(LoadedDocument);
                        }
                        args.Handled = true;
                        break;
                    case 87: // W
                    case 115: // F4
                        MainViewModel.CloseActiveDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 9: // TAB
                        if (args.ShiftKey)
                        {
                            MainViewModel.PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            MainViewModel.NextDocumentCommand.Execute(null);
                        }
                        args.Handled = true;
                        break;
                }
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
    }
}
