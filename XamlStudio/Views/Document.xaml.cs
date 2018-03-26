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
                    // TODO: Move these to a central spot for the control itself vs. the document.
                    document.CodeEditor.Options.Language = "xml"; // TODO: Workaround for https://github.com/hawkerm/monaco-editor-uwp/issues/10
                    document.CodeEditor.Options.Folding = true;

                    document.ViewModel.Document = args.NewValue as XamlDocument;

                    // Pass Reference to our Control so we can 'render' to it.
                    document.ViewModel.XamlRoot = document.XamlRoot;

                    // Listen for Line Highlighting Changes and Update our Editor
                    document.ViewModel.Compiled += (sender2, args2) =>
                    {
                        document.CodeEditor.Decorations = document.ViewModel.LineDecorations;
                    };

                    // Render XAML
                    document.ViewModel.UpdateXamlCommand.Execute(null);
                }
            }));

        public Document()
        {
            this.InitializeComponent();

            this.ViewModel = new DocumentViewModel();
        }
    }
}
