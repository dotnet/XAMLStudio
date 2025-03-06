using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.SourceGenerators;
using CommunityToolkit.WinUI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Markup;

namespace XamlStudio3;

public sealed partial class MainPage : Page
{
    // Cache of all our compilation inputs
    private static readonly string RuntimePath = RuntimeEnvironment.GetRuntimeDirectory();
    private static readonly ImmutableArray<MetadataReference> AssemblyReferences = CreateReferences();
    private static readonly IIncrementalGenerator[] Generators = [
        new ObservablePropertyGenerator(),
        new RelayCommandGenerator()
    ];
    private static readonly CSharpCompilationOptions CompilationOptions = new(
                    outputKind: OutputKind.DynamicallyLinkedLibrary,
                    nullableContextOptions: NullableContextOptions.Enable);

    // Debounce Timers and Interval
    private readonly DispatcherQueueTimer _debounceTimerXaml = DispatcherQueue.GetForCurrentThread().CreateTimer();
    private readonly DispatcherQueueTimer _debounceTimerCode = DispatcherQueue.GetForCurrentThread().CreateTimer();
    private readonly DispatcherQueueTimer _debounceTimerResources = DispatcherQueue.GetForCurrentThread().CreateTimer();
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(150);

    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();

    // Private cached intermediary results
    private object _viewModel;

    private ResourceDictionary _resources;

    public MainPage()
    {
        this.InitializeComponent();

        // TODO: Should spin this off as a task so UI can load and show progress/spinner
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        XamlMarkup.Text = await ReadTemplateTextAsync(@"Templates\Default\BlankPage.txaml");
        CSharpCode.Text = await ReadTemplateTextAsync(@"Templates\Default\ViewModel.cs");
        XamlResourceMarkup.Text = await ReadTemplateTextAsync(@"Templates\Default\ResourceDictionary.xaml");

        // We want to do initial pass after loading all text in the proper/efficient order.
        CompileResources();
        CompileCode();
        CompileXaml(); // We do this after as it uses the other results

        // Then listen to new/incremental changes.
        // TODO: Maybe have an IsLoading guard for when we load other samples?
        XamlMarkup.TextChanged += XamlMarkup_TextChanged;
        CSharpCode.TextChanged += CSharpCode_TextChanged;
        XamlResourceMarkup.TextChanged += XamlResourceMarkup_TextChanged;
    }

    private async Task<string> ReadTemplateTextAsync(string relativeFilePath)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Path.Combine("ms-appx:///", relativeFilePath)));
        return await FileIO.ReadTextAsync(file);
    }

    private void XamlMarkup_TextChanged(object sender, TextChangedEventArgs e)
    {
        _debounceTimerXaml.Debounce(() =>
        {
            CompileXaml();
        }, interval: _debounceInterval);
    }

    private void CSharpCode_TextChanged(object sender, TextChangedEventArgs e)
    {
        _debounceTimerCode.Debounce(() =>
        {
            CompileCode();
        }, interval: _debounceInterval);
    }

    private void XamlResourceMarkup_TextChanged(object sender, TextChangedEventArgs e)
    {
        _debounceTimerResources.Debounce(() =>
        {
            CompileResources();
        }, interval: _debounceInterval);
    }

    /// <summary>
    /// Reads the XamlMarkup.Text and modifies the ResultRoot.Child with the result,
    /// uses the last compiled _resources and _viewModel.
    /// </summary>
    private void CompileXaml()
    {
        try
        {
            // Depends on CompileResources and CompileCode to be most efficient
            if (XamlReader.Load(XamlMarkup.Text) is FrameworkElement fe)
            {
                // Unload old element and its resources
                if (ResultRoot.Child is FrameworkElement oldFe)
                {
                    oldFe.Resources.MergedDictionaries.Clear();
                    oldFe.Resources.Clear();
                    oldFe.Resources = null;
#if WINDOWS
                    VisualTreeHelper.DisconnectChildrenRecursive(oldFe);
#endif
                    ResultRoot.Child = null;
                }                

                // Re-hook cached drivers
                if (_viewModel != null)
                {
                    fe.DataContext = _viewModel;
                }

                if (_resources != null)
                {
                    // Add resources to the merged dictionary of the new element
                    // It's important we do this BEFORE the element is added to the visual tree
                    fe.Resources.MergedDictionaries.Add(_resources);
                }

                // Add to visual tree
                ResultRoot.Child = fe;
            }
        }
        catch (Exception exception)
        {
            // TODO: Report Errors
        }
    }

    /// <summary>
    /// Compiles the resources and injects the result into the ResultRoot.Child's Resources.
    /// </summary>
    private void CompileResources()
    {
        try
        {
            if (XamlReader.Load(XamlResourceMarkup.Text) is ResourceDictionary rd)
            {
                if (ResultRoot.Child is FrameworkElement oldFe)
                {
                    // There may be merged dictionaries added by the XAML file, so we want to find ours and remove it only.
                    oldFe.Resources.MergedDictionaries.Remove(_resources);
                }

                _resources = rd;

                // Update UI
                if (ResultRoot.Child is FrameworkElement fe)
                {
                    fe.Resources.MergedDictionaries.Add(_resources);

                    // TODO: Remember what setting we actually want and pick a different one...
                    // Workaround, see https://github.com/microsoft/microsoft-ui-xaml/issues/5457 and https://github.com/microsoft/microsoft-ui-xaml/issues/4443
                    // We remove and re-add the framework element from the visual tree to re-apply style (toggling theme as called out in issue didn't work).
                    ResultRoot.Child = null;
                    ResultRoot.Child = fe;
                }
            }
        }
        catch (Exception exception)
        {
            // TODO: Report Errors
        }
    }

    /// <summary>
    /// Compiles the code and injects the result into the ResultRoot.Child's DataContext.
    /// </summary>
    private void CompileCode()
    {
        try
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(CSharpCode.Text,
                                                               CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "SomeAssembly",
                syntaxTrees: [ syntaxTree ],
                references: AssemblyReferences,
                options: CompilationOptions
                );

            GeneratorDriver driver = CSharpGeneratorDriver.Create(Generators)
                .WithUpdatedParseOptions((CSharpParseOptions)syntaxTree.Options);

            // Run Source Generators
            _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

            // TODO: Check diagnostic output from source generators

            // Compile to memory
            using MemoryStream stream = new();
            EmitResult emitResult = outputCompilation.Emit(stream);

            if (emitResult.Success is false)
            {
                // TODO: Errors
                //// emitResult.Diagnostics;
                return;
            }

            _ = stream.Seek(0, SeekOrigin.Begin);

            var assembly = Assembly.Load(stream.ToArray());
            if (assembly != null && assembly.CreateInstance("ViewModel") is object viewModel)
            {
                // TODO: Get/detect class name from input text
                _viewModel = viewModel;
            }
            else
            {
                // TODO: Report error
                return;
            }

            // Update UI
            if (ResultRoot.Child is FrameworkElement fe)
            {
                fe.DataContext = _viewModel;
            }
        }
        catch (Exception exception)
        {
            // TODO: Errors
        }
    }

    private static ImmutableArray<MetadataReference> CreateReferences() => new[]
        {
            // System References
            typeof(object).GetTypeInfo().Assembly.Location,
            Path.Combine(RuntimePath, "System.Collections.dll"),
            Path.Combine(RuntimePath, "System.Linq.Expressions.dll"),
            Path.Combine(RuntimePath, "System.Runtime.dll"),
            typeof(PropertyChangedEventHandler).GetTypeInfo().Assembly.Location,

            // Custom Dependencies
            typeof(ObservableObject).GetTypeInfo().Assembly.Location,
            typeof(ObservablePropertyGenerator).GetTypeInfo().Assembly.Location,
        }.Select(path => MetadataReference.CreateFromFile(path))
         .Cast<MetadataReference>()
         .ToImmutableArray();
}
