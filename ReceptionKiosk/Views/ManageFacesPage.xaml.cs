using ReceptionKiosk.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.Views
{
    public sealed partial class ManageFacesPage : Page
    {
        public ManageFacesViewModel ViewModel { get; } = new ManageFacesViewModel();
        public ManageFacesPage()
        {
            InitializeComponent();
        }
    }
}
