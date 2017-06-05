using Microsoft.ProjectOxford.Face;
using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using System;
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
            try
            {
                if (FaceService == null)
                    FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();
            }
            catch (FaceAPIException ex)//Handle API-Exception
            {
                await MessageDialogHelper.MessageDialogAsync(ex.ErrorMessage);
            }
            catch (Exception ex)
            {
                await MessageDialogHelper.MessageDialogAsync(ex.Message);
            }
        }
    }
}
