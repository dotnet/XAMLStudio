using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace XamlStudio.Services
{
    public interface IFileOpener
    {
        void OpenFileItems(IStorageItem[] file);
    }
}
