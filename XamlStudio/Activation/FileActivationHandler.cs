// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;

using XamlStudio.Services;
using XamlStudio.Views;

namespace XamlStudio.Activation
{
    // TODO WTS: Open package.appxmanifest and change the declaration for the scheme (from the default of 'wtsapp') to what you want for your app.
    // More details about this functionality can be found at https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/features/uri-scheme.md
    // TODO WTS: Change the image in Assets/Logo.png to one for display if the OS asks the user which app to launch.
    internal class FileActivationHandler : ActivationHandler<FileActivatedEventArgs>
    {
        // By default, this handler expects URIs of the format 'wtsapp:sample?secret={value}'
        protected override async Task HandleInternalAsync(FileActivatedEventArgs args)
        {
            if (args.Files != null && args.Files.Count > 0)
            {
                // App already Initialized
                if (NavigationService.Frame.Content is IFileOpener opener)
                {
                    opener.OpenFileItems(args.Files.ToArray());
                }
                else
                {
                    // On App Launch, Navigate to main page with arguments.
                    NavigationService.Navigate(typeof(MainPage), args.Files.ToArray());
                }
            }

            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(FileActivatedEventArgs args)
        {
            // If your app has multiple handlers of FileActivationEventArgs
            // use this method to determine which to use. (possibly checking args.Files)
            return true;
        }
    }
}
