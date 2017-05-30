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
        }

        private APISettingsService _apiSettingService;

        public APISettingsService ApiSettingService { get => _apiSettingService; set => _apiSettingService = value; }

        public async void Initialize()
        {
            await ApiSettingService.LoadAPIKeysFromSettingsAsync();
        }

    }
}
