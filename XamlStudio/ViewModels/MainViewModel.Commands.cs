using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
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
        /// <summary>
        /// Show File Dialog to Open a File.
        /// </summary>
        public ICommand OpenDocumentCommand { get; private set; }
        public ICommand SaveDocumentCommand { get; private set; }
        public ICommand SaveDocumentAsCommand { get; private set; }
        public ICommand CloseActiveDocumentCommand { get; private set; }

        /// <summary>
        /// Open a File from a <see cref="StorageFile"/>.
        /// </summary>
        public ICommand OpenFileCommand { get; private set; }

        public ICommand KeyDownCommand { get; private set; }
        public ICommand PreviousDocumentCommand { get; private set; }
        public ICommand NextDocumentCommand { get; private set; }

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand<RoutedEventArgs>(NewDocument);
            OpenDocumentCommand = new RelayCommand<RoutedEventArgs>(OpenDocument);
            SaveDocumentCommand = new RelayCommand<XamlDocument>(new Action<XamlDocument>(async (args) => { await SaveDocument(args); }));
            SaveDocumentAsCommand = new RelayCommand<XamlDocument>(new Action<XamlDocument>(async (args) => { await SaveDocumentAs(args); }));
            CloseActiveDocumentCommand = new RelayCommand<XamlDocument>(CloseActiveDocument);

            OpenFileCommand = new RelayCommand<StorageFile>(OpenFile);

            KeyDownCommand = new RelayCommand<KeyEventArgs>(KeyDown);
            PreviousDocumentCommand = new RelayCommand<RoutedEventArgs>(PreviousDocument);
            NextDocumentCommand = new RelayCommand<RoutedEventArgs>(NextDocument);
        }
    }
}
