using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.UnitTests
{
    [TestClass]
    public class XamlRenderServiceTests
    {
        #if DEBUG
        [TestMethod]
        public void BindingInjectTest_NoPath()
        {
            var binding = "{Binding}";
            var info = new XamlBindingInfo(1, 1, binding);
            var expected = new BindingValue();
            var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

            Assert.AreEqual("{Binding Converter={StaticResource XamlBindingWrapper},ConverterParameter=" + info.Id + "}", output);
        }

        [TestMethod]
        public void BindingInjectTest_RelativeSource()
        {
            var binding = "{Binding DataContext, RelativeSource={RelativeSource Self}}";
            var info = new XamlBindingInfo(1, 1, binding);
            var expected = new BindingValue()
            {
                RelativeSource = "{RelativeSource Self}",
                Path = "DataContext"
            };
            var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

            Assert.AreEqual("{Binding DataContext, RelativeSource={RelativeSource Self},Converter={StaticResource XamlBindingWrapper},ConverterParameter=" + info.Id + "}", output);
        }
        #endif
    }
}
