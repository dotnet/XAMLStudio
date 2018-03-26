using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using XamlStudio.Helpers;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel
    {
        private ICommand _newDocumentCommand;
        public ICommand NewDocumentCommand
        {
            get
            {
                if (_newDocumentCommand == null)
                {
                    _newDocumentCommand = new RelayCommand<RoutedEventArgs>(NewDocument);
                }

                return _newDocumentCommand;
            }
        }

        private void NewDocument(RoutedEventArgs args)
        {
            OpenFiles.Add(new Models.XamlDocument("Untitled-" + _untitledCount++)
            {
                Content =
@"<Page
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
    xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
    mc:Ignorable=""d"">

    <Grid Background=""{ThemeResource ApplicationPageBackgroundThemeBrush}"">
        <TextBlock>
            <Run FontSize=""24"">Get Started with XAML Studio</Run><LineBreak/>
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
    }
}
