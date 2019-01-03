using Newtonsoft.Json;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using XamlStudio.Helpers;
using XamlStudio.Models;
using XamlStudio.Services;
using XamlStudio.Toolkit.Helpers;
using XamlStudio.Views;
using Windows.UI.Core;

namespace XamlStudio
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        private ExtendedExecutionSession extendedExecutionSession;

        private bool _badError = false;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            AppLoggerService.Initialize();

            InitializeComponent();

            UnhandledException += App_UnhandledException;
            EnteredBackground += App_EnteredBackground;
            Suspending += App_Suspending;
            Resuming += App_Resuming;

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);

            var culture = CultureInfoHelper.GetCurrentCulture();
            var region = new RegionInfo(culture.LCID);
            AppCenter.SetCountryCode(region.TwoLetterISORegionName);
            AppCenter.Start("", typeof(Analytics));
            AppCenter.Start("", typeof(Crashes));
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                AppLoggerService.LogInfo($"[AppActivation] Application activated by {args.Kind}");
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            AppLoggerService.LogInfo($"[AppActivation] Application activated by {args.Kind}");
            await ActivationService.ActivateAsync(args);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            AppLoggerService.LogInfo($"[AppActivation] Application activated by {args.Kind}");
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.MainPage));
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.
        /// </summary>
        private async void App_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            try
            {
                Task loggerTask = AppLoggerService.OnSuspending();

                await Task.WhenAll(loggerTask);
            }
            catch(Exception)
            {

            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Initializes the <see cref="ExtendedExecutionSession"/> instance.
        /// </summary>
        private void ClearExtendedExecution()
        {
            if(extendedExecutionSession != null)
            {
                extendedExecutionSession.Revoked -= App_ExtendedExecutionRevoked;
            }
        }

        /// <summary>
        /// Handles the event of extended execution being revoked.
        /// </summary>
        private void App_ExtendedExecutionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            AppLoggerService.LogInfo($"The request to ExtendedExecutionSession was revoked with reason {args.Reason}");
        }

        /// <summary>
        /// Invoked when application execution is being resummed.
        /// </summary>
        private void App_Resuming(object sender, object e)
        {
            AppLoggerService.OnResuming();
            AppLoggerService.LogInfo($"[AppResumming] Resuming application.");
        }

        /// <summary>
        /// Handles the event of an unhandles exception bubbling up all the way to the App instance.
        /// </summary>
        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO: Check if these are coming from a Render call and just ignore then.# (this is usually the case, but doesn't save us)
            e.Handled = true;

            try
            {
                AppLoggerService.LogCrash(e.Exception, sender);
                AppLoggerService.FlushMessages();

                // Save Exception for easier location
                Task.Run(async () => {
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("lastexception.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(new UnhandledException(e.Message, e.Exception)));
                }).Wait();  
				              
                Analytics.TrackEvent("UnhandledException", new Dictionary<string, string> {
                    { "Message", e.Message },
                    { "Exception", e.Exception.ToString() },
                    { "StackTrace", e.Exception.StackTrace },
                });
            }
            catch(Exception)
            {

            }
            finally
            {
                UnhandledException -= App_UnhandledException;
            }

#if DEBUG
            Debugger.Break();
#endif

            // Restart app for stability, except on RS5 19329065/19654150
            var t = CoreApplication.RequestRestartAsync(string.Empty);
        }
    }
}
