using Microsoft.ProjectOxford.Face;
using ReceptionKiosk.Services;
using System.Threading.Tasks;

namespace ReceptionKiosk.Helpers
{
    public class FaceServiceHelper
    {
        /// <summary>
        /// API Settings
        /// </summary>
        private static APISettingsService apiSettingsService { get; set; } = new APISettingsService();

        /// <summary>
        /// FaceServiceClient base URL
        /// </summary>
        private static string faceServiceBaseUrl { get; set; } = "https://{0}.api.cognitive.microsoft.com/face/v1.0";//"https://westeurope.api.cognitive.microsoft.com/face/v1.0";
        
        /// <summary>
        /// Create a FaceServiceClient Instance with initialized API-Keys
        /// </summary>
        /// <returns></returns>
        public static async Task<FaceServiceClient> CreateNewFaceServiceAsync()
        {
            await apiSettingsService.LoadAPIKeysFromSettingsAsync();

            return new FaceServiceClient(apiSettingsService.FaceApiKey, string.Format(faceServiceBaseUrl, apiSettingsService.FaceApiRegion));
        }
    }
}
