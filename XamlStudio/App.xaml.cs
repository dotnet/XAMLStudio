using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

using XamlStudio.Services;

namespace XamlStudio
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            EnteredBackground += App_EnteredBackground;

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);

            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Task t = new Task(async () =>
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("unhandlederror.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, e.Message + Environment.NewLine + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
                Debug.WriteLine("Render Error Out: " + file.Path);
            });

            t.Start();
            t.Wait(3000);

            // TODO: Check if these are coming from a Render call and just ignore then.
            Debugger.Break();
            e.Handled = true;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            CustomizeTitleBar();

            void CustomizeTitleBar()
            {
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = (Color)App.Current.Resources["Color-Grey-Light-1"];
                //titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = titleBar.ButtonInactiveForegroundColor = Colors.White;

                //var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                //coreTitleBar.ExtendViewIntoTitleBar = true;
            }

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.MainPage));
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Windows.Foundation.Deferral deferral = e.GetDeferral();
            await Helpers.Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }
    }
}
