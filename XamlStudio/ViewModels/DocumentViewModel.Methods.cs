using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AppCenter.Analytics;
using Monaco;
using Monaco.Editor;
using Monaco.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.Web.Http;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

using Range = Monaco.Range;

namespace XamlStudio.ViewModels;

public partial class DocumentViewModel
{
    private readonly AsyncLock _renderMutex = new AsyncLock();

    // TODO: Need to offset line with location in document?
    /*private async void SelectiveRenderXaml(string content)
    {
        HasCompiled = false;

        await InternalRenderXamlAsync(content, 0, true);
    }*/

    // TODO: We should just move this to the Document.xaml.cs file???
    internal async Task<XamlRenderResultContext> InternalRenderXamlAsync(string content, uint lineoffset, bool keepContentSameLength, bool overrideBinding = false)
    {
        if (SettingsService.Instance.IsLiveDataContextRefreshedOnRender == true)
        {
            // Update from our live Data Source if we've enabled it.
            await RefreshLiveDataContext();
        }

        // Update Data Context with active content (mostly for reloading from resume)
        ParseDataContext();

        var start = DateTime.UtcNow.Ticks;
        var render_analytics = new Dictionary<string, string>();

        LineDecorations.Clear(); // Clear out old errors
        _bindingHistory.Clear();

        render_analytics.Add("HasBindingDebugging", SettingsService.Instance.IsPowerBindingDebuggingEnabled.ToString());
        render_analytics.Add("HasBindingOverride", overrideBinding.ToString());
        render_analytics.Add("ContentLength", content.Length.ToString());

        var settings = new XamlRenderSettings(SettingsService.Instance.KnownNamespaces)
        {
            IsBindingDebuggingEnabled = overrideBinding ? false : SettingsService.Instance.IsPowerBindingDebuggingEnabled.Value,
            KeepSuggestedContentSameLength = keepContentSameLength,
            DataContext = DataContext, // TODO: Resolve conflict here and between design-data file
            ResourceRoot = Document.ParentFolder ?? MainViewModel.WorkspaceFolders.FirstOrDefault()?.BackingFolder,
        };

        // If we override the binding, we're calling it from within and already saved.
        if (!overrideBinding)
        {
            // Only render one at a time to not stomp on files.
            using (await _renderMutex.LockAsync())
            {
                // Save out workbench in case of error.  Should this just be done in unhandled exception case?
                await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync(Document.Id);

                // Log XAML before rendering in case issue, we can retrieve later for bugs
                try
                {
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("lastcompiled.xaml", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, content);
                    Debug.WriteLine("Render File Out: " + file.Path);
                }
                catch (Exception e)
                {
                    Debugger.Break();

                    // This fails during debug occassionally, track to see if problem in release...
                    Analytics.TrackEvent("Render_Backup_Fail", new Dictionary<string, string>()
                    {
                        { "Message", e.Message }
                    });
                }
            }
        }

        // Store in temp to prevent double-display of errors due to issue below needing double-render...
        var testResult = await XamlRenderer.RenderAsync(content, settings);

        if (testResult.Document != null)
        {
            render_analytics.Add("NumDocumentNodes", testResult.Document.Descendants().Count().ToString());
        }
        render_analytics.Add("ElementType", "" + testResult?.ElementType);
        render_analytics.Add("KnownNamespaces", "" + testResult?.DetectedNamespaces?.Length);
        render_analytics.Add("NumErrors", "" + testResult?.Errors?.Count);
        render_analytics.Add("HasContentSuggestions", (testResult.Content.Length != testResult.SuggestedContent.Length).ToString());

        if (testResult.Element == null)
        {
            // TODO: Need to investigate why we get strange XamlBindingWrapperConverter ctor error with other errors...
            if (settings.IsBindingDebuggingEnabled)
            {
                // For now, if we encounter an issue while parsing with our power binding, turn it off temporarily to try again.
                return await InternalRenderXamlAsync(content, lineoffset, keepContentSameLength, true);
            }

            // Highlight Errors
            var summary = new List<string>();
            foreach (var error in testResult.Errors)
            {
                LineDecorations.Add(new IModelDeltaDecoration(new Range(lineoffset + error.StartLine, error.StartColumn, lineoffset + error.EndLine, error.EndColumn),
                    new IModelDecorationOptions()
                    {
                        IsWholeLine = error.IsWholeLine,
                        ClassName = _errorLineStyle, // For Whole Line only
                        InlineClassName = _errorStyle,
                        HoverMessage = new string[]
                        {
                            error.Message
                        }.ToMarkdownString()
                    }));
                summary.Add(error.Message);
            }

            render_analytics.Add("ErrorMessages", string.Join(" | ", summary));

            Result = testResult;
        }
        else
        {
            Result = testResult;

            CreateBindingDecorations();
        }

        Compiled?.Invoke(this, Result);

        render_analytics.Add("TotalRenderTimeSec", Math.Round((DateTime.UtcNow.Ticks - start) / 10000000d, 2).ToString());
        Analytics.TrackEvent("Render_XAML", render_analytics);

        return Result;
    }

    private void BindingUpdated(XamlBindingInfo binding, ConversionRecord record, object newvalue)
    {
        _bindingHistory.Add(record);

        this.CreateBindingDecorations();
    }

    private void CreateBindingDecorations()
    {
        LineDecorations.Clear();

        // Highlight Bindings
        foreach (var binding in Result.Bindings)
        {
            // TODO: Does monaco-editor support inplace updating?  If so, should look into plumbing that
            switch (binding.LastKnownBindingState)
            {
                case XamlBindingInfo.XamlBindingState.NotBound:
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            InlineClassName = _bindingStyleUnbound,
                            HoverMessage = new string[]
                                {
                                    "Binding not Triggered Yet."
                                }.ToMarkdownString()
                        }));
                    break;
                case XamlBindingInfo.XamlBindingState.Successful:
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            InlineClassName = _bindingStyleSuccess,
                            HoverMessage = new string[]
                                {
                                    "Last Binding Value: " + binding.LastConvertedResultOrValue?.ToString(),
                                    "Total Hit Count: " + binding.BindingCount
                                }.ToMarkdownString()
                        }));
                    break;
                case XamlBindingInfo.XamlBindingState.ConversionError:
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(binding.Line, binding.Column, binding.Line, (uint)(binding.Column + binding.Length)),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            InlineClassName = _bindingStyleError,
                            HoverMessage = new string[]
                                {
                                    binding.LastExceptionMessage
                                }.ToMarkdownString()
                        }));
                    break;
            }

            // Register for changes
            binding.BindingUpdated -= BindingUpdated;
            binding.BindingUpdated += BindingUpdated;
        }
    }

    [RelayCommand]
    private async Task RefreshLiveDataContext()
    {
        if (Document.DataContext.IsRemote)
        {
            try
            {
                LiveDataContextRefreshError = null;

                var uri = new Uri(Document.DataContext.Uri);

                var http = new HttpClient();

                var response = await http.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();

                // Update DataContext
                ////MainViewModel.ActiveDocumentViewModel.DataContext = JsonConvert.DeserializeObject<ExpandoObject>(body);

                Document.DataContext.Content = body;

                Analytics.TrackEvent("DataSources_LoadRemote", new Dictionary<string, string>()
                {
                    { "Success", "True" }
                });
            }
            catch (Exception e2)
            {
                var msg = e2.Message;
                msg = msg.Replace("The text associated with this error code could not be found.", "").Trim();
                LiveDataContextRefreshError = msg;

                Analytics.TrackEvent("DataSources_LoadRemote", new Dictionary<string, string>()
                {
                    { "Success", "False" }
                });
            }
        }
    }

    [RelayCommand]
    private void ParseDataContext()
    {
        // TODO: Consolidate with eventual XamlRender parsing method.
        // TODO: Consolidate with Document and it's auto-compile timer logic, go back to KeyDown?
        // TODO: Is there a better way to consolidate this logic?  Maybe array and loop?
        object result = null;
        try
        {
            result = JsonConvert.DeserializeObject<ExpandoObject>(Document.DataContext.Content);
        }
        catch (Exception)
        {
        }

        if (result == null)
        {
            try
            {
                result = JsonConvert.DeserializeObject<List<ExpandoObject>>(Document.DataContext.Content);
            }
            catch (Exception)
            {
            }
        }

        if (result == null)
        {
            try
            {
                result = JsonConvert.DeserializeObject<List<object>>(Document.DataContext.Content);
            }
            catch (Exception)
            {
            }
        }

        if (result == null)
        {
            try
            {
                result = JsonConvert.DeserializeObject<object>(Document.DataContext.Content);
            }
            catch (Exception)
            {
            }
        }

        if (result != null)
        {
            // Update DataContext if we have something.
            DataContext = result;
        }
    }

    [RelayCommand]
    private void ForceRefresh()
    {
        // Force a hard-refresh of our XAML.
        HasCompiled = false;
        WeakReferenceMessenger.Default.Send<RenderXamlMessage>();
    }

    [RelayCommand]
    private void RotatePaneOrientation()
    {
        var orientation = Document.State.PreviewOrientation;

        // If we're set to default, go grab that value to start from
        if (orientation == null)
        {
            orientation = Settings.DefaultPreviewPanePosition;
        }

        // Go to next enum value
        orientation = (PaneOrientation)((((int)orientation.Value) + 1) % 4);

        Analytics.TrackEvent("Document_RotateOrientation", new Dictionary<string, string>()
            {
                { "Value", "" + orientation },
            });

        // If we're now set to default, set to null so we stay aligned if default changes
        if (orientation == Settings.DefaultPreviewPanePosition)
        {
            orientation = null;
        }

        Document.State.PreviewOrientation = orientation;
    }

    [RelayCommand]
    private void TogglePreviewTheme()
    {
        // If we're set to default, use whatever we're currently displaying
        ElementTheme? theme = Document.State.PreviewAreaTheme ?? ActualTheme;

        // Go to next value
        theme = theme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;

        Analytics.TrackEvent("Document_TogglePreviewAreaTheme", new Dictionary<string, string>()
            {
                { "Value", "" + theme },
            });

        // If we're now set to default, set to null so we stay aligned if default changes
        if (theme == ThemeSelectorService.Theme)
        {
            theme = null;
        }

        Document.State.PreviewAreaTheme = theme;
    }
}
