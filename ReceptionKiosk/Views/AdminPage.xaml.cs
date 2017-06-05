using ReceptionKiosk.ViewModels;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.Views
{
    public sealed partial class AdminPage : Page
    {
        public AdminViewModel ViewModel { get; } = new AdminViewModel();

        public AdminPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.InitializeAsync();
        }
    }
}
