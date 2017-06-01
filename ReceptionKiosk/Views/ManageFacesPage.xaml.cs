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
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }
    }
}
