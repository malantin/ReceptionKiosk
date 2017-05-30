using ReceptionKiosk.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.Views
{
    public sealed partial class AddFacePage : Page
    {
        public AddFaceViewModel ViewModel { get; } = new AddFaceViewModel();
        public AddFacePage()
        {
            InitializeComponent();
        }
    }
}
