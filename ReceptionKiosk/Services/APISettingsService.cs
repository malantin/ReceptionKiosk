using System;
using System.Threading.Tasks;

using ReceptionKiosk.Helpers;

using Windows.Storage;
using Windows.UI.Xaml;

namespace ReceptionKiosk.Services
{
    public class APISettingsService
    {
        private string faceAPI;
        private string bingAPI;

        public APISettingsService()
        {
            LoadAPIKeysFromSettingsAsync();
        }

        public string FaceAPI
        {
            get { return faceAPI; }
            set
            {
                faceAPI = value;
                SaveAPIKeysInSettingsAsync("FaceAPI", faceAPI);
            }
        }

        public string BingAPI
        {
            get { return bingAPI; }
            set
            {
                bingAPI = value;
                SaveAPIKeysInSettingsAsync("BingAPI", bingAPI);
            }
        }

        private async void LoadAPIKeysFromSettingsAsync()
        {
            BingAPI = await ApplicationData.Current.LocalSettings.ReadAsync<string>("FaceAPI");
            FaceAPI = await ApplicationData.Current.LocalSettings.ReadAsync<string>("BingAPI");
        }

        private static async Task SaveAPIKeysInSettingsAsync(string SettingsKey, string APIKeyValue)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, APIKeyValue);
        }
    }
}
