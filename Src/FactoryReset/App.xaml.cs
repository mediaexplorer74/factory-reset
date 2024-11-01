using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace GameManager
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            RequiresPointerMode = Windows.UI.Xaml.ApplicationRequiresPointerMode.WhenRequested;
            Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var landingPage = Windows.UI.Xaml.Window.Current.Content;
            if (landingPage == null)
            {
                var newPage = new UI.Root();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }
                
                Windows.UI.Xaml.Window.Current.Content = newPage;
            }

            Windows.UI.Xaml.Window.Current.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
