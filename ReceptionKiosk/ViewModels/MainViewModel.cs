using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using ReceptionKiosk.Controls;
using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace ReceptionKiosk.ViewModels
{
    public class MainViewModel : Observable
    {
        /// <summary>
        /// Resource loader for localized strings
        /// </summary>
        ResourceLoader loader = new Windows.ApplicationModel.Resources.ResourceLoader();

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

        private int _APICalls;
        public int APICalls
        {
            get { return _APICalls; }
            set { Set(ref _APICalls, value); }
        }

        #endregion //Properties

        private ReceptionCamera _cameraControl;

        private Guid[] _lastFaces;

        private bool _loopInProgress = false;
        private Task _loopTask = null;

        private PersonGroup[] _personGroups = null; 

        public MainViewModel(ReceptionCamera cameraControl)
        {
            _cameraControl = cameraControl;
            APICalls = 0;
            Message = loader.GetString("Main_Message_Init");
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (FaceService == null)
                    FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();

                //Load all facegroups
                _personGroups = await FaceService.ListPersonGroupsAsync();
            }
            catch (FaceAPIException ex)//Handle API-Exception
            {
                await MessageDialogHelper.MessageDialogAsync(ex.ErrorMessage);
            }
            catch (Exception ex)
            {
                await MessageDialogHelper.MessageDialogAsync(ex.Message);
            }

            startLoop();
        }

        private void startLoop()
        {
            this._loopInProgress = true;

            if (this._loopTask == null || this._loopTask.Status != TaskStatus.Running)
            {
                this._loopTask = Task.Run(() => this.loop());
            }
        }

        public void stopLoop()
        {
            this._loopInProgress = false;
        }

        private async void loop()
        {
            while (this._loopInProgress)
            {              

                if (_lastFaces != _cameraControl.LastFaces && _cameraControl.LastFaces != null)
                {
                    
                    _lastFaces = _cameraControl.LastFaces;

                    //await
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        bool _personFound = false;
                        if (_personGroups != null)
                        {
                            foreach (var group in _personGroups)
                            {
                                foreach (var face in _cameraControl.LastFaces)
                                {
                                    var results = await FaceService.IdentifyAsync(group.PersonGroupId, _cameraControl.LastFaces);
                                    APICalls++;
                                    foreach (var result in results)
                                    {
                                        if (result.Candidates.Length > 0)
                                        {
                                            var resultCandidate = result.Candidates[0];

                                            //Let's start with 50% confidence for now
                                            if (resultCandidate.Confidence > 0.5)
                                            {
                                                var identifiedPerson = await FaceService.GetPersonAsync(group.PersonGroupId, resultCandidate.PersonId);
                                                APICalls++;
                                                Message = String.Format(loader.GetString("Main_Message_Back"), identifiedPerson.Name);

                                                //Currently we take the first best person
                                                _personFound = true;
                                                break;
                                            }
                                        }
                                    }

                                    //Currently we take the first best person
                                    if (_personFound)
                                    {
                                        break;
                                    }
                                }

                                //Currently we take the first best person
                                if (_personFound)
                                {
                                    break;
                                }
                            }
                        }
                    });

                }

                //Wait 5 seconds to save API calls
                await Task.Delay(5000);
            }
        }
    }
}
