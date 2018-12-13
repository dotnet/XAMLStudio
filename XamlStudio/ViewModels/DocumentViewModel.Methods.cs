using Monaco;
using Monaco.Editor;
using Monaco.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml;
using XamlStudio.Helpers;
using XamlStudio.Services;
using XamlStudio.Toolkit.Controls;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.ViewModels
{
    public partial class DocumentViewModel
    {
        // TODO: Need to align these two methods for rendering.
        private async void UpdateXaml(RoutedEventArgs args)
        {
            // Check if nothing to do
            if (Document.Content == null || Document.Content.Length == 0 || HasCompiled)
            {
                return;
            }

            var keepcontent = !SettingsService.Instance.IsContentUpdatedWithSuggested.Value;

            var newcontent = await InternalRenderXamlAsync(Document.Content, 0, keepcontent);

            if (!keepcontent)
            {
                // Update our document with suggested changes.
                Document.Content = newcontent;

                // BUGBUG: TODO: Need to restore cursor location!
            }

            HasCompiled = true;
        }

        private async void SelectiveRenderXaml(string content)
        {
            HasCompiled = false;

            // TODO: Need to offset line with location in document once we update monaco-uwp 0.7...
            await InternalRenderXamlAsync(content, 0, true);
        }

        private async Task<string> InternalRenderXamlAsync(string content, uint lineoffset, bool keepContentSameLength, bool overrideBinding = false)
        {
            LineDecorations.Clear(); // Clear out old errors
            _bindingHistory.Clear();

            var settings = new XamlRenderSettings(SettingsService.Instance.KnownNamespaces)
            {
                IsBindingDebuggingEnabled = overrideBinding ? false : SettingsService.Instance.IsPowerBindingDebuggingEnabled.Value,
                KeepSuggestedContentSameLength = keepContentSameLength,
                DataContext = DataContext
            };

            // Log XAML before rendering in case issue, we can retrieve later for bugs
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("lastcompiled.xaml", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, content);
            Debug.WriteLine("Render File Out: " + file.Path);

            // Store in temp to prevent double-display of errors due to issue below needing double-render...
            var testResult = await XamlRenderer.RenderAsync(content, settings);

            if (testResult.Element == null)
            {
                // TODO: Need to investigate why we get strange XamlBindingWrapperConverter ctor error with other errors...
                if (settings.IsBindingDebuggingEnabled)
                {
                    // For now, if we encounter an issue while parsing with our power binding, turn it off temporarily to try again.
                    return await InternalRenderXamlAsync(content, lineoffset, keepContentSameLength, true);
                }

                // Highlight Errors
                foreach (var error in testResult.Errors)
                {
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(lineoffset + error.StartLine, error.StartColumn, lineoffset + error.EndLine, error.EndColumn),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = error.IsWholeLine,
                            ClassName = this._errorStyle,
                            HoverMessage = new string[]
                            {
                                error.Message
                            }.ToMarkdownString()
                        }));
                }

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
                element = new ResourceViewer() { ResourceDictionary = element as ResourceDictionary };
            }

            // Only Update if we have a new well-parsed element.
            if (element != null && element is UIElement)
            {
                // Add element to main panel
                XamlRoot.Children.Clear();
                XamlRoot.Children.Add(element as UIElement);
            }

            Compiled?.Invoke(this, new EventArgs());

            return Result.SuggestedContent;
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
                                ClassName = this._bindingStyleUnbound,
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
                                ClassName = this._bindingStyleSuccess,
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
                                ClassName = this._bindingStyleError,
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
            // Handle Shortcuts.
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
                    UpdateXaml(null);
                ////}

                // Eat key stroke
                args.Handled = true;
            } else if (args.CtrlKey)
            {
                // Need to duplicate this here from ShellViewModel as Control eats CoreWindow event.
                /*switch (args.KeyCode)
                {
                    case 78: // N
                        (Application.Current as App).ViewModel.NewDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 79: // O
                        (Application.Current as App).ViewModel.OpenDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 83: // S
                        (Application.Current as App).ViewModel.SaveDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 87: // W
                    case 115: // F4
                        (Application.Current as App).ViewModel.CloseDocumentCommand.Execute(null);
                        args.Handled = true;
                        break;
                    case 9: // TAB
                        if (args.ShiftKey)
                        {
                            (Application.Current as App).ViewModel.PreviousDocumentCommand.Execute(null);
                        }
                        else
                        {
                            (Application.Current as App).ViewModel.NextDocumentCommand.Execute(null);
                        }
                        args.Handled = true;
                        break;
                }*/
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
                    this._autocompileTimer = ThreadPoolTimer.CreateTimer(async (e) =>
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            UpdateXaml(null);
                        });
                    }, TimeSpan.FromSeconds(SettingsService.Instance.AutoCompileDelay.Value));
                }
            }
        }
    }
}
