using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using XamlStudio.Models;
using XamlStudio.ViewModels;

namespace XamlStudio.Views
{
    /// <summary>
    /// Returns the <see cref="DocumentViewModel"/> object given the <see cref="XamlDocument"/>.
    /// </summary>
    public class DocumentToViewModelConverter : IValueConverter
    {
        public MainViewModel MainViewModel { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is XamlDocument xd)
            {
                if (MainViewModel.DocumentViewModels.ContainsKey(xd))
                {
                    return MainViewModel.DocumentViewModels[xd];
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
