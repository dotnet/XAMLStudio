using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace CommunityToolkit.WinUI.Extensions.Future
{
    public static partial class Bind
    {
        private static ResourceLoader _resLoader = ResourceLoader.GetForCurrentView();

        public static string LocalizedString(string resourceKey)
        {
            return _resLoader.GetString(resourceKey);
        }

        public static Visibility NotVisible(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility Visible(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
