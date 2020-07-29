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
        public ICommand DuplicateDocumentCommand { get; private set; }
        /// <summary>
        /// Show File Dialog to Open a File.
        /// </summary>
        public ICommand OpenDocumentCommand { get; private set; }
        public IAsyncCommand SaveDocumentCommand { get; private set; }
        public IAsyncCommand SaveDocumentAsCommand { get; private set; }
        public IAsyncCommand CloseActiveDocumentCommand { get; private set; }

        /// <summary>
        /// Open a File from a <see cref="StorageFile"/>.
        /// </summary>
        public ICommand OpenFileCommand { get; private set; }

        public ICommand KeyDownCommand { get; private set; }
        public ICommand PreviousDocumentCommand { get; private set; }
        public ICommand NextDocumentCommand { get; private set; }

        public ICommand OpenSettingsCommand { get; private set; }
        public ICommand OpenActivityCommand { get; private set; }

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand<RoutedEventArgs>(NewDocument);
            DuplicateDocumentCommand = new RelayCommand<RoutedEventArgs>(DuplicateDocument);
            OpenDocumentCommand = new RelayCommand<RoutedEventArgs>(OpenDocument);
            SaveDocumentCommand = new AsyncRelayCommand<XamlDocument>(SaveDocument);
            SaveDocumentAsCommand = new AsyncRelayCommand<XamlDocument>(SaveDocumentAs);
            CloseActiveDocumentCommand = new AsyncRelayCommand<XamlDocument>(CloseActiveDocument);

            OpenFileCommand = new RelayCommand<StorageFile>(OpenFile);

            KeyDownCommand = new RelayCommand<KeyEventArgs>(KeyDown);
            PreviousDocumentCommand = new RelayCommand<RoutedEventArgs>(PreviousDocument);
            NextDocumentCommand = new RelayCommand<RoutedEventArgs>(NextDocument);

            OpenSettingsCommand = new RelayCommand<RoutedEventArgs>(OpenSettingsPage);
            OpenActivityCommand = new RelayCommand<string>(OpenActivityPanel);
        }
    }
}
