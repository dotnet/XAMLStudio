using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using XamlStudio.Helpers;

namespace XamlStudio.Models
{
    public class DocumentState : SimpleObservable
    {
        private PaneOrientation? _paneOrientation;
        public PaneOrientation? PreviewOrientation
        {
            get { return _paneOrientation; }
            set { Set(ref _paneOrientation, value); }
        }

        private ElementTheme? _previewAreaTheme;
        public ElementTheme? PreviewAreaTheme
        {
            get { return _previewAreaTheme; }
            set { Set(ref _previewAreaTheme, value); }
        }
    }
}
