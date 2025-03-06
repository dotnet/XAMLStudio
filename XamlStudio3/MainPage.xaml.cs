using System.Reflection;

namespace XamlStudio3;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        XamlMarkup.Text = await ReadTemplateTextAsync(@"Templates\Default\BlankPage.txaml");
        CSharpCode.Text = await ReadTemplateTextAsync(@"Templates\Default\ViewModel.cs");
        XamlResourceMarkup.Text = await ReadTemplateTextAsync(@"Templates\Default\ResourceDictionary.xaml");
    }

    private async Task<string> ReadTemplateTextAsync(string relativeFilePath)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Path.Combine("ms-appx:///", relativeFilePath)));
        return await FileIO.ReadTextAsync(file);
    }
}
