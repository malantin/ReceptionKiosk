using ReceptionKiosk.ViewModels;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;

namespace ReceptionKiosk.Views
{
    public sealed partial class AddFacePage : Page
    {        
        public AddFaceViewModel ViewModel { get; } = new AddFaceViewModel();

        public AddFacePage()
        {
            InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Initialize();

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
