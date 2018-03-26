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

        public MainViewModel()
        {
        }
    }
}
