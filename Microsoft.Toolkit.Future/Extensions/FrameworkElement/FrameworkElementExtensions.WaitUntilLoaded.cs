// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CommunityToolkit.WinUI.Extensions.Future;

public static partial class FrameworkElementExtensions
{
    public static Task<bool> WaitUntilLoadedAsync(this FrameworkElement element, TaskCreationOptions? options = null)
    {
        if (element.IsLoaded) // && element.Parent != null) // TODO: Seeing a case where IsLoaded is true, but Parent is null still... gumming up the works, as Loaded I don't think is called again after in that case.
        {
            return Task.FromResult(true);
        }

        var taskCompletionSource = options.HasValue ? new TaskCompletionSource<bool>(options.Value)
                : new TaskCompletionSource<bool>();
        try
        {
            void LoadedCallback(object sender, RoutedEventArgs args)
            {
                element.Loaded -= LoadedCallback;
                taskCompletionSource.SetResult(true);
            }

            element.Loaded += LoadedCallback;
        }
        catch (Exception e)
        {
            taskCompletionSource.SetException(e);
        }

        return taskCompletionSource.Task;
    }
}