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
            LoadAPIKeysFromSettingsAsync();
            //FaceApiKey = "djfklsdjflkjsdflke";
        }

        public string FaceApiKey
        {
            get { return _faceApiKey; }
            set
            {
                _faceApiKey = value;
                SaveAPIKeysInSettingsAsync("FaceApiKey", _faceApiKey);
                Set(ref _faceApiKey, value);
            }
        }

        public string BingSearchApiKey
        {
            get { return _bingSearchApiKey; }
            set
            {
                _bingSearchApiKey = value;
                SaveAPIKeysInSettingsAsync("BingSearchApiKey", _bingSearchApiKey);
                Set(ref _faceApiKey, value);
            }
        }

        public string LinkedInApiKey
        {
            get { return _linkedInApiKey; }
            set
            {
                _linkedInApiKey = value;
                SaveAPIKeysInSettingsAsync("LinkedInApiKey", _linkedInApiKey);
                Set(ref _faceApiKey, value);
            }
        }

        public async void LoadAPIKeysFromSettingsAsync()
        {
            BingSearchApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("BingSearchApiKey");
            FaceApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("FaceApiKey");
            LinkedInApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>("LinkedInApiKey");
        }

        private static async Task SaveAPIKeysInSettingsAsync(string SettingsKey, string APIKeyValue)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, APIKeyValue);
        }
    }
}
