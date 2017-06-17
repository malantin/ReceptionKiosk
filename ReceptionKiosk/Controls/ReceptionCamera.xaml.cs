using ReceptionKiosk.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Core;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.ProjectOxford.Face;
using Windows.Storage.Streams;
using System.IO;
using Microsoft.ProjectOxford.Face.Contract;
using System.Collections.Generic;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ReceptionKiosk.Controls
{
    public sealed partial class ReceptionCamera : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Resource loader for localized strings
        /// </summary>
        ResourceLoader loader = new Windows.ApplicationModel.Resources.ResourceLoader();

        public MediaCapture _mediaCapture;

        /// <summary>
        /// FaceServiceClient instance
        /// </summary>
        private FaceServiceClient FaceService { get; set; }

        ObservableCollection<BitmapWrapper> _pictures;

        FaceDetectionEffect _faceDetectionEffect;

        public Guid[] LastFaces { get; set; }

        public ObservableCollection<BitmapWrapper> Pictures { get { return _pictures; } set { _pictures = value; } }

        //Is the camera preview active?
        private bool _isPreviewing = false;

        //Is face detection active?
        private bool _isDetecting = false;

        //Is there in identification call in progress?
        private bool _isRecognizing = false;

        private DisplayRequest _displayRequest = new DisplayRequest();
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);

        public static readonly DependencyProperty PhotoButtonVisibleProperty = 
            DependencyProperty.Register(
                "PhotoButtonVisible",
                typeof(bool),
                typeof(ReceptionCamera),
                new PropertyMetadata(true, OnValueChanged));

        public bool PhotoButtonVisible
        {
            get { return (bool)GetValue(PhotoButtonVisibleProperty); }
            set { SetValue(PhotoButtonVisibleProperty, value);
                if (value == true)
                {
                    photoButton.Visibility = Visibility.Visible;
                }
                else
                {
                    photoButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static readonly DependencyProperty FaceRecognitionActiveProperty =
            DependencyProperty.Register(
                "FaceRecognitionActive",
                typeof(bool),
                typeof(ReceptionCamera),
                new PropertyMetadata(false, OnValueChanged));

        public bool FaceRecognitionActive
        {
            get { return (bool)GetValue(FaceRecognitionActiveProperty); }
            set
            {
                SetValue(FaceRecognitionActiveProperty, value);
            }
        }

        private static void OnValueChanged(DependencyObject d,
        DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Is the camera preview active?
        /// </summary>
        public bool IsPreviewing
        {
            get { return _isPreviewing;}
            set { _isPreviewing = value; OnPropertyChanged("IsPreviewing"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int CameraResolutionWidth { get; private set; }
        public int CameraResolutionHeight { get; private set; }
        public double CameraAspectRatio { get; set; }    

        public ReceptionCamera()
        {
            this.InitializeComponent();
            IsPreviewing = false;
            LastFaces = null;
        }

        /// <summary>
        /// Used to just display the camera preview with no buttons
        /// </summary>
        /// <param name="previewActive"></param>
        /// <param name="controlsVisible"></param>
        public ReceptionCamera(bool previewActive, bool controlsVisible)
        {
            this.InitializeComponent();
            IsPreviewing = previewActive;
            if (!controlsVisible)
            {
                HideCameraControls();
            }
        }

        void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Handle a face detected event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            // Only run one face detection call to Cognitive Services at a time
            if (!_isRecognizing)
            {
                //If we need the box for the detected face we can get them here
                //foreach (Windows.Media.FaceAnalysis.DetectedFace face in args.ResultFrame.DetectedFaces)
                //{
                //    BitmapBounds faceRect = face.FaceBox;
                //}

                _isRecognizing = true;

                var lowLagCapture = await _mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                var capturedPhoto = await lowLagCapture.CaptureAsync();
                var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                await lowLagCapture.FinishAsync();

                using (IRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream())
                {

                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream);

                    encoder.SetSoftwareBitmap(softwareBitmap);

                    await encoder.FlushAsync();

                    var stream = randomAccessStream.AsStreamForRead();

                    try
                    { 
                        //This call the Cognitive Services face API to detect the faces
                        var faces = await FaceService.DetectAsync(stream, true, false);

                        List<Guid> faceList = new List<Guid>();

                        foreach (var face in faces)
                        {
                            faceList.Add(face.FaceId);
                        }

                        LastFaces = faceList.ToArray();
                    }
                    catch
                    {
                        //We could not detect faces using Cognitive Services
                    }
                }

                _isRecognizing = false;
            }
        }

        /// <summary>
        /// Hides the grid that contains the preview and photo button
        /// </summary>
        public void HideCameraControls()
        {
            this.commandBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Starts the media capture on the default camera and displays the video stream
        /// </summary>
        /// <returns></returns>
        public async Task StartPreviewAsync()
        {
            try
            {
                IsPreviewing = true;
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                _displayRequest.RequestActive();
                await SetVideoEncodingToHighestResolution();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                IsPreviewing = false;
                // This will be thrown if the user denied access to the camera in privacy settings
                await ShowNoCameraAccessDialogAsync();
                return;
            }

            try
            {
                CameraPreview.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
            }
            catch (System.IO.FileLoadException)
            {
                //TODO Handle the exception
            }

            if (FaceRecognitionActive)
            {
                await InitializeFaceDetection();
            }
        }

        /// <summary>
        /// Initializes face detection on the preview stream, from https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/scene-analysis-for-media-capture
        /// </summary>
        /// <returns></returns>
        public async Task InitializeFaceDetection()
        {
            // Load the face service client to to face recognition with Cognitive Services
            if (FaceService == null)
                FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();

            // Create the definition, which will contain some initialization settings
            var definition = new FaceDetectionEffectDefinition();

            // To ensure preview smoothness, do not delay incoming samples
            definition.SynchronousDetectionEnabled = false;

            // In this scenario, choose detection speed over accuracy
            definition.DetectionMode = FaceDetectionMode.HighPerformance;

            // Add the effect to the preview stream
            _faceDetectionEffect = (FaceDetectionEffect)await _mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

            // TODO: Chance to a good frequency to save Cognitive Services API calls
            // Choose the shortest interval between detection events
            //_faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(33);
            // Currently we offline detect faces every 3 seconds to save API calls
            _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(3000);

            // Start detecting faces
            _faceDetectionEffect.Enabled = true;

            // Register for face detection events
            _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

            _isDetecting = true;
        }

        /// <summary>
        /// Stops the camera preview and releases all resources
        /// </summary>
        /// <returns></returns>
        public async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
                if (_isDetecting)
                {
                    // Disable detection
                    _faceDetectionEffect.Enabled = false;

                    // Unregister the event handler
                    _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;

                    // Remove the effect from the preview stream
                    await _mediaCapture.ClearEffectsAsync(MediaStreamType.VideoPreview);

                    // Clear the member variable that held the effect instance
                    _faceDetectionEffect = null;

                    _isDetecting = false;
                }

                if (IsPreviewing)
                {
                    await _mediaCapture.StopPreviewAsync();
                    IsPreviewing = false;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CameraPreview.Source = null;
                    if (_displayRequest != null)
                    {
                        _displayRequest.RequestRelease();
                    }

                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                });
            }

        }

        /// <summary>
        /// Display a dialog if access to the camera was denied
        /// </summary>
        /// <returns></returns>
        private async Task ShowNoCameraAccessDialogAsync()
        {
            ContentDialog noCameraAccess = new ContentDialog()
            {
                Title = loader.GetString("CameraControl_NoAccessToCameraTitle"),
                Content = loader.GetString("CameraControl_NoAccessToCameraText"),
                PrimaryButtonText = "OK"
            };

            await noCameraAccess.ShowAsync();
        }

        /// <summary>
        /// Starts the camera preview on the capture element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PreviewButtonClick(object sender, RoutedEventArgs e)
        {
            if (!IsPreviewing)
            {
                try
                {
                    await StartPreviewAsync();
                    previewButton.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    await new MessageDialog(ex.Message).ShowAsync();
                }
            }
        }

        /// <summary>
        /// Handles the camera button click to take a picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CameraControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsPreviewing)
            {
                var img = await CaptureFrameAsync();
                if (img != null)
                {
                    this.cameraControlSymbol.Symbol = Symbol.Refresh;
                }
            }
            else
            {
                await StartPreviewAsync();
            }
        }

        /// <summary>
        /// Resets the control to the initial state
        /// </summary>
        public void Reset()
        {
            IsPreviewing = false;
            CameraPreview.Source = null;
            previewButton.IsEnabled = true;
        }

        private async Task SetVideoEncodingToHighestResolution()
        {
            VideoEncodingProperties highestVideoEncodingSetting;

            // Sort the available resolutions from highest to lowest
            var availableResolutions = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Cast<VideoEncodingProperties>().OrderByDescending(v => v.Width * v.Height * (v.FrameRate.Numerator / v.FrameRate.Denominator));

            // Use the highest resolution
            highestVideoEncodingSetting = availableResolutions.FirstOrDefault();

            if (highestVideoEncodingSetting != null)
            {
                this.CameraAspectRatio = (double)highestVideoEncodingSetting.Width / (double)highestVideoEncodingSetting.Height;
                this.CameraResolutionHeight = (int)highestVideoEncodingSetting.Height;
                this.CameraResolutionWidth = (int)highestVideoEncodingSetting.Width;

                await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, highestVideoEncodingSetting);
            }
        }


        /// <summary>
        /// Captures a frame from the camera preview and saves the photo
        /// </summary>
        /// <returns></returns>
        public async Task<SoftwareBitmap> CaptureFrameAsync()
        {
            try
            {
                if (!(await this.frameProcessingSemaphore.WaitAsync(250)))
                {
                    return null;
                }

                var myPictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                StorageFile file = await myPictures.SaveFolder.CreateFileAsync($"photo-{DateTime.Now.ToString("yyyy-MM-dd-hh-m-s")}.jpg", CreationCollisionOption.GenerateUniqueName);

                var lowLagCapture = await _mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                var capturedPhoto = await lowLagCapture.CaptureAsync();
                var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                await lowLagCapture.FinishAsync();

                //Save image to disk
                using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Create an encoder with the desired format
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, filestream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();

                    // Create SoftwareBitmapSource and save to Pictures collection if available
                    if (Pictures != null)
                    {
                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        var softwareBitmapSource = new SoftwareBitmapSource();
                        await softwareBitmapSource.SetBitmapAsync(softwareBitmap);
                        Pictures.Add(new BitmapWrapper(softwareBitmap, softwareBitmapSource));
                    }                    
                }

                return softwareBitmap;
            }
            catch (Exception ex)
            {
                //TODO Handle the exception
            }
            finally
            {
                this.frameProcessingSemaphore.Release();
            }

            return null;
        }

        /// <summary>
        /// Adds a reference to a picture collection
        /// </summary>
        /// <param name="pictures"></param>
        internal void SetPictureLib(ObservableCollection<BitmapWrapper> pictures)
        {
            Pictures = pictures;
        }
    }
}
