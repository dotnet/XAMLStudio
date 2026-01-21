// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace XamlStudio.Toolkit.UnitTests;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
sealed partial class App : Application
{
    // MacOS and iOS don't know the correct type without a full namespace declaration, confusing it with NSWindow and UIWindow.
    // Using static will not work.
#if WINAPPSDK
    private static Microsoft.UI.Xaml.Window currentWindow = Microsoft.UI.Xaml.Window.Current;
#else
    private static Windows.UI.Xaml.Window currentWindow = Windows.UI.Xaml.Window.Current;
#endif

    // Holder for test content to abstract Window.Current.Content
    public static FrameworkElement? ContentRoot
    {
        get => currentWindow.Content as FrameworkElement;
        set => currentWindow.Content = value;
    }

    // Abstract CoreApplication.MainView.DispatcherQueue
    public static DispatcherQueue DispatcherQueue =>
#if !WINAPPSDK
            CoreApplication.MainView.DispatcherQueue;
#else
            currentWindow.DispatcherQueue;
#endif


    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        this.Suspending += OnSuspending;
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {

#if DEBUG
        if (System.Diagnostics.Debugger.IsAttached)
        {
            this.DebugSettings.EnableFrameRateCounter = true;
        }
#endif

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (currentWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            currentWindow.Content = rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        ////Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.CreateDefaultUI();

        // Ensure the current window is active
        currentWindow.Activate();

#if !WINAPPSDK
        Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(e.Arguments);
#else
        UITestMethodAttribute.DispatcherQueue = DispatcherQueue;

        // Replace back with e.Arguments when https://github.com/microsoft/microsoft-ui-xaml/issues/3368 is fixed
        Microsoft.VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(Environment.CommandLine);
#endif
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

    /// <summary>
    /// Invoked when application execution is being suspended.  Application state is saved
    /// without knowing whether the application will be terminated or resumed with the contents
    /// of memory still intact.
    /// </summary>
    /// <param name="sender">The source of the suspend request.</param>
    /// <param name="e">Details about the suspend request.</param>
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        //TODO: Save application state and stop any background activity
        deferral.Complete();
    }
}
