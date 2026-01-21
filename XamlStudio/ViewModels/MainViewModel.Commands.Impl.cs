// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Mvvm.Input;
using Microsoft.AppCenter.Analytics;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;

namespace XamlStudio.ViewModels;

public partial class MainViewModel
{
    private readonly AsyncLock _openMutex = new();

    private void CloseWelcomeScreen()
    {
        // If on Welcome screen, we should close it...
        if (OpenActivity == "WELCOME")
        {
            // TODO: Should maybe have the last open one tracked?
            OpenActivity = null;
        }
    }

    [RelayCommand]
    private void NewDocument()
    {
        OpenFiles.Add(new Models.XamlDocument("Untitled-" + _untitledCount++)
        {
            // TODO: Make this template somewhere user-editable?
            Content = "NewDocumentTemplate".GetLocalized()
        });

        ActiveFile = OpenFiles.Last();

        CloseWelcomeScreen();

        Analytics.TrackEvent("Document_New");
    }

    [RelayCommand]
    private void DuplicateDocument()
    {
        OpenFiles.Add(new Models.XamlDocument("Untitled-" + _untitledCount++)
        {
            Content = "" + ActiveFile.Content
        }); ;

        ActiveFile = OpenFiles.Last();
        Analytics.TrackEvent("Document_Duplicate");
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add(".xaml");
        picker.FileTypeFilter.Add(".xml");
        picker.FileTypeFilter.Add(".bind");
        picker.FileTypeFilter.Add(".txt");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            // If we only have one new document we haven't modified, close it before we open our 'first' doc.
            /*if (IsOnlyNewDocumentOpen)
            {
                Documents.RemoveAt(0);
            }*/

            await OpenFile(file);

            Analytics.TrackEvent("Document_Open");
        }
    }

    [RelayCommand]
    private async Task OpenFile(StorageFile file)
    {
        using (await _openMutex.LockAsync())
        {

            // Application now has read/write access to the picked file
            var doc = await XamlDocument.LoadFromFileAsync(file);
            OpenFiles.Add(doc);

            SettingsService.Instance.RememberFileOrFolder(file);

            ActiveFile = doc;

            CloseWelcomeScreen();
        }
    }

    public async void OpenFileFromWorkspace(StorageFile file, StorageFolder workspace)
    {
        // check if already open
        XamlDocument openDoc = OpenFiles.FirstOrDefault(f => f?.BackingFile?.Path == file.Path);
        if (openDoc != null)
        {
            openDoc.ParentFolder ??= workspace;

            ActiveFile = openDoc;
        }
        else
        {
            using (await _openMutex.LockAsync())
            {
                // Application now has read/write access to the picked file
                var doc = await XamlDocument.LoadFromFileAsync(file);
                doc.ParentFolder = workspace;
                OpenFiles.Add(doc);

                SettingsService.Instance.RememberFileOrFolder(file);

                ActiveFile = doc;
            }

            Analytics.TrackEvent("Document_Open_From_Workspace");
        }
    }

    [RelayCommand]
    private async Task OpenFolderPicker()
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            OpenFolder(folder);

            // Ensure can see new folder opened in Explorer after opening (if was on Welcome Page)
            OpenActivity = "EXPLORER";

            Analytics.TrackEvent("Folder_Open");
        }
    }

    private async void OpenFolder(StorageFolder folder)
    {
        using (await _openMutex.LockAsync())
        {
            // TODO: Check not contained within another workspace?
            SetupWorkspace(folder);

            SettingsService.Instance.RememberFileOrFolder(folder);

            Analytics.TrackEvent("Open_Workspace");
        }
    }

    [RelayCommand]
    private async Task<bool> CloseActiveDocument(XamlDocument document)
    {
        // TODO: Why is item null here?

        if (document.HasChanged)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(String.Format("Application_CloseConfirm".GetLocalized(), document.Title.TrimEnd('*')));

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            var saveCmd = new UICommand("Application_CloseConfirmSave".GetLocalized());
            var dontSaveCmd = new UICommand("Application_CloseConfirmDontSave".GetLocalized());
            var cancelCmd = new UICommand("Application_CloseConfirmCancel".GetLocalized());
            messageDialog.Commands.Add(saveCmd);
            messageDialog.Commands.Add(dontSaveCmd);
            messageDialog.Commands.Add(cancelCmd);

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 2;

            // Show the message dialog
            var result = await messageDialog.ShowAsync();

            if (result == saveCmd)
            {
                // Important to wait here for result.
                if (!await SaveDocument(document))
                {
                    // Cancel closing if they cancel the save (or error).
                    return false;
                }
            }
            else if (result == cancelCmd)
            {
                return false;
            }
        }

        /*if (IsOnlyNewDocumentOpen)
        {
            // Save and Exit if the only doc we're closing is the 'new' one.
            await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            Application.Current.Exit();
            return;
        }*/

        // Create a new Document if we're removing the last one (it will be selected)
        if (OpenFiles.Count == 1)
        {
            // TODO: We should probably disable other ActivityBar buttons in XAML if there's no active document?
            OpenActivity = "WELCOME";
        }

        // Remove what we had as active (otherwise, the active would be null and we'd hit an error)
        OpenFiles.RemoveAt(OpenFiles.IndexOf(document));

        ActiveFile = OpenFiles.LastOrDefault();

        Analytics.TrackEvent("Document_Close");

        return true;
    }

    // Ctrl+Shift+S
    [RelayCommand]
    private async Task<bool> SaveDocumentAs(XamlDocument document)
    {
        // TODO: Localize
        var name = "New Document";

        if (document.CanSave)
        {
            name = document.DisplayName + " Copy";
        }

        var file = await SaveFileDialog(name);

        if (file != null)
        {
            return await SaveFile(document, file);
        }

        return false;
    }

    private async Task<StorageFile> SaveFileDialog(string documentName)
    {
        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        // Dropdown of file types the user can save the file as
        savePicker.FileTypeChoices.Add("eXtended Application Markup Language", [".xaml"]);
        // Default file name if the user does not type one in or select a file to replace
        savePicker.SuggestedFileName = documentName;

        return await savePicker.PickSaveFileAsync();
    }

    [RelayCommand]
    private async Task<bool> SaveDocument(XamlDocument document)
    {

        // Ensure if we can restore the file if it wasn't available on load
        await document.RestoreFileAsync();

        StorageFile file;
        // Save As
        if (!document.CanSave)
        {
            file = await SaveFileDialog("New Document");
        }
        else
        {
            // Resave to Existing File
            file = document.BackingFile;
        }

        if (file != null)
        {
            return await SaveFile(document, file);
        }

        return false;
    }

    private static async Task<bool> SaveFile(XamlDocument document, StorageFile file)
    {
        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
        CachedFileManager.DeferUpdates(file);

        // Update/Save Document
        var result = await document.SaveAsAsync(file);

        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
        // Completing updates may require Windows to ask for user input.
        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
        if (status == FileUpdateStatus.Complete)
        {
            if (result)
            {
                SettingsService.Instance.RememberFileOrFolder(file);

                Analytics.TrackEvent("Document_Save", new Dictionary<string, string>()
                {
                    { "Success", "True" }
                });

                return true;
            }
        }

        // Show error about saving
        var messageDialog = new MessageDialog(String.Format("Application_SaveError".GetLocalized(), document.Title.TrimEnd('*')));
        await messageDialog.ShowAsync();

        Analytics.TrackEvent("Document_Save", new Dictionary<string, string>()
        {
            { "Success", "False" }
        });

        return false;
    }

    // Ctrl+Shift+Tab
    [RelayCommand]
    private void PreviousDocument()
    {
        var index = OpenFiles.IndexOf(ActiveFile);
        index = index == 0 ? OpenFiles.Count - 1 : index - 1;

        ActiveFile = OpenFiles[index];

        Analytics.TrackEvent("Document_Previous");
    }

    // Ctrl+Tab
    [RelayCommand]
    private void NextDocument()
    {
        var index = OpenFiles.IndexOf(ActiveFile);
        index = index == OpenFiles.Count - 1 ? 0 : index + 1;

        ActiveFile = OpenFiles[index];

        Analytics.TrackEvent("Document_Next");
    }

    // Ctrl+I
    [RelayCommand]
    private void OpenSettingsPage()
    {
        // TODO: Feels like enum to use? Switch SwitchPresenters?
        OpenActivity = "SETTINGS";

        Analytics.TrackEvent("Open_Settings");
    }

    // Ctrl+Shift+?
    [RelayCommand]
    private void OpenActivityPanel(string activity)
    {
        if (activity == OpenActivity)
        {
            // Close if the same as already open?
            OpenActivity = null;
        }
        else
        {
            OpenActivity = activity;
        }
    }
}
