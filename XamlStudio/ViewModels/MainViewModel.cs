using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using XamlStudio.Models;

namespace XamlStudio.ViewModels;

public partial class MainViewModel : WorkspaceWindow
{
    /// <summary>
    /// Keeps track of number of untitled documents we've created this session.
    /// </summary>
    private int _untitledCount = 1;

    public Dictionary<XamlDocument, DocumentViewModel> DocumentViewModels { get; } = new Dictionary<XamlDocument, DocumentViewModel>();

    [ObservableProperty]
    private DocumentViewModel _activeDocumentViewModel;

    partial void OnActiveDocumentViewModelChanged(DocumentViewModel oldValue, DocumentViewModel newValue)
    {
        WeakReferenceMessenger.Default.Send<ActiveDocumentViewModelChangedMessage>(new(oldValue, newValue));
    }

    public SettingsPanelViewModel SettingsViewModel { get; } = new SettingsPanelViewModel();

    public override void Initialize()
    {
        OpenFiles.CollectionChanged += OpenFiles_CollectionChanged;
        ActiveFileChanged += (sender, file) =>
        {
            if (file != null) // TabView can give us null first as it changes to the next one, is this a bug?
            {
                ActiveDocumentViewModel = DocumentViewModels[file];
            }
        };

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

        foreach (var doc in docs)
        {
            // Reopen/make connection to backing OS file.
            await doc.RestoreFileAsync();

            // Restore Data Context File (if one).
            await doc.DataContext.RestoreFileAsync();

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
                    DocumentViewModels[xd] = new DocumentViewModel() { Document = xd, MainViewModel = this };
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems)
            {
                if (item is XamlDocument xd)
                {
                    DocumentViewModels.Remove(xd);
                }
            }
        }
    }
}
