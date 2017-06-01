using ReceptionKiosk.Helpers;
using System.Threading.Tasks;
using Windows.Storage;

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
                SaveAPIKeysInSettingsAsync(nameof(FaceApiKey), _faceApiKey);                
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

        public async Task LoadAPIKeysFromSettingsAsync()
        {
            FaceApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(FaceApiKey));
            BingSearchApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(BingSearchApiKey));
            LinkedInApiKey = await ApplicationData.Current.LocalSettings.ReadAsync<string>(nameof(LinkedInApiKey));
        }

        private async void SaveAPIKeysInSettingsAsync(string SettingsKey, string APIKeyValue)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, APIKeyValue);
        }
    }
}
