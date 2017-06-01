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
        /// FaceServiceClient URL
        /// </summary>
        private static string faceServiceUrl { get; set; } = "https://westeurope.api.cognitive.microsoft.com/face/v1.0";

        /// <summary>
        /// Erstellt eine FaceServiceClient Instanz
        /// </summary>
        /// <returns></returns>
        public static async Task<FaceServiceClient> CreateNewFaceServiceAsync()
        {
            await apiSettingsService.LoadAPIKeysFromSettingsAsync();

            return new FaceServiceClient(apiSettingsService.FaceApiKey, faceServiceUrl);
        }
    }
}
