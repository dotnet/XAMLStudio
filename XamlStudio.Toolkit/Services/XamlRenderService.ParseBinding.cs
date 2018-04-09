using System.Linq;
using System.Text.RegularExpressions;
using Windows.Storage;
using XamlStudio.Toolkit.Models;

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

                // Need 'x' namespace for resource key in our converter wrapper...
                if (!context.RenderedContent.Contains("xmlns:x"))
                {
                    // Find the end of the first tag
                    var oti = context.RenderedContent.IndexOf(">");
                    if (oti != -1)
                    {
                        context.RenderedContent = context.RenderedContent.Substring(0, oti) + @" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""" + context.RenderedContent.Substring(oti);
                    }
                }

                // Inject our extension namespace (if needed), so we have access to our Binding Wrapper.
                if (!context.RenderedContent.Contains("xmlns:xstc="))
                {
                    // Find the end of the first tag
                    var oti = context.RenderedContent.IndexOf(">");
                    if (oti != -1)
                    {
                        context.RenderedContent = context.RenderedContent.Substring(0, oti) + @" xmlns:xstc=""using:XamlStudio.Toolkit.Converters""" + context.RenderedContent.Substring(oti);
                    }
                }

                var resourceSearch = "<" + typename + ".Resources>";
                const string converter = "<xstc:XamlBindingWrapperConverter x:Key=\"XamlBindingWrapper\"/>";

                if (context.RenderedContent.IndexOf(resourceSearch) != -1)
                {
                    context.RenderedContent = context.RenderedContent.Replace(resourceSearch, resourceSearch + converter);
                }
                else
                {
                    // TODO: should use preparser stuff here for end of tag.  
                    // BUGBUG: Also need to account for a closed tag...
                    // If we don't have an existing resource section, add one right after our initial type tag.
                    var oti = context.RenderedContent.IndexOf(">");
                    if (oti != -1)
                    {
                        context.RenderedContent = context.RenderedContent.Substring(0, oti + 1) + resourceSearch + converter + "</" + typename + ".Resources>" + context.RenderedContent.Substring(oti + 1);
                    }
                }
            }

            int offset = 0;

            foreach (Match binding in BindingSearcher.Matches(context.RenderedContent))
            {
                var isXBind = binding.Groups["Type"]?.Value == "x:Bind";
                var quoteChar = binding.Value[0]; // Grab the ' or " char surrounding our binding expression.

                var original = binding.Value;

                // Calculate Editor Based Position // TODO: Make sure we're not out of line with earlier modification steps
                uint line = 1 + (uint)context.RenderedContent.Substring(0, binding.Index + offset).Count(c => c == '\n');
                var position = binding.Index + offset - context.RenderedContent.LastIndexOf('\n', binding.Index + offset);

                var bindingInfo = new Models.XamlBindingInfo(line, (uint)position, original);

                XamlBindingWrapperManager.Instance.AddNewBinding(this.Id, bindingInfo);

                const string newBinding = "{StaticResource XamlBindingWrapper}";
                var foundConverter = false;
                var foundConverterParameter = false;

                // Copy of ongoing permutations to original binding string holder
                var newbindingstr = string.Empty + original;

                foreach (Match property in BindingPropertyExtractor.Matches(binding.Value))
                {
                    if (property.Groups["Property"]?.Value == "Converter")
                    {
                        foundConverter = true;

                        var value = property.Groups["Value"].Value;
                        var space = value.IndexOf(" ");

                        var converterkey = value.Substring(space + 1, value.Length - space - 2);

                        bindingInfo.ConverterKey = converterkey;

                        // Replace converter with our new one
                        var str = newbindingstr.Replace(property.Groups["Value"].Value, newBinding);

                        // Inject back to original string
                        context.RenderedContent = context.RenderedContent.Replace(newbindingstr, str);

                        // Update positions for next strings
                        offset += (str.Length - newbindingstr.Length);

                        newbindingstr = str;
                    }
                    else if (property.Groups["Property"]?.Value == "ConverterParameter")
                    {
                        foundConverterParameter = true;

                        // TODO: Retrieve original converter parameter if resource??? (Probably have to do same as Converter, not sure how common/capabilties
                        bindingInfo.ConverterParameter = property.Groups["Value"].Value;

                        // Our new converter parameter is 'Id{Binding ...}'
                        var str = newbindingstr.Replace(property.Groups["Value"].Value, string.Empty + bindingInfo.Id);

                        // Inject back to original string
                        context.RenderedContent = context.RenderedContent.Replace(newbindingstr, str);

                        // Update positions for next strings
                        offset += (str.Length - newbindingstr.Length);

                        newbindingstr = str;
                    }
                }

                if (!foundConverter)
                {
                    // TODO: BUGBUG need to remember changes to string above too, as don't know if we had a parameter without a converter (odd?)
                    // If no converter on binding, add ours
                    var str = binding.Value.Substring(0, binding.Value.Length - 2) + ",Converter=" + newBinding + "}" + quoteChar;

                    context.RenderedContent = context.RenderedContent.Replace(newbindingstr, str);

                    // Update positions for next strings
                    offset += (str.Length - newbindingstr.Length);

                    newbindingstr = str;
                }

                if (!foundConverterParameter)
                {
                    var str = newbindingstr.Substring(0, newbindingstr.Length - 2) + ",ConverterParameter=" + bindingInfo.Id + "}" + quoteChar;

                    // If no converterparameter on binding, add ours
                    context.RenderedContent = context.RenderedContent.Replace(newbindingstr, str);

                    // Update positions for next strings
                    offset += (str.Length - newbindingstr.Length);
                }
            }
        }
    }
}
