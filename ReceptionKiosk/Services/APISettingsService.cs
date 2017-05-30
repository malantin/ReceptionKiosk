using System;
using System.Threading.Tasks;
using ReceptionKiosk.Helpers;
using System.ComponentModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ReceptionKiosk.Services
{
    public class APISettingsService : Observable
    {
        private string _faceApiKey;
        private string _bingSearchApiKey;
        private string _linkedInApiKey;

        public APISettingsService()
        {
        }

        public string FaceApiKey
        {
            get { return _faceApiKey; }
            set
            {
                Set(ref _faceApiKey, value);
                SaveAPIKeysInSettingsAsync("FaceApiKey", _faceApiKey);                
            }
        }

        public string BingSearchApiKey
        {
            get { return _bingSearchApiKey; }
            set
            {
                Set(ref _bingSearchApiKey, value);
                SaveAPIKeysInSettingsAsync("BingSearchApiKey", _bingSearchApiKey);                
            }
        }

        public string LinkedInApiKey
        {
            get { return _linkedInApiKey; }
            set
            {
                Set(ref _linkedInApiKey, value);
                SaveAPIKeysInSettingsAsync("LinkedInApiKey", _linkedInApiKey);                
            }
        }

        public async Task LoadAPIKeysFromSettingsAsync()
        {
            FaceApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("FaceApiKey");
            BingSearchApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("BingSearchApiKey");
            LinkedInApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("LinkedInApiKey");
        }

        private async void SaveAPIKeysInSettingsAsync(string SettingsKey, string APIKeyValue)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, APIKeyValue);
        }
    }
}
