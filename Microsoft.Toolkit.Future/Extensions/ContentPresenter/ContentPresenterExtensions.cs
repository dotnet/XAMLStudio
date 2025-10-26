using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Extensions.Future
{
    [Bindable]
    public static class ContentPresenterExtensions
    {
        public static bool GetUpdateTemplateSelectorOnContentChange(ContentPresenter obj)
        {
            return (bool)obj.GetValue(UpdateTemplateSelectorOnContentChangeProperty);
        }

        public static void SetUpdateTemplateSelectorOnContentChange(ContentPresenter obj, bool value)
        {
            obj.SetValue(UpdateTemplateSelectorOnContentChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for UpdateTemplateSelectorOnContentChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateTemplateSelectorOnContentChangeProperty =
            DependencyProperty.RegisterAttached("UpdateTemplateSelectorOnContentChange", typeof(bool), typeof(ContentPresenterExtensions), new PropertyMetadata(false, OnUpdateTemplateSelectorOnContentChange));

        private static long GetContentChangedToken(ContentPresenter obj)
        {
            return (long)obj.GetValue(ContentChangedTokenProperty);
        }

        private static void SetContentChangedToken(ContentPresenter obj, long value)
        {
            obj.SetValue(ContentChangedTokenProperty, value);
        }

        // Using a DependencyProperty as the backing store for ContentChangedToken.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty ContentChangedTokenProperty =
            DependencyProperty.RegisterAttached("ContentChangedToken", typeof(long), typeof(ContentPresenterExtensions), new PropertyMetadata(0L));

        private static void OnUpdateTemplateSelectorOnContentChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cp = d as ContentPresenter;

            if (e.NewValue as bool? == true)
            {
                var token = GetContentChangedToken(cp);
                if (token != 0)
                {
                    cp.UnregisterPropertyChangedCallback(ContentPresenter.ContentProperty, token);
                }

                SetContentChangedToken(cp, cp.RegisterPropertyChangedCallback(ContentPresenter.ContentProperty,
                    (s2, e2) =>
                    {
                        var cp2 = s2 as ContentPresenter;

                        if (cp2.ContentTemplateSelector is DataTemplateSelector selector)
                        {
                            cp2.ContentTemplate = selector.SelectTemplate(cp2.Content, cp2);
                        }
                    }));
            }
        }
    }
}
