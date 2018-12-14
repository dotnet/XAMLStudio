using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using XamlStudio.Helpers;
using XamlStudio.Models;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel : WorkspaceWindow
    {
        /// <summary>
        /// Keeps track of number of untitled documents we've created this session.
        /// </summary>
        private int _untitledCount = 1;

        public Dictionary<XamlDocument, DocumentViewModel> DocumentViewModels { get; } = new Dictionary<XamlDocument, DocumentViewModel>();

        public DocumentViewModel ActiveDocumentViewModel
        {
            get { return (DocumentViewModel)GetValue(ActiveDocumentViewModelProperty); }
            set { SetValue(ActiveDocumentViewModelProperty, value); }
        }

        public static readonly DependencyProperty ActiveDocumentViewModelProperty =
            DependencyProperty.Register(nameof(ActiveDocumentViewModelProperty), typeof(DocumentViewModel), typeof(WorkspaceWindow), new PropertyMetadata(null));

        public SettingsPanelViewModel SettingsViewModel { get; } = new SettingsPanelViewModel();

        public override void Initialize()
        {
            OpenFiles.CollectionChanged += OpenFiles_CollectionChanged;
            RegisterPropertyChangedCallback(ActiveFileProperty, (s, dp) =>
            {
                if (ActiveFile != null) // TabView can give us null first as it changes to the next one, is this a bug?
                {
                    ActiveDocumentViewModel = DocumentViewModels[ActiveFile];
                }
            });

            var welcome = XamlDocument.WelcomeDocument();

            OpenFiles.Add(welcome);
            ActiveFile = welcome;
        }

        public async Task RestoreWorkspaceAsync(XamlDocument[] docs)
        {
            if (OpenFiles.Count == 1 && OpenFiles.First().DocumentType != DocumentType.Document)
            {
                OpenFiles.Clear();
            }

            bool welcome = false;

            foreach(var doc in docs)
            {
                // Reopen/make connection to backing OS file.
                await doc.RestoreFileAsync();

                OpenFiles.Add(doc);

                if (doc.DocumentType == DocumentType.Welcome)
                {
                    welcome = true;
                }

                if (doc.IsActive)
                {
                    ActiveFile = doc;
                }
            }

            // Add our welcome screen back at the end for convenience.
            if (!welcome)
            {
                OpenFiles.Add(XamlDocument.WelcomeDocument());
            }
        }

        private void OpenFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is XamlDocument xd)
                    {
                        // Need MainViewModel to own these so we can keep track of them all.
                        DocumentViewModels[xd] = new DocumentViewModel() { Document = xd };
                    }
                }
            }
        }
    }
}
