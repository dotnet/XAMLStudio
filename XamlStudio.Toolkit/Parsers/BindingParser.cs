using System.Linq;
using System.Text.RegularExpressions;

namespace XamlStudio.Toolkit.Parsers
{
    public static class BindingParser
    {
        private const string BindingReg = "{Binding (?<Binding>.*)}";
        private const string regProp = "(?<Property>BindBack|Converter|ConverterLanguage|ConverterParameter|ElementName|FallbackValue|Mode|Path|RelativeSource|Source|TargetNullValue|UpdateSourceTrigger)";
        private const string regValueCurly = "(?<Value>{(?>{(?<DEPTH>)|}(?<-DEPTH>)|[^{}]+)*}(?(DEPTH)(?!)))"; //"(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}]))))";
        private const string regValueQuote = "(?<Value>'.*?')"; //"(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}]))))";
        private static string regValue = string.Format("({0}|{1})", regValueCurly, regValueQuote);
        private static string BindingPropertiesPattern = string.Format("({0}\\s*=\\s*{1})+", regProp, regValue);
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
                var names = from n in text.GetType().GetProperties() select n.Name;

                names.ToList().ForEach(
                        x =>
                        {
                            if (property.Groups["Property"]?.Value == x)
                            {
                                text.GetType().GetProperty(x).SetValue(text, property.Groups["Value"].Value);
                            }
                        }
                    );

                if (property.Groups["Property"]?.Value == "Converter")
                {
                    var value = property.Groups["Value"].Value;
                    var space = value.IndexOf(" ");

                    var converterkey = value.Substring(space + 1, value.Length - space - 2);

                    text.Converter = converterkey;
                }
            }

            return text;
        }
    }
}
