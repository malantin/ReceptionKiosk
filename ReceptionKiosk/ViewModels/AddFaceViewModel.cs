using System;

using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using Microsoft.ProjectOxford.Face;
using System.Collections.ObjectModel;
using Microsoft.ProjectOxford.Face.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace ReceptionKiosk.ViewModels
{
    public class AddFaceViewModel : Observable
    {
        private static APISettingsService apiSettingsService = new APISettingsService();

        private FaceServiceClient fsc;

        public string NewFaceName { get; set; }

        #region Properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        #endregion

        /// <summary>
        /// Liste aller verfügbaren Gruppen
        /// </summary>
        public ObservableCollection<PersonGroup> PersonGroups { get; private set; }

        public AddFaceViewModel()
        {
            PersonGroups = new ObservableCollection<PersonGroup>();
        }

        public async Task Initialize()
        {
            IsLoading = true;

            await apiSettingsService.LoadAPIKeysFromSettingsAsync();
            fsc = new FaceServiceClient(apiSettingsService.FaceApiKey, "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            var personGroupResult = await fsc.ListPersonGroupsAsync();
            personGroupResult.OrderBy(pg => pg.Name);
            personGroupResult.ForEach(pg => PersonGroups.Add(pg));

            IsLoading = false;
        }
    }
}
