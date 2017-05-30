using ReceptionKiosk.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();
        public MainPage()
        {
            InitializeComponent();
        }
    }
}
