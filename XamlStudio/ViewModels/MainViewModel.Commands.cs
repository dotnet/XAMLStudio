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

        private ICommand _openDocumentCommand;
        public ICommand OpenDocumentCommand
        {
            get
            {
                if (_openDocumentCommand == null)
                {
                    _openDocumentCommand = new RelayCommand<RoutedEventArgs>(OpenDocument);
                }

                return _openDocumentCommand;
            }
        }
    }
}
