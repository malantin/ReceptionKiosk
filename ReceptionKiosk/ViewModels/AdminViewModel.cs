using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using System.Threading.Tasks;

namespace ReceptionKiosk.ViewModels
{
    public class AdminViewModel : Observable
    {
        #region Properties

        private APISettingsService _apiSettingService;
        public APISettingsService ApiSettingService { get => _apiSettingService; set => _apiSettingService = value; }

        #endregion //Properties

        public AdminViewModel()
        {
            _apiSettingService = new APISettingsService();
        }

        public async Task InitializeAsync()
        {
            await ApiSettingService.LoadAPIKeysFromSettingsAsync();

            //Set default region to westeurope
            if (string.IsNullOrEmpty(ApiSettingService.FaceApiRegion))
                ApiSettingService.FaceApiRegion = "westeurope";
        }
    }
}
