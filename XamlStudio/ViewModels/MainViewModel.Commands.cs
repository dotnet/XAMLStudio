using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlStudio.Helpers;
using XamlStudio.Models;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand NewDocumentCommand { get; private set; }
        public ICommand OpenDocumentCommand { get; private set; }
        public ICommand SaveDocumentCommand { get; private set; }
        public ICommand CloseActiveDocumentCommand { get; private set; }

        public ICommand KeyDownCommand { get; private set; }
        public ICommand PreviousDocumentCommand { get; private set; }
        public ICommand NextDocumentCommand { get; private set; }

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand<RoutedEventArgs>(NewDocument);
            OpenDocumentCommand = new RelayCommand<RoutedEventArgs>(OpenDocument);
            SaveDocumentCommand = new RelayCommand<XamlDocument>(new Action<XamlDocument>(async (args) => { await SaveDocument(args); }));
            CloseActiveDocumentCommand = new RelayCommand<PivotItem>(CloseActiveDocument);

            KeyDownCommand = new RelayCommand<KeyEventArgs>(KeyDown);
            PreviousDocumentCommand = new RelayCommand<RoutedEventArgs>(PreviousDocument);
            NextDocumentCommand = new RelayCommand<RoutedEventArgs>(NextDocument);
        }
    }
}
