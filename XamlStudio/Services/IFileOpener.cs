using Windows.Storage;

namespace XamlStudio.Services
{
    public interface IFileOpener
    {
        void OpenFileItems(IStorageItem[] file);
    }
}
