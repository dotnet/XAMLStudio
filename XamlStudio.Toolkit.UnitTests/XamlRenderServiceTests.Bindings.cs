// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.UnitTests;

[TestClass]
public partial class XamlRenderServiceTests
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
    public void BindingInjectTest_DotPath()
    {
        var binding = "{Binding .}";
        var info = new XamlBindingInfo(1, 1, binding);
        var expected = new BindingValue()
        {
            Path = "."
        };
        var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

        Assert.AreEqual("{Binding .,Converter={StaticResource XamlBindingWrapper},ConverterParameter=" + info.Id + "}", output);
    }

    [TestMethod]
    public void BindingInjectTest_DotPath2()
    {
        var binding = "{Binding Path=.}";
        var info = new XamlBindingInfo(1, 1, binding);
        var expected = new BindingValue()
        {
            Path = "."
        };
        var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

        Assert.AreEqual("{Binding Path=.,Converter={StaticResource XamlBindingWrapper},ConverterParameter=" + info.Id + "}", output);
    }

    [TestMethod]
    public void BindingInjectTest_Converter()
    {
        var binding = "{Binding IsOn, Converter={StaticResource BooleanToVisibilityConverter}}";
        var info = new XamlBindingInfo(1, 1, binding);
        var expected = new BindingValue()
        {
            Path = "IsOn",
            Converter = "BooleanToVisibilityConverter"
        };
        var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

        Assert.AreEqual("{Binding IsOn, Converter={StaticResource XamlBindingWrapper},ConverterParameter=" + info.Id + "}", output);
    }

    [TestMethod]
    public void BindingInjectTest_ConverterParameterValue()
    {
        var binding = @"{Binding RangeMin, ElementName=RangeSelector, Converter={StaticResource StringFormatConverter}, ConverterParameter='{0:0.##}'}";
        var info = new XamlBindingInfo(1, 1, binding);
        var expected = new BindingValue()
        {
            Path = "RangeMin",
            ElementName = "RangeSelector",
            Converter = "StringFormatConverter",
            ConverterParameter = "{0:0.##}",
            ConverterParameterRaw = "'{0:0.##}'"
        };
        var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

        Assert.AreEqual("{Binding RangeMin, ElementName=RangeSelector, Converter={StaticResource XamlBindingWrapper}, ConverterParameter=" + info.Id + "}", output);
    }

    [TestMethod]
    public void BindingInjectTest_ConverterParameterValue2()
    {
        var binding = @"{Binding RangeMin, ElementName=RangeSelector, Converter={StaticResource StringFormatConverter}, ConverterParameter=\{0:0.##\}}";
        var info = new XamlBindingInfo(1, 1, binding);
        var expected = new BindingValue()
        {
            Path = "RangeMin",
            ElementName = "RangeSelector",
            Converter = "StringFormatConverter",
            ConverterParameter = "{0:0.##}",
            ConverterParameterRaw = "\\{0:0.##\\}"
        };
        var output = XamlRenderService.InjectBindingConverter(binding, expected, info);

        Assert.AreEqual("{Binding RangeMin, ElementName=RangeSelector, Converter={StaticResource XamlBindingWrapper}, ConverterParameter=" + info.Id + "}", output);
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
