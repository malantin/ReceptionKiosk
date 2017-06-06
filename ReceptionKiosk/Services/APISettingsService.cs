using ReceptionKiosk.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace ReceptionKiosk.Services
{
    public class APISettingsService : Observable
    {
        private string _faceApiKey;
        private string _bingSearchApiKey;
        private string _linkedInApiKey;
        private string _faceApiRegion;

        public APISettingsService()
        {

        }

        public string FaceApiKey
        {
            get { return _faceApiKey; }
            set
            {
                Set(ref _faceApiKey, value);
                SaveAPIKeysInSettingsAsync(nameof(FaceApiKey), _faceApiKey);
            }
        }

        public string FaceApiRegion
        {
            get { return _faceApiRegion; }
            set
            {
                Set(ref _faceApiRegion, value);
                SaveAPIKeysInSettingsAsync(nameof(FaceApiRegion), _faceApiRegion);
            }
        }

        public string BingSearchApiKey
        {
            get { return _bingSearchApiKey; }
            set
            {
                Set(ref _bingSearchApiKey, value);
                SaveAPIKeysInSettingsAsync(nameof(BingSearchApiKey), _bingSearchApiKey);
            }
        }

        public string LinkedInApiKey
        {
            get { return _linkedInApiKey; }
            set
            {
                Set(ref _linkedInApiKey, value);
                SaveAPIKeysInSettingsAsync(nameof(LinkedInApiKey), _linkedInApiKey);
            }
        }

        public List<string> AvailableRegions
        {
            get
            {
                return new List<string>() { "eastus2", "southeastasia", "westcentralus", "westeurope", "westus" };
            }
        }

        public async Task LoadAPIKeysFromSettingsAsync()
        {
            FaceApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(FaceApiKey));
            FaceApiRegion = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(FaceApiRegion));
            BingSearchApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(BingSearchApiKey));
            LinkedInApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(LinkedInApiKey));
        }

        private async void SaveAPIKeysInSettingsAsync(string SettingsKey, string APIKeyValue)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, APIKeyValue);
        }
    }
}
