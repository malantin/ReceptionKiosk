using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceptionKiosk.Helpers
{
    public class ImageAnalyzer
    {
        private FaceServiceClient FaceService { get; set; }

        /// <summary>
        /// Erkannte Gesichter
        /// </summary>
        public IEnumerable<Face> DetectedFaces { get; private set; }
    }
}
