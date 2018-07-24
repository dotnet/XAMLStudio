using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Parsers
{
    public static class BindingParser
    {
        private const string BindingPropertiesPattern = "((?<Property>(?:BindBack)|(?:Converter)|(?:ConverterLanguage)|(?:ConverterParameter)|(?:ElementName)|(?:FallbackValue)|(?:Mode)|(?:Path)|(?:RelativeSource)|(?:Source)|(?:TargetNullValue)|(?:UpdateSourceTrigger))\\s*=\\s*(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}])))))+";
        private static Regex BindingPropertyExtractor = new Regex(BindingPropertiesPattern, RegexOptions.Compiled | RegexOptions.Singleline);

        public static BindingValue Parse(string binding)
        {
            BindingValue text = new BindingValue();

            ////var isXBind = binding.Groups["Type"]?.Value == "x:Bind";
            ////var quoteChar = binding.Value[0]; // Grab the ' or " char surrounding our binding expression.
            var original = binding;

            // Copy of ongoing permutations to original binding string holder
            var newbindingstr = string.Empty + original;

            foreach (Match property in BindingPropertyExtractor.Matches(binding))
            {
                if (property.Groups["Property"]?.Value == "Converter")
                {
                    var value = property.Groups["Value"].Value;
                    var space = value.IndexOf(" ");

                    var converterkey = value.Substring(space + 1, value.Length - space - 2);

                    text.Converter = converterkey;
                }
                else if (property.Groups["Property"]?.Value == "ConverterParameter")
                {
                    // TODO: Retrieve original converter parameter if resource??? (Probably have to do same as Converter, not sure how common/capabilties
                    text.ConverterParameter = property.Groups["Value"].Value;
                }
            }

            return text;
        }
    }
}
