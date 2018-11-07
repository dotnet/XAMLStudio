using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel
    {
        private readonly AsyncLock _openMutex = new AsyncLock();

        private void NewDocument(RoutedEventArgs args)
        {
            OpenFiles.Add(new Models.XamlDocument("Untitled-" + _untitledCount++)
            {
                // TODO: Make this template somewhere editable
                Content =
@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
    mc:Ignorable=""d"">

    <Grid Padding=""40"">
        <TextBlock>
            <Run FontSize=""24"" Foreground=""#FFFC5185"">Get Started with XAML Studio</Run><LineBreak/>
            <Run> Modify this text below to see a live preview.</Run>
        </TextBlock>
    </Grid>
</Page>"
            });

            ActiveFile = OpenFiles.Last();
        }

        private async void OpenDocument(RoutedEventArgs args)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
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

                OpenFile(file);
            }
        }

        private async void OpenFile(StorageFile file)
        {
            using (await _openMutex.LockAsync())
            {

                // Application now has read/write access to the picked file
                var doc = await XamlDocument.LoadFromFileAsync(file);
                OpenFiles.Add(doc);

                SettingsService.Instance.RememberFile(file);

                ActiveFile = doc;
            }
        }

        private async void CloseActiveDocument(XamlDocument document)
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
                        return;
                    }
                }
                else if (result == cancelCmd)
                {
                    return;
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
                NewDocument(null);
            }

            // Remove what we had as active (otherwise, the active would be null and we'd hit an error)
            OpenFiles.RemoveAt(OpenFiles.IndexOf(document));            
        }

        // Ctrl+Shift+S
        private async Task<bool> SaveDocumentAs(XamlDocument document)
        {
            // TODO: Localize
            var name = "New Document";

            if (document.CanSave)
            {
                name = document.BackingFile.DisplayName + " Copy";
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
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("eXtended Application Markup Language", new List<string>() { ".xaml" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = documentName;

            return await savePicker.PickSaveFileAsync();
        }

        private async Task<bool> SaveDocument(XamlDocument document)
        {
            StorageFile file = null;

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
                    SettingsService.Instance.RememberFile(file);
                }
                //OutputTextBlock.Text = "File " + file.Name + " was saved.";
                return result;
            }
            else
            {
                //OutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
                return false; // Should have another status/msg here?
            }
        }

        // Ctrl+Shift+Tab
        private void PreviousDocument(RoutedEventArgs args)
        {
            var index = OpenFiles.IndexOf(ActiveFile);
            index = index == 0 ? OpenFiles.Count - 1 : index - 1;

            ActiveFile = OpenFiles[index];
        }

        // Ctrl+Tab
        private void NextDocument(RoutedEventArgs args)
        {
            var index = OpenFiles.IndexOf(ActiveFile);
            index = index == OpenFiles.Count - 1 ? 0 : index + 1;

            ActiveFile = OpenFiles[index];
        }

        private void KeyDown(KeyEventArgs args)
        {
            var ctrl = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            var shift = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            if (ctrl)
            {
                switch (args.VirtualKey)
                {
                    // New
                    case Windows.System.VirtualKey.N:
                        NewDocumentCommand.Execute(null);
                        break;
                    // Open
                    case Windows.System.VirtualKey.O:
                        OpenDocumentCommand.Execute(null);
                        break;
                    // Save
                    case Windows.System.VirtualKey.S:
                        if (shift)
                        {
                            SaveDocumentAsCommand.Execute(null);
                        }
                        else
                        {
                            SaveDocumentCommand.Execute(null);
                        }
                        break;
                    // Close
                    case Windows.System.VirtualKey.W:
                    case Windows.System.VirtualKey.F4:
                        CloseActiveDocumentCommand.Execute(null);
                        break;
                    // Prev/Next Document
                    case Windows.System.VirtualKey.Tab:                        
                        if (shift)
                        {
                            PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            NextDocumentCommand.Execute(null);
                        }
                        break;
                }
            }
        }
    }
}
