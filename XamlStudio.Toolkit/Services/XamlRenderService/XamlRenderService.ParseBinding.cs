using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;

namespace XamlStudio.Toolkit.Services
{
    //// Helpers for parsing Binding Expressions in pre-processing steps.

    public partial class XamlRenderService
    {
        private const string BindingSearcherPattern = "([\"']){\\s*(?<Type>(?:Binding)|(?:x:Bind)).*?}\\1"; // \1 matches initial single or double quote used in first capturing group.
        private const string BindingPropertiesPattern = "((?<Property>(?:BindBack)|(?:Converter)|(?:ConverterLanguage)|(?:ConverterParameter)|(?:ElementName)|(?:FallbackValue)|(?:Mode)|(?:Path)|(?:RelativeSource)|(?:Source)|(?:TargetNullValue)|(?:UpdateSourceTrigger))\\s*=\\s*(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}])))))+";
        private static Regex BindingSearcher = new Regex(BindingSearcherPattern, RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex BindingPropertyExtractor = new Regex(BindingPropertiesPattern, RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Replace Binding Expressions with equivalents but intercepted by our own converter for additional logic/redirection.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private void InterceptBindings(ref XamlRenderResultContext context)
        {
            // Need to inject Converter Resource into FrameworkElement
            if (context.ElementType != null)
            {
                var typename = context.ElementType.Name;

                // TODO: use result value from already calculated point.
                if (!context.IsFrameworkElement)
                {
                    // We can't inject resources (like our binding wrapper) into non-framework elements.
                    return;
                }

                // TODO: Need to force inject these to RenderedContent in PreProcessXmlns step.

                // Find the end of the first tag
                var match = GetInitialElement(context.RenderedContent);

                if (match?.Success == true)
                {
                    // Need 'x' namespace for resource key in our converter wrapper...
                    if (!context.RenderedContent.Contains("xmlns:x"))
                    {
                        // Find the end of the first tag
                        var oti = context.RenderedContent.IndexOf(">", match.Index + 1);
                        if (oti != -1)
                        {
                            context.RenderedContent = context.RenderedContent.Substring(0, oti) + @" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""" + context.RenderedContent.Substring(oti);
                        }
                    }

                    // Inject our extension namespace (if needed), so we have access to our Binding Wrapper.
                    if (!context.RenderedContent.Contains("xmlns:xstc="))
                    {
                        // Find the end of the first tag
                        var oti = context.RenderedContent.IndexOf(">", match.Index + 1);
                        if (oti != -1)
                        {
                            context.RenderedContent = context.RenderedContent.Substring(0, oti) + @" xmlns:xstc=""using:XamlStudio.Toolkit.Converters""" + context.RenderedContent.Substring(oti);
                        }
                    }
                }

                var resourceSearch = "<" + typename + ".Resources>";
                const string converter = "<xstc:XamlBindingWrapperConverter x:Key=\"XamlBindingWrapper\"/>";

                if (context.RenderedContent.IndexOf(resourceSearch) != -1)
                {
                    context.RenderedContent = context.RenderedContent.Replace(resourceSearch, resourceSearch + converter);
                }
                else if (match?.Success == true)
                {
                    // TODO: should use preparser stuff here for end of tag.  
                    // BUGBUG: Also need to account for a single closed tag...
                    // If we don't have an existing resource section, add one right after our initial type tag.
                    var oti = context.RenderedContent.IndexOf(">", match.Index);
                    if (oti != -1)
                    {
                        context.RenderedContent = context.RenderedContent.Substring(0, oti + 1) + resourceSearch + converter + "</" + typename + ".Resources>" + context.RenderedContent.Substring(oti + 1);
                    }
                }
                else
                {
                    // TODO: If we don't know the first tag??? Don't think this should happen?
                }
            }
        }

        // Given all binding info, return a new binding string with our shim injected.
        internal static string InjectBindingConverter(string original, BindingValue binding, XamlBindingInfo info)
        {
            const string converterShim = "{StaticResource XamlBindingWrapper}";
            var foundConverter = !string.IsNullOrWhiteSpace(binding.Converter);
            var foundConverterParameter = !string.IsNullOrWhiteSpace(binding.ConverterParameter);

            if (foundConverter)
            {
                // TODO: Do I need to worry about it not being "{StaticResource"?
                original = original.Replace(binding.Converter, "XamlBindingWrapper");
            }
            else
            {
                char separator = ',';
                // If no converter on binding, add ours
                if (string.IsNullOrWhiteSpace(binding.Path))
                {
                    separator = ' ';
                }
                original = original.Substring(0, original.Length - 1) + separator + "Converter=" + converterShim + "}";
            }

            if (foundConverterParameter)
            {
                original = original.Replace(binding.ConverterParameter, string.Empty + info.Id);
            }
            else
            {
                original = original.Substring(0, original.Length - 1) + ",ConverterParameter=" + info.Id + "}";
            }

            return original;
        }
    }
}
