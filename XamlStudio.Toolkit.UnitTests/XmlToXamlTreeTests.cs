using CommunityToolkit.Tests;
using CommunityToolkit.WinUI;
using Microsoft.Language.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.Toolkit.UnitTests;

[TestClass]
public class XmlToXamlTreeTests : VisualUITestBase
{
    [TestMethod]
    public async Task XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
            """
            <Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"              
                  x:Name="ToolboxPage">

                <Page.Resources>
                    <Style x:Key="ListViewItemContainerWideStyle" TargetType="ListViewItem">
                        <Setter Property="HorizontalContentAlignment" Value="Stretch" />
                    </Style>
                </Page.Resources>

                <Grid Background="Blue"
                      x:Name="RootGrid">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="54" />
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="*" />
                    </Grid.RowDefinitions>
                    <Button Content="First"
                            Margin="4"/>
                    <Button>
                        <TextBlock Text="Second"/>
                    </Button>
                </Grid>
            </Page>
            """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            // TODO: Make some assertions...
            var grid = fwe.FindChild("RootGrid");
            Assert.IsTrue(coordinator.TryGetXmlElement(grid, out var gridNode));
            if (gridNode is IXmlElementSyntax gridElement)
            {
                Assert.AreEqual("RootGrid", gridElement.GetAttributeValue("Name", "x"));
                Assert.AreEqual("Blue", gridElement.GetAttributeValue("Background"));
            }
            else
            {
                Assert.Fail("Xml Node not an Element for Grid");
            }

            var textblockNode = xml.FindNode(xaml.IndexOf("<TextBlock") + 1).ParentElement;
            if (textblockNode is IXmlElementSyntax textblockElement)
            {
                Assert.IsTrue(coordinator.TryGetVisualElement(textblockElement, out var dotbe));
                if (dotbe is TextBlock textblock)
                { 
                    Assert.AreEqual("Second", textblock.Text);
                }
                else
                {
                    Assert.Fail("Failed to retrieve TextBlock from Xml Element");
                }
            }
            else
            {
                Assert.Fail("Xml Node not an Element for TextBlock");
            }

            await UnloadTestContentAsync(fwe);
        });                
    }
}