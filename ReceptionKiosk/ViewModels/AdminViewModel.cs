using System;
using ReceptionKiosk.Services;
using ReceptionKiosk.Helpers;

namespace ReceptionKiosk.ViewModels
{
    public class AdminViewModel : Observable
    {
        public AdminViewModel()
        {
            _apiSettingService = new APISettingsService();
            _apiSettingService.LoadAPIKeysFromSettingsAsync();
        }

        private APISettingsService _apiSettingService;

        public APISettingsService ApiSettingService { get => _apiSettingService; set => _apiSettingService = value; }

        public void Initialize()
        {

        }

    }
}
