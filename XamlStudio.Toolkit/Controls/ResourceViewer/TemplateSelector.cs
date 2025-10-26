using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XamlStudio.Toolkit.Controls
{
    /// <summary>
    /// Tries to locate a <see cref="DataTemplate"/> from the provided Resources <see cref="ResourceDictionary"/>.
    /// Uses the key of the objects Type's Name + 'Template'
    /// </summary>
    public class TemplateSelector : DataTemplateSelector
    {
        private const string TemplateSuffix = "Template";

        public ResourceDictionary Resources { get; set; }

        public TemplateSelector()
        {
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return LookupDataTemplate(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return LookupDataTemplate(item);
        }

        private DataTemplate LookupDataTemplate(object item)
        {
            if (item != null)
            {
                var typename = item.GetType().Name;

                if (Resources != null &&
                    Resources.TryGetValue(typename + TemplateSuffix, out object obj) &&
                    obj is DataTemplate template)
                {
                    return template;
                }
            }

            return null;
        }
    }
}
