using Monaco.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XamlStudio.Views
{
    public sealed partial class Document : UserControl
    {
        private string[] _decorations = Array.Empty<string>();

        public MainViewModel MainViewModel { get; set; }

        public DocumentViewModel ViewModel { get; private set; }

        public XamlDocument LoadedDocument
        {
            get { return (XamlDocument)GetValue(LoadedDocumentProperty); }
            set { SetValue(LoadedDocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoadedDocument.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadedDocumentProperty =
            DependencyProperty.Register("LoadedDocument", typeof(XamlDocument), typeof(Document), new PropertyMetadata(null, (sender, args) =>
            {
                var document = (sender as Document);
                if (document != null) {
                    document.ViewModel.Document = args.NewValue as XamlDocument;

                    // RenderAsync XAML
                    document.ViewModel.UpdateXamlCommand.Execute(null);
                }
            }));

        public Document()
        {
            this.InitializeComponent();

            ViewModel = new DocumentViewModel();

            // Pass Reference to our Control so we can 'render' to it.
            ViewModel.XamlRoot = XamlRoot;

            // Listen for Line Highlighting Changes and Update our Editor
            ViewModel.Compiled += (sender2, args2) =>
            {
                CodeEditor.Decorations = ViewModel.LineDecorations;
            };

            CodeEditor.Options.Folding = true;

            Loaded += Document_Loaded;
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            MainViewModel.DocumentViewModel = ViewModel;   
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
                        MainViewModel.SaveDocumentCommand.Execute(LoadedDocument);
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
    }
}
