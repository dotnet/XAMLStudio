using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
