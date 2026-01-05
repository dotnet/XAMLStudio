// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using XamlStudio.Activation;
using XamlStudio.Helpers;

namespace XamlStudio.Services
{
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        private readonly AsyncLock _suspendMutex = new AsyncLock();

        //// TODO WTS: For more information regarding the application lifecycle and how to handle suspend and resume, please see:
        //// Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle

        private const string StateFilename = "SuspendAndResumeState";

        // TODO WTS: This event is fired just before the app enters in background. Subscribe to this event if you want to save your current state.
        public event EventHandler<OnBackgroundEnteringEventArgs> OnBackgroundEntering;

        public async Task SaveStateAsync(string renderId = null)
        {
            using (await _suspendMutex.LockAsync())
            {
                var suspensionState = new SuspensionState()
                {
                    FromRender = renderId != null,
                    LastRenderedId = renderId,
                    SuspensionDate = DateTime.Now
                };

                var target = OnBackgroundEntering?.Target.GetType();
                var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target, suspensionState.FromRender);

                OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

                await ApplicationData.Current.LocalFolder.SaveAsync(StateFilename, onBackgroundEnteringArgs);
            }
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await RestoreStateAsync();
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return true;
        }

        private async Task RestoreStateAsync()
        {
            using (await _suspendMutex.LockAsync())
            {
                var saveState = await ApplicationData.Current.LocalFolder.ReadAsync<OnBackgroundEnteringEventArgs>(StateFilename);
                if (saveState?.Target != null && typeof(Page).IsAssignableFrom(saveState.Target))
                {
                    NavigationService.Navigate(saveState.Target, saveState.SuspensionState);
                }
            }
        }
    }
}
