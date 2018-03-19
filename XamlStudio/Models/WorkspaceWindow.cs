using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Models
{
    public class WorkspaceWindow
    {
        public StorageFolder Folder { get; private set; }


        public static WorkspaceWindow GetDefaultWorkspace()
        {
            throw new NotImplementedException();
        }
    }
}
