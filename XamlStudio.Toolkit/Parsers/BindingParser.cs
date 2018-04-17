using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace XamlStudio.Toolkit.Parsers
{
    public class BindingText
    {
        //
        // Summary:
        //     Gets or sets the path to the binding source property.
        //
        // Returns:
        //     The property path for the source of the binding.
        public string Path { get; set; }
        //
        // Summary:
        //     Gets or sets a value that indicates the direction of the data flow in the binding.
        //
        // Returns:
        //     One of the BindingMode values. The default is **OneWay**: the source updates
        //     the target, but changes to the target value do not update the source.
        public string Mode { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the element to use as the binding source for the Binding.
        //
        // Returns:
        //     The value of the Name property or x:Name attribute for the element you want to
        //     use as the binding source. The default is an empty string.
        public string ElementName { get; set; }
        //
        // Summary:
        //     Gets or sets a parameter that can be used in the Converter logic.
        //
        // Returns:
        //     A parameter to be passed to the Converter. This can be used in the conversion
        //     logic. The default is **null**.
        public string ConverterParameter { get; set; }
        //
        // Summary:
        //     Gets or sets a value that names the language to pass to any converter specified
        //     by the Converter property.
        //
        // Returns:
        //     A string that names a language. Interpretation of this value is ultimately up
        //     to the converter logic.
        public string ConverterLanguage { get; set; }
        //
        // Summary:
        //     Gets or sets the converter object that is called by the binding engine to modify
        //     the data as it is passed between the source and target, or vice versa.
        //
        // Returns:
        //     The IValueConverter object that modifies the data.
        public string Converter { get; set; }
        //
        // Summary:
        //     Gets or sets the data source for the binding.
        //
        // Returns:
        //     The source object that contains the data for the binding.
        public string Source { get; set; }
        //
        // Summary:
        //     Gets or sets the binding source by specifying its location relative to the position
        //     of the binding target. This is most often used in bindings within XAML control
        //     templates.
        //
        // Returns:
        //     The relative location of the binding source to use. The default is **null**.
        public string RelativeSource { get; set; }
        //
        // Summary:
        //     Gets or sets a value that determines the timing of binding source updates for
        //     two-way bindings.
        //
        // Returns:
        //     One of the UpdateSourceTrigger values. The default is **Default**, which evaluates
        //     as a **PropertyChanged** update behavior.
        public string UpdateSourceTrigger { get; set; }
        //
        // Summary:
        //     Gets or sets the value that is used in the target when the value of the source
        //     is **null**.
        //
        // Returns:
        //     The value that is used in the binding target when the value of the source is
        //     **null**.
        public string TargetNullValue { get; set; }
        //
        // Summary:
        //     Gets or sets the value to use when the binding is unable to return a value.
        //
        // Returns:
        //     The value to use when the binding is unable to return a value.
        public string FallbackValue { get; set; }
    }

    public static class BindingParser
    {
        private const string BindingPropertiesPattern = "((?<Property>(?:BindBack)|(?:Converter)|(?:ConverterLanguage)|(?:ConverterParameter)|(?:ElementName)|(?:FallbackValue)|(?:Mode)|(?:Path)|(?:RelativeSource)|(?:Source)|(?:TargetNullValue)|(?:UpdateSourceTrigger))\\s*=\\s*(?<Value>.*?(?({)({(?>{(?<DEPTH>)|}(?<-DEPTH>)|.?)*(?(DEPTH)(?!))}(?=[,}]))|(.*?(?=[,}])))))+";
        private static Regex BindingPropertyExtractor = new Regex(BindingPropertiesPattern, RegexOptions.Compiled | RegexOptions.Singleline);

        public static BindingText Parse(string binding)
        {
            BindingText text = new BindingText();

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
