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
    public async Task Basic_XmlToXamlTest()
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

            Assert.AreEqual(5, coordinator.Count, "Expected 5 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });                
    }

    [TestMethod]
    public async Task NestedParentTypes_XmlToXamlTest()
    {
        await EnqueueAsync(async() =>
        {
            var xaml =
                """
                <Page
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="using:TestApp"
                    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                    mc:Ignorable="d"
                    d:DataContext="{d:DesignData /SampleData/XAMLing.json, Type=local:XamlingInfo}"
                    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"><!-- Note: d:DesignData is crashing designer currently, opened issue. -->

                    <StackPanel>
                        <TextBlock FontSize="36">
                            <Run FontSize="96" Foreground="#FFFC5185">#XAMLing</Run><LineBreak/>
                            <Run>/ˈzæməlɪŋ/</Run><LineBreak/>
                            <Run FontStyle="Italic">verb</Run> <Run>[With XAML Studio]</Run><LineBreak/>
                        </TextBlock>
                        <ItemsControl ItemsSource="{Binding Items}" Margin="20,-12,0,0">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <TextBlock Margin="0,0,0,24" FontSize="24">
                                        <Run Text="{Binding Index}"/>. <Run Text="{Binding Definition}"/>
                                    </TextBlock>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        <StackPanel>
                            <Image Source="/Assets/LlamaCircle.png" Width="64" Height="64"/>
                            <TextBlock Text="- The XAML Llama" VerticalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Page>                
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var sp1Index = xaml.IndexOf("<StackPanel>");
            var sp1Node = xml.FindNode(sp1Index + 1).ParentElement;
            var sp2Node = xml.FindNode(xaml.IndexOf("<StackPanel>", sp1Index + 1) + 1).ParentElement;

            var sp1 = fwe.FindChild<StackPanel>();
            Assert.IsTrue(coordinator.TryGetXmlElement(sp1, out var sp1NodeRetrieved), "First StackPanel Xml node wasn't Element");
            Assert.AreEqual(sp1Node, sp1NodeRetrieved, "First StackPanel XML Element didn't match.");

            var sp2 = sp1.FindChild<StackPanel>();
            Assert.IsTrue(coordinator.TryGetXmlElement(sp2, out var sp2NodeRetrieved), "Second StackPanel Xml node wasn't Element");
            Assert.AreEqual(sp2Node, sp2NodeRetrieved, "Second StackPanel XML Element didn't match.");

            Assert.AreEqual(7, coordinator.Count, "Expected 7 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }
}