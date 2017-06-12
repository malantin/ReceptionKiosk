using ReceptionKiosk.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ReceptionKiosk.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {            
            InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
            Application.Current.EnteredBackground += Current_EnteredBackground;
            ViewModel = new MainViewModel(cameraControl);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.stopLoop();
            await cameraControl.CleanupCameraAsync();
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(AddFacePage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await cameraControl.CleanupCameraAsync();
                deferral.Complete();
            }
        }


        private void Current_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            cameraControl.Reset();
        }
    }
}
