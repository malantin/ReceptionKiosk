using ReceptionKiosk.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ReceptionKiosk.Views
{
    public sealed partial class ManageFacesPage : Page
    {
        public ManageFacesViewModel ViewModel { get; } = new ManageFacesViewModel();

        public ManageFacesPage()
        {
            InitializeComponent();

            //Fixed: Textbox can't focus
            //Further Information: https://stackoverflow.com/questions/39096758/cant-enter-enter-text-in-textbox-control-inside-flyout
            if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "AllowFocusOnInteraction"))
            {
                openAddFlyoutButton.AllowFocusOnInteraction = true;
                openDeleteFlyoutButton.AllowFocusOnInteraction = true;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }
    }
}
