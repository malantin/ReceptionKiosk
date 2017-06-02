using ReceptionKiosk.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ReceptionKiosk.Views
{
    public sealed partial class AddFacePage : Page
    {        
        public AddFaceViewModel ViewModel { get; } = new AddFaceViewModel();

        public AddFacePage()
        {
            InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
            Application.Current.EnteredBackground += Current_EnteredBackground;
        }

        private void Current_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            cameraControl.Reset();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();

            if (groupBox.Items.Count > 0) groupBox.SelectedIndex = 0;
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
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
    }
}
