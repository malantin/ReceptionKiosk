using Microsoft.ProjectOxford.Face;
using ReceptionKiosk.Helpers;
using System.Threading.Tasks;

namespace ReceptionKiosk.ViewModels
{
    public class MainViewModel : Observable
    {
        /// <summary>
        /// FaceServiceClient
        /// </summary>
        private FaceServiceClient FaceService { get; set; }

        #region Properties

        private string _message;
        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }
        
        #endregion //Properties

        public MainViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            if (FaceService == null)
                FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();
        }
    }
}
