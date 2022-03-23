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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Web.Http;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Controls;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.ViewModels
{
    public partial class DocumentViewModel
    {
        private readonly AsyncLock _renderMutex = new AsyncLock();

        private async void SelectiveRenderXaml(string content)
        {
            HasCompiled = false;

            // TODO: Need to offset line with location in document once we update monaco-uwp 0.7...
            await InternalRenderXamlAsync(content, 0, true);
        }

        internal async Task<XamlRenderResultContext> InternalRenderXamlAsync(string content, uint lineoffset, bool keepContentSameLength, bool overrideBinding = false)
        {
            if (SettingsService.Instance.IsLiveDataContextRefreshedOnRender == true)
            {
                // Update from our live Data Source if we've enabled it.
                await RefreshLiveDataContext(null);
            }

            // Update Data Context with active content (mostly for reloading from resume)
            ParseDataContext(null);

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
                ResourceRoot = Document.ParentFolder,
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

            var element = Result.Element;
            if (Result.IsResourceDictionary)
            {
                element = new ResourceViewer()
                {
                    ResourceDictionary = element as ResourceDictionary,
                    XmlDocument = Result.Document
                };
            }

            // Only Update if we have a new well-parsed element.
            if (element != null && element is UIElement)
            {
                // Add element to main panel
                XamlRoot.Children.Clear();
                XamlRoot.Children.Add(element as UIElement);
            }

            Compiled?.Invoke(this, new EventArgs());

            render_analytics.Add("TotalRenderTimeSec", Math.Round((DateTime.UtcNow.Ticks - start) / 10000000d, 2).ToString());
            Analytics.TrackEvent("Render_XAML", render_analytics);

            return Result;
        }

        private void BindingUpdated(XamlBindingInfo binding, ConversionRecord record, object newvalue)
        {
            _bindingHistory.Add(record);

            LineDecorations.Clear();
            this.CreateBindingDecorations();
            Compiled?.Invoke(this, new EventArgs());
        }

        private void CreateBindingDecorations()
        {
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

        private async Task RefreshLiveDataContext(RoutedEventArgs args)
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

        private void ParseDataContext(RoutedEventArgs args)
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

        private static readonly int[] NonCharacterCodes = new int[] {
            // Modifier Keys
            16, 17, 18, 20, 91,
            // Esc / Page Keys / Home / End / Insert
            27, 33, 34, 35, 36, 45,
            // Arrow Keys
            37, 38, 39, 40,
            // Function Keys
            112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123
        };

        private void KeyDown(WebKeyEventArgs args)
        {
            // Handle Shortcuts. https://keycode.info/
            // Ctrl+Enter or F5 Update // TODO: Do we need this in the app handler too? (Thinking no)
            if ((args.KeyCode == 13 && args.CtrlKey) ||
                 args.KeyCode == 116)
            {
                ////if (args.KeyCode == 116 &&
                ////    SettingsService.Instance.IsCompileSelectionEnabled.Value == true &&
                ////    !string.IsNullOrWhiteSpace(SelectedText))
                ////{
                ////    SelectiveRenderXaml(SelectedText);
                ////}
                ////else
                ////{
                    UpdateXamlCommand?.Execute(null);
                ////}

                // Eat key stroke
                args.Handled = true;
            }
            else if (args.CtrlKey)
            {
                if (args.ShiftKey)
                {
                    switch (args.KeyCode)
                    {
                        // E - Open Explorer
                        case 69:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("EXPLORER");
                            break;
                        // C - Open Data Context
                        case 67:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("DATASOURCES");
                            break;
                        // B - Open Binding Debugger
                        case 66:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("DEBUG");
                            break;
                        // T - Open Toolbox
                        case 84:
                            args.Handled = true;
                            MainViewModel.OpenActivityCommand.Execute("TOOLBOX");
                            break;
                    }
                }
                // Need to duplicate this here from ShellViewModel as Control eats CoreWindow event.
                switch (args.KeyCode)
                {
                    case 73: // I
                        MainViewModel.OpenSettingsCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 78: // N
                        MainViewModel.NewDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 79: // O
                        MainViewModel.OpenDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 83: // S
                        if (args.ShiftKey)
                        {
                            MainViewModel.SaveDocumentAsCommand.Execute(MainViewModel.ActiveFile);
                        }
                        else
                        {
                            MainViewModel.SaveDocumentCommand.Execute(MainViewModel.ActiveFile);
                        }
                        args.Handled = true;
                        break;
                    case 87: // W
                    case 115: // F4
                        MainViewModel.CloseActiveDocumentCommand.Execute(MainViewModel.ActiveFile);
                        args.Handled = true;
                        break;
                    case 9: // TAB
                        if (args.ShiftKey)
                        {
                            MainViewModel.PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            MainViewModel.NextDocumentCommand.Execute(null);
                        }
                        args.Handled = true;
                        break;
                }
            }

            if (args.Handled)
            {
                Analytics.TrackEvent("Key_Shortcut", new Dictionary<string, string>()
                {
                    { "Location", "Document" },
                    { "Action", args.Handled.ToString() },
                    { "Ctrl", args.CtrlKey.ToString() },
                    { "Shift", args.ShiftKey.ToString() },
                    { "Code", args.KeyCode.ToString() }
                });
            }

            // Ignore as a change to the document if we handle it as a shortcut above or it's a special char.
            if (!args.Handled && Array.IndexOf(NonCharacterCodes, args.KeyCode) == -1)
            {
                // TODO: Filter out non-display characters or look for text change...
                this.Document.HasChanged = true; // Mark Dirty
                this.HasCompiled = false;

                // Setup Time for Auto-Compile
                if (SettingsService.Instance.IsAutoCompileEnabled.Value)
                {
                    this._autocompileTimer?.Cancel(); // Stop Old Timer
                                                      // Create Compile Timer
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    this._autocompileTimer = ThreadPoolTimer.CreateTimer((e) =>
                    {
                        dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                        {
                            UpdateXamlCommand?.Execute(null);
                        });
                    }, TimeSpan.FromSeconds(SettingsService.Instance.AutoCompileDelay.Value));
                }
            }
        }

        private void RotatePaneOrientation(RoutedEventArgs args)
        {
            var orientation = _document.State.PreviewOrientation;

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

            _document.State.PreviewOrientation = orientation;
        }

        private void TogglePreviewTheme(RoutedEventArgs args)
        {
            // If we're set to default, use whatever we're currently displaying
            ElementTheme? theme = _document.State.PreviewAreaTheme ?? ActualTheme;

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

            _document.State.PreviewAreaTheme = theme;
        }
    }
}
