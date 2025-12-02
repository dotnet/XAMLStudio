using CommunityToolkit.Tests;
using CommunityToolkit.WinUI;
using Microsoft.Language.Xml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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

                    <Grid x:Name="RootGrid">
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

            var grid = fwe.FindChild("RootGrid");
            Assert.IsTrue(coordinator.TryGetXmlElement(grid, out var gridNode));
            if (gridNode is IXmlElementSyntax gridElement)
            {
                Assert.AreEqual("RootGrid", gridElement.GetAttributeValue("Name", "x"));
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
        await EnqueueAsync(async () =>
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
                    d:DataContext="{d:DesignData /SampleData/XAMLing.json, Type=local:XamlingInfo}">

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

    [TestMethod]
    public async Task ColorConversion_XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
                """
                <Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

                    <StackPanel>
                        <Border Background="Blue"/>
                        <Border Background="#FFFF0000"/>
                        <Border Background="#0000FF"/>
                    </StackPanel>
                </Page>
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var sp = fwe.FindChild<StackPanel>();
            Assert.IsTrue(coordinator.TryGetXmlElement(sp, out var spNode));
            if (spNode is not IXmlElementSyntax)
            {
                Assert.Fail("Xml Node not an Element for StackPanel");
            }

            var colorMatches = new[]
            {
                Colors.Blue,
                Color.FromArgb(255, 255, 0, 0),
                Color.FromArgb(255, 0, 0, 255),
            };
            var i = 0;
            var j = 0;

            Assert.AreEqual(3, sp.Children.Count, "Expected StackPanel to have 3 children");
            foreach (var child in sp.Children)
            {
                i = xaml.IndexOf("<Border", i) + 1;
                var node = xml.FindNode(i).ParentElement;
                if (node is IXmlElementSyntax nodeElement)
                {
                    Assert.IsTrue(coordinator.TryGetVisualElement(nodeElement, out var element));
                    if (element is Border border
                        && element == child)
                    {
                        Assert.AreEqual(colorMatches[j++], (border.Background as SolidColorBrush).Color, $"Unexpected color for Border[{j - 1}]");
                    }
                    else
                    {
                        Assert.Fail("Unexpected Child Border element");
                    }
                }
            }

            Assert.AreEqual(5, coordinator.Count, "Expected 5 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }

    [TestMethod]
    public async Task ImageConversion_XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
                """
                <Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                
                    <StackPanel>
                        <Image Source="ms-appx:///Assets/StoreLogo.png"/>
                        <Image Source="ms-appx:///Assets/SplashScreen.png"/>
                        <Image Source="/Assets/StoreLogo.png"/>
                        <Image Source="https://picsum.photos/200/200"/>
                    </StackPanel>
                </Page>
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var sp = fwe.FindChild<StackPanel>();
            Assert.IsTrue(coordinator.TryGetXmlElement(sp, out var spNode));
            if (spNode is not IXmlElementSyntax)
            {
                Assert.Fail("Xml Node not an Element for StackPanel");
            }

            var uriMatches = new[]
            {
                "ms-appx:///Assets/StoreLogo.png",
                "ms-appx:///Assets/SplashScreen.png",
                "ms-resource:///Files/Assets/StoreLogo.png", // Gets converted to ms-resource:// path prepended with /Files by System
                "https://picsum.photos/200/200",
            };
            var i = 0;
            var j = 0;

            Assert.AreEqual(4, sp.Children.Count, "Expected StackPanel to have 3 children");
            foreach (var child in sp.Children)
            {
                i = xaml.IndexOf("<Image", i) + 1;
                var node = xml.FindNode(i).ParentElement;
                if (node is IXmlElementSyntax nodeElement)
                {
                    Assert.IsTrue(coordinator.TryGetVisualElement(nodeElement, out var element));
                    if (element is Image image
                        && element == child)
                    {
                        Assert.AreEqual(uriMatches[j++], (image.Source as BitmapImage).UriSource.AbsoluteUri, $"Unexpected uri for Image[{j - 1}]");
                    }
                    else
                    {
                        Assert.Fail("Unexpected Child Image element");
                    }
                }
            }

            Assert.AreEqual(6, coordinator.Count, "Expected 5 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }

    [TestMethod]
    public async Task NamespacedControl_XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
                """
                <Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:muxc="using:Microsoft.UI.Xaml.Controls"
                      x:Name="ToolboxPage">

                    <muxc:TabView x:Name="MyTabView">
                        <muxc:TabViewItem Header="Tab 1">
                            <TextBlock Text="Content for Tab 1"/>
                        </muxc:TabViewItem>
                        <muxc:TabViewItem Header="Tab 2">
                            <TextBlock Text="Content for Tab 2"/>
                        </muxc:TabViewItem>
                    </muxc:TabView>
                </Page>
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var tabview = fwe.FindChild("MyTabView");
            Assert.IsTrue(coordinator.TryGetXmlElement(tabview, out var tabviewNode));
            if (tabviewNode is IXmlElementSyntax tabviewElement)
            {
                Assert.AreEqual("MyTabView", tabviewElement.GetAttributeValue("Name", "x"));
            }
            else
            {
                Assert.Fail("Xml Node not an Element for TabView");
            }

            var tabviewitemNode = xml.FindNode(xaml.IndexOf("<muxc:TabViewItem") + 1).ParentElement;
            if (tabviewitemNode is IXmlElementSyntax tabviewitemElement)
            {
                Assert.IsTrue(coordinator.TryGetVisualElement(tabviewitemElement, out var dotbe));
                if (dotbe is TabViewItem tabViewItem)
                {
                    Assert.AreEqual("Tab 1", tabViewItem.Header);
                }
                else
                {
                    Assert.Fail("Failed to retrieve TabViewItem from Xml Element");
                }
            }
            else
            {
                Assert.Fail("Xml Node not an Element for TabViewItem");
            }

            Assert.AreEqual(6, coordinator.Count, "Expected 6 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }

    [TestMethod]
    public async Task ListViewSimple_XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
                """
                <Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                      xmlns:muxc="using:Microsoft.UI.Xaml.Controls"
                      x:Name="ToolboxPage">

                    <ListView x:Name="MyListView">
                        <ListViewItem>
                            <TextBlock Text="Content for Item 1"/>
                        </ListViewItem>
                    </ListView>
                </Page>
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var listview = fwe.FindChild("MyListView");
            Assert.IsTrue(coordinator.TryGetXmlElement(listview, out var listviewNode));
            if (listviewNode is IXmlElementSyntax listviewElement)
            {
                Assert.AreEqual("MyListView", listviewElement.GetAttributeValue("Name", "x"));
            }
            else
            {
                Assert.Fail("Xml Node not an Element for ListView");
            }

            Assert.AreEqual(4, coordinator.Count, "Expected 4 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }

    [TestMethod]
    public async Task ListViewComplex_XmlToXamlTest()
    {
        await EnqueueAsync(async () =>
        {
            var xaml =
                """
                <Page
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                    xmlns:converters="using:CommunityToolkit.WinUI.Converters"
                    mc:Ignorable="d"
                    d:DesignWidth="400"
                    d:DesignHeight="420">

                    <Page.Resources>
                        <converters:StringFormatConverter x:Key="StringFormatConverter"/>
                        <DataTemplate x:Key="PhotoTemplate">
                            <Grid Background="{Binding Color}"
                                  Margin="0">
                                <TextBlock Text="{Binding Word}" FontSize="{Binding Size}" Foreground="Black"/>
                            </Grid>
                        </DataTemplate>
                        <Style TargetType="ListViewItem">
                            <!--  Change those values to change the WrapPanel's children alignment  -->
                            <Setter Property="VerticalContentAlignment" Value="Center" />
                            <Setter Property="HorizontalContentAlignment" Value="Center" />
                            <Setter Property="Margin" Value="0" />
                            <Setter Property="Padding" Value="0" />
                            <Setter Property="MinWidth" Value="0" />
                            <Setter Property="MinHeight" Value="0" />
                        </Style>
                    </Page.Resources>

                    <ListView x:Name="MyListView"
                              ItemTemplate="{StaticResource PhotoTemplate}"
                              ItemsSource="{Binding WrapPanelCollection, Mode=OneWay}">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <StackPanel x:Name="sampleStackPanel"
                                            Padding="12"
                                            Spacing="4" />
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                    </ListView>
                </Page>
                """;

            var fwe = XamlReader.Load(xaml) as FrameworkElement;
            var xml = Parser.ParseText(xaml);

            await LoadTestContentAsync(fwe);

            XamlXmlTreeCoordinator coordinator = new();
            coordinator.Initialize(xml, fwe);

            var listview = fwe.FindChild("MyListView");
            Assert.IsTrue(coordinator.TryGetXmlElement(listview, out var listviewNode));
            if (listviewNode is IXmlElementSyntax listviewElement)
            {
                Assert.AreEqual("MyListView", listviewElement.GetAttributeValue("Name", "x"));
            }
            else
            {
                Assert.Fail("Xml Node not an Element for ListView");
            }

            Assert.AreEqual(2, coordinator.Count, "Expected 2 elements to be mapped.");

            await UnloadTestContentAsync(fwe);
        });
    }
}