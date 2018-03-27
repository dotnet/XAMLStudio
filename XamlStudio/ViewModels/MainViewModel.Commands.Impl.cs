using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using XamlStudio.Models;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel
    {
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

    <Grid Background=""{ThemeResource ApplicationPageBackgroundThemeBrush}"" Padding=""24"">
        <TextBlock>
            <Run FontSize=""24"" Foreground=""Green"">Get Started with XAML Studio</Run><LineBreak/>
            <Run> Modify this text below to see a live preview.</Run>
        </TextBlock>
    </Grid>
</Page>"
            });
            ////Documents.Add(new DocumentViewModel(TemplatedDocument()));

            ////SelectedDocument = Documents.Last();

            // Select Workspace Page
            ////NavigationService.Navigate(typeof(WorkspacePage));
        }

        private async void OpenDocument(RoutedEventArgs args)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
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

                // Application now has read/write access to the picked file
                var doc = await XamlDocument.LoadFromFileAsync(file);
                OpenFiles.Add(doc);

                ActiveFile = doc;
            }
        }
    }
}
