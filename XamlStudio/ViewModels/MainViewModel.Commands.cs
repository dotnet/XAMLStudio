using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Helpers;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand NewDocumentCommand { get; private set; }
        public ICommand OpenDocumentCommand { get; private set; }
        public ICommand CloseActiveDocumentCommand { get; private set; }

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand<RoutedEventArgs>(NewDocument);
            OpenDocumentCommand = new RelayCommand<RoutedEventArgs>(OpenDocument);
            CloseActiveDocumentCommand = new RelayCommand<PivotItem>(CloseActiveDocument);
        }
    }
}
