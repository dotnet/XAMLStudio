using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Parsers;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.UnitTests;

public partial class XamlRenderServiceTests
{
#if DEBUG
    [TestMethod]
    public async Task UnwrapApplicationResourceDictionary_Basic()
    {
        var xamlInput =
            """
            <Application
                x:Class="XSTestFolder.App"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:XSTestFolder">

                <Application.Resources>
                    <Style TargetType="Button">
                        <Setter Property="Background" Value="CornflowerBlue"/>
                        <Setter Property="FontSize" Value="24"/>
                        <Setter Property="HorizontalAlignment" Value="Right"/>
                    </Style>
                </Application.Resources>
            </Application>
            """;

        var expectedOutput =
            """
            <ResourceDictionary

                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:XSTestFolder">
            

                    <Style TargetType="Button">
                        <Setter Property="Background" Value="CornflowerBlue"/>
                        <Setter Property="FontSize" Value="24"/>
                        <Setter Property="HorizontalAlignment" Value="Right"/>
                    </Style>

            </ResourceDictionary>
            """;

        // Load extra Metadata about other available types.
        if (!AppAssemblyInfo.Instance.IsLoaded)
        {
            await AppAssemblyInfo.Instance.InitializeAsync();
        }

        XamlRenderResultContext context = new(xamlInput);
        XamlRenderSettings settings = new();

        XamlRenderService.PreProcessXmlns(ref context, ref settings);

        Assert.AreEqual(expectedOutput, context.RenderedContent);
    }

    // TODO: Add test with more complex scenario of a merged dictionary...
#endif
}
