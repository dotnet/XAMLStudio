using Microsoft.AppCenter.Analytics;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
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
                // TODO: Make this template somewhere user-editable?
                Content = "NewDocumentTemplate".GetLocalized()
            });

            ActiveFile = OpenFiles.Last();

            Analytics.TrackEvent("Document_New");
        }

        private void DuplicateDocument(RoutedEventArgs args)
        {

            OpenFiles.Add(new Models.XamlDocument("Untitled-" + _untitledCount++)
            {
                // TODO: Make this template somewhere user-editable?
                Content = ActiveFile.Content
            });;

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

                Analytics.TrackEvent("Document_Open");
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
                OpenFiles.Add(XamlDocument.WelcomeDocument());
            }

            // Remove what we had as active (otherwise, the active would be null and we'd hit an error)
            OpenFiles.RemoveAt(OpenFiles.IndexOf(document));

            ActiveFile = OpenFiles.Last();

            Analytics.TrackEvent("Document_Close");

            return true;
        }

        // Ctrl+Shift+S
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

            // Ensure if we can restore the file if it wasn't available on load
            await document.RestoreFileAsync();

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
        private void PreviousDocument(RoutedEventArgs args)
        {
            var index = OpenFiles.IndexOf(ActiveFile);
            index = index == 0 ? OpenFiles.Count - 1 : index - 1;

            ActiveFile = OpenFiles[index];

            Analytics.TrackEvent("Document_Previous");
        }

        // Ctrl+Tab
        private void NextDocument(RoutedEventArgs args)
        {
            var index = OpenFiles.IndexOf(ActiveFile);
            index = index == OpenFiles.Count - 1 ? 0 : index + 1;

            ActiveFile = OpenFiles[index];

            Analytics.TrackEvent("Document_Next");
        }

        // Ctrl+I
        private void OpenSettingsPage(RoutedEventArgs args)
        {
            XamlDocument settings = OpenFiles.FirstOrDefault(f => f.DocumentType == DocumentType.Settings);
            if (settings != null)
            {
                ActiveFile = settings;
            }
            else
            {
                OpenFiles.Add(XamlDocument.SettingsDocument());
                ActiveFile = OpenFiles.Last();
            }

            Analytics.TrackEvent("Open_Settings");
        }

        // Ctrl+Shift+?
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

        private void KeyDown(KeyEventArgs args)
        {
            var ctrl = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            var shift = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            var active = false;
            // Need to duplicate in DocumentViewModel for the Editor too, TODO: Figure out centralization or create editor commands?
            if (ctrl)
            {
                if (shift)
                {
                    // Quick Shortcuts for Things
                    switch (args.VirtualKey)
                    {
                        // Open Explorer
                        case VirtualKey.E:
                            active = true;
                            OpenActivityCommand.Execute("EXPLORER");
                            break;
                        // Open Data Context
                        case VirtualKey.C:
                            active = true;
                            OpenActivityCommand.Execute("DATASOURCES");
                            break;
                        // Open Binding Debugger
                        case VirtualKey.B:
                            active = true;
                            OpenActivityCommand.Execute("DEBUG");
                            break;
                        // Open Toolbox
                        case VirtualKey.T:
                            active = true;
                            OpenActivityCommand.Execute("TOOLBOX");
                            break;
                    }
                }
                else
                {
                    switch (args.VirtualKey)
                    {
                        // Open Settings
                        case VirtualKey.I:
                            active = true;
                            OpenSettingsCommand.Execute(null);
                            break;
                        // New
                        case VirtualKey.N:
                            active = true;
                            NewDocumentCommand.Execute(null);
                            break;
                        // Open
                        case VirtualKey.O:
                            active = true;
                            OpenDocumentCommand.Execute(null);
                            break;
                        // Save
                        case VirtualKey.S:
                            if (shift)
                            {
                                SaveDocumentAsCommand.Execute(ActiveFile);
                            }
                            else
                            {   
                                SaveDocumentCommand.Execute(ActiveFile);
                            }
                            active = true;
                            break;
                        // Close
                        case VirtualKey.W:
                        case VirtualKey.F4:
                            active = true;
                            CloseActiveDocumentCommand.Execute(ActiveFile);
                            break;
                        // Prev/Next Document
                        case VirtualKey.Tab:
                            if (shift)
                            {
                                active = true;
                                PreviousDocumentCommand.Execute(null);
                            }
                            else
                            {
                                active = true;
                                NextDocumentCommand.Execute(null);
                            }
                            break;
                    }
                }

                if (active)
                {
                    Analytics.TrackEvent("Key_Shortcut", new Dictionary<string, string>()
                    {
                        { "Location", "MainView" },
                        { "Action", active.ToString() },
                        { "Ctrl", ctrl.ToString() },
                        { "Shift", shift.ToString() },
                        { "Code", args.VirtualKey.ToString() }
                    });
                }
            }
        }
    }
}
