using Monaco;
using Monaco.Editor;
using Monaco.Helpers;
using System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using XamlStudio.Services;
using XamlStudio.Toolkit.Models;
using XamlStudio.Toolkit.Services;

namespace XamlStudio.ViewModels
{
    public partial class DocumentViewModel
    {
        private async void UpdateXaml(RoutedEventArgs args)
        {
            // Check if nothing to do
            if (Document.Content == null || Document.Content.Length == 0 || HasCompiled)
            {
                return;
            }

            LineDecorations.Clear(); // Clear out old errors
            _bindingHistory.Clear();

            var settings = new XamlRenderSettings(SettingsService.Instance.KnownNamespaces)
            {
                IsBindingDebuggingEnabled = SettingsService.Instance.IsPowerBindingDebuggingEnabled.Value,
                KeepSuggestedContentSameLength = !SettingsService.Instance.IsContentUpdatedWithSuggested.Value,
                DataContext = DataContext
            };

            // Pre-parse            
            var content = Document.Content;

            Result = await XamlRenderer.RenderAsync(content, settings);

            if (!settings.KeepSuggestedContentSameLength)
            {
                // Update our document with suggested changes.
                Document.Content = Result.SuggestedContent;
            }

            if (Result.Element == null)
            {
                // Highlight Errors
                foreach (var error in Result.Errors)
                {
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(error.StartLine, error.StartColumn, error.EndLine, error.EndColumn),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            ClassName = this._errorStyle,
                            HoverMessage = new string[]
                            {
                                error.Message
                            }
                        }));
                }
            }
            else
            {
                CreateBindingDecorations();
            }

            // Only Update if we have a new well-parsed element.
            if (Result.Element != null)
            {
                // Add element to main panel
                XamlRoot.Children.Clear();
                XamlRoot.Children.Add(Result.Element);
            }

            HasCompiled = true;
            Compiled?.Invoke(this, new EventArgs());
        }

        private async void SelectiveRenderXaml(string content)
        {
            HasCompiled = false;

            // TODO: reuse code above better
            LineDecorations.Clear(); // Clear out old errors
            _bindingHistory.Clear();

            var settings = new XamlRenderSettings(SettingsService.Instance.KnownNamespaces)
            {
                IsBindingDebuggingEnabled = SettingsService.Instance.IsPowerBindingDebuggingEnabled.Value,
                KeepSuggestedContentSameLength = true,
                DataContext = DataContext
            };

            Result = await XamlRenderer.RenderAsync(content, settings);

            if (Result.Element == null)
            {
                // TODO: Need to offset with location in document...

                // Highlight Errors
                foreach (var error in Result.Errors)
                {
                    LineDecorations.Add(new IModelDeltaDecoration(new Range(error.StartLine, error.StartColumn, error.EndLine, error.EndColumn),
                        new IModelDecorationOptions()
                        {
                            IsWholeLine = false,
                            ClassName = this._errorStyle,
                            HoverMessage = new string[]
                            {
                                error.Message
                            }
                        }));
                }
            }
            else
            {
                CreateBindingDecorations();
            }

            // Only Update if we have a new well-parsed element.
            if (Result.Element != null)
            {
                // Add element to main panel
                XamlRoot.Children.Clear();
                XamlRoot.Children.Add(Result.Element);
            }

            Compiled?.Invoke(this, new EventArgs());
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
                                    }
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
                                    }
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
                                    }
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
                if (args.KeyCode == 116 &&
                    SettingsService.Instance.IsCompileSelectionEnabled.Value == true &&
                    !string.IsNullOrWhiteSpace(SelectedText))
                {
                    SelectiveRenderXaml(SelectedText);
                }
                else
                {
                    UpdateXaml(null);
                }

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
