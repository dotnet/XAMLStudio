// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XamlStudio.Toolkit.Parsers;

public static class BindingParser
{
    private const string BindingReg = @"(Binding|x:Bind)\s+(?<Path>(Path=|)[^,=]*?)(,|})";

    private const string regProp = @"(?<Property>BindBack|Converter|ConverterLanguage|ConverterParameter|ElementName|FallbackValue|Mode|Path|RelativeSource|Source|TargetNullValue|UpdateSourceTrigger)"; // Possible Property Values

    private const string regValueCurly = @"(?<Value>{(?>{(?<DEPTH>)|}(?<-DEPTH>)|[^{}]+)*}(?(DEPTH)(?!)))"; // Grab sets of curly braces for things like static resource converter values
    private const string regValueQuote = @"(?<Value>'.*?')"; // Grab value within single quotes
    private const string regValueComma = @"((?<Value>.*?)(,|(}\Z)))"; // Grab value only escaped with \ curly quotes, but don't grab the end of binding quote.

    // Combine our above possible value patterns into one set
    private static string regValue = string.Format("({0})", string.Join("|", regValueCurly, regValueQuote, regValueComma));

    // Put together our possible Property = Value clause
    private static string BindingPropertiesPattern = string.Format("({0}\\s*=\\s*{1})+", regProp, regValue);
    private static Regex BindingPropertyExtractor = new(BindingPropertiesPattern, RegexOptions.Compiled | RegexOptions.Singleline);

    private static Dictionary<string, PropertyInfo> PropertyTable =
        typeof(BindingValue).GetProperties()
        .ToDictionary(prop => prop.Name, prop => prop);

    public static BindingValue Parse(string binding)
    {
        BindingValue text = new BindingValue();

        ////var isXBind = binding.Groups["Type"]?.Value == "x:Bind";
        ////var quoteChar = binding.Value[0]; // Grab the ' or " char surrounding our binding expression.
        var original = binding;

        // Copy of ongoing permutations to original binding string holder
        var newbindingstr = string.Empty + original;

        // Find a path if the property 'Path=' is left out
        var pathMatch = Regex.Match(binding, BindingReg);
        if (pathMatch.Success)
        {
            text.Path = pathMatch.Groups["Path"]?.Value;
        }
        // Special case '.' Bindings not in Path=
        else if (binding.StartsWith("{Binding ."))
        {
            text.Path = ".";
        }

        // Find all other properties (name on BindingValue has to match the property name in binding)
        foreach (Match property in BindingPropertyExtractor.Matches(binding))
        {
            // Set Property Value
            if (PropertyTable.TryGetValue(property.Groups["Property"]?.Value, out PropertyInfo prop))
            {
                prop.SetValue(text, property.Groups["Value"]?.Value);
            }

            // Extra work
            switch (property.Groups["Property"]?.Value)
            {
                case "Converter":
                    var value = property.Groups["Value"].Value;
                    var space = value.IndexOf(" ");

                    var converterkey = value.Substring(space + 1, value.Length - space - 2);

                    text.Converter = converterkey;
                    break;
                case "ConverterParameter":
                    // Copy to raw
                    text.ConverterParameterRaw = string.Empty + text.ConverterParameter;

                    // Need to handle escaped values.
                    if (text.ConverterParameter.StartsWith("'") &&
                        text.ConverterParameter.EndsWith("'"))
                    {
                        // Trim string start/end as we already store as string.
                        text.ConverterParameter = text.ConverterParameter.Substring(1, text.ConverterParameter.Length - 2);
                    }

                    text.ConverterParameter = text.ConverterParameter
                        .Replace("{}", "")
                        .Replace("\\{", "{")
                        .Replace("\\}", "}")
                        .Replace("\\\\", "\\")
                        .Replace("\\=", "-")
                        .Replace("\\,", ",");

                    break;
            }

        }

        return text;
    }
}
