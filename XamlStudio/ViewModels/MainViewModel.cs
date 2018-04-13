using System;

using XamlStudio.Helpers;
using XamlStudio.Models;

namespace XamlStudio.ViewModels
{
    public partial class MainViewModel : WorkspaceWindow
    {
        /// <summary>
        /// Keeps track of number of untitled documents we've created this session.
        /// </summary>
        private int _untitledCount = 1;

        private DocumentViewModel _documentVM;
        public DocumentViewModel DocumentViewModel
        {
            get { return _documentVM; }
            set { Set(ref _documentVM, value); }
        }

        private SettingsPanelViewModel _settingsVM = new SettingsPanelViewModel();
        public SettingsPanelViewModel SettingsViewModel => _settingsVM;
    }
}
