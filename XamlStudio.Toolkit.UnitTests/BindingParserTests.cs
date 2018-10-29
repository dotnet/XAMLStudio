using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XamlStudio.Toolkit.Parsers;

namespace XamlStudio.Toolkit.UnitTests
{
    [TestClass]
    public class BindingParserTests
    {
        [TestMethod]
        public void BindingTest_Converter1()
        {
            string xaml = @"{Binding IsOn, ElementName=AutoCompileToggle, Converter={StaticResource StringFormatConverter}, ConverterParameter='Is Loading: {0}'}";
            var expected = new BindingValue()
            {
                ElementName = "AutoCompileToggle",
                Path = "IsOn",
                Converter = "StringFormatConverter",
                ConverterParameter = "'Is Loading: {0}'"
            };
            var binding = BindingParser.Parse(xaml);
            TestBinding(expected, binding);
            PrintBinding(xaml, binding);
        }

        [TestMethod]
        public void BindingTest_Converter2()
        {
            string xaml = @"{Binding RangeMin, ElementName=RangeSelector, Converter={StaticResource StringFormatConverter}, ConverterParameter='{}{0:0.##}'}";
            var expected = new BindingValue()
            {
                Path = "RangeMin",
                ElementName = "RangeSelector",
                Converter = "StringFormatConverter",
                ConverterParameter = "'{}{0:0.##}'"
            };
            var binding = BindingParser.Parse(xaml);
            TestBinding(expected, binding);
            PrintBinding(xaml, binding);
        }

        [TestMethod]
        public void BindingTest_Path1()
        {
            string xaml = @"{Binding ElementName=AutoCompileToggle, Path=IsOn}";
            var expected = new BindingValue()
            {
                ElementName = "AutoCompileToggle",
                Path = "IsOn"
            };
            var binding = BindingParser.Parse(xaml);
            TestBinding(expected, binding);
            PrintBinding(xaml, binding);
        }

        [TestMethod]
        public void BindingTest_Path2()
        {
            string xaml = @"{x:Bind local:myprop, ElementName=AutoCompileToggle}";
            var expected = new BindingValue()
            {
                ElementName = "AutoCompileToggle",
                Path = "local:myprop"
            };
            var binding = BindingParser.Parse(xaml);
            TestBinding(expected, binding);
            PrintBinding(xaml, binding);
        }

        private static void TestBinding(BindingValue expected, BindingValue actual)
        {
            foreach (var prop in expected.GetType().GetProperties())
            {
                Debug.WriteLine("Comparing {0} : {1} to {2}", prop.Name, prop.GetValue(expected), prop.GetValue(actual));
                Assert.AreEqual(prop.GetValue(expected), prop.GetValue(actual),
                    $"{prop.Name} is not equal: expected {prop.GetValue(expected)}, actual: {prop.GetValue(actual)}");
            }
        }

        private static void PrintBinding(string xaml, BindingValue binding)
        {
            Debug.WriteLine(xaml);
            Debug.WriteLine("--------------");

            foreach (var prop in binding.GetType().GetProperties())
            {
                Debug.WriteLine("{0} : {1}", prop.Name, prop.GetValue(binding));
            }
            Debug.WriteLine(binding.Converter);
            Debug.WriteLine("--------------\n\n");
        }
    }
}
