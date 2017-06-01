using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ReceptionKiosk.Controls
{
    public sealed partial class ReceptionCamera : UserControl
    {
        MediaCapture _mediaCapture;
        bool _isPreviewing;
        DisplayRequest _displayRequest = new DisplayRequest();
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);
        public int CameraResolutionWidth { get; private set; }
        public int CameraResolutionHeight { get; private set; }
        public double CameraAspectRatio { get; set; }

        public ReceptionCamera()
        {
            this.InitializeComponent();
            photoButton.IsEnabled = false;
        }

        public void HideCameraControls()
        {
            this.commandBar.Visibility = Visibility.Collapsed;
        }

        public async Task StartPreviewAsync()
        {
            try
            {

                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                _displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                photoButton.IsEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                await ShowNoCameraAccessDialogAsync();
                return;
            }

            try
            {
                CameraPreview.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                //_mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }

        }

        //private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        //{
        //    if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
        //    {
        //        ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
        //    }
        //    else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !_isPreviewing)
        //    {
        //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //        {
        //            await StartPreviewAsync();
        //        });
        //    }
        //}

        public async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
                if (_isPreviewing)
                {
                    await _mediaCapture.StopPreviewAsync();
                    _isPreviewing = false;
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

        private async Task ShowNoCameraAccessDialogAsync()
        {
            ContentDialog noCameraAccess = new ContentDialog()
            {
                Title = "No access to camera",
                Content = "This app needs to access the default camera."
            };

            await noCameraAccess.ShowAsync();
        }

        private async void PreviewButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_isPreviewing)
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
            else
            {

            }
        }


        private async void CameraControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.cameraControlSymbol.Symbol == Symbol.Camera)
            {
                var img = await CaptureFrameAsync();
                if (img != null)
                {
                    this.cameraControlSymbol.Symbol = Symbol.Refresh;
                }
            }
            else
            {
                //this.cameraControlSymbol.Symbol = Symbol.Camera;

                await StartPreviewAsync();
            }
        }

        //private async Task SetVideoEncodingToHighestResolution()
        //{
        //    VideoEncodingProperties highestVideoEncodingSetting;

        //    // Sort the available resolutions from highest to lowest
        //    var availableResolutions = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Cast<VideoEncodingProperties>().OrderByDescending(v => v.Width * v.Height * (v.FrameRate.Numerator / v.FrameRate.Denominator));

        //    // Use the highest resolution
        //    highestVideoEncodingSetting = availableResolutions.FirstOrDefault();

        //    if (highestVideoEncodingSetting != null)
        //    {
        //        this.CameraAspectRatio = (double)highestVideoEncodingSetting.Width / (double)highestVideoEncodingSetting.Height;
        //        this.CameraResolutionHeight = (int)highestVideoEncodingSetting.Height;
        //        this.CameraResolutionWidth = (int)highestVideoEncodingSetting.Width;

        //        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, highestVideoEncodingSetting);
        //    }
        //}

        public async Task<Uri> CaptureFrameAsync()
        {
            try
            {
                if (!(await this.frameProcessingSemaphore.WaitAsync(250)))
                {
                    return null;
                }

                // Capture a frame from the preview stream
                //var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, CameraResolutionWidth, CameraResolutionHeight);
                //using (var currentFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame))
                //{
                //    using (SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap)
                //    {

                //    }
                //}

                // Prepare and capture photo
                //var lowLagCapture = await _mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

                //var capturedPhoto = await lowLagCapture.CaptureAsync();
                //var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

                //await lowLagCapture.FinishAsync();

                var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                StorageFile file = await myPictures.SaveFolder.CreateFileAsync($"photo-{DateTime.Now.ToString("yyyy-MM-dd-hh-m-s")}.jpg", CreationCollisionOption.GenerateUniqueName);

                using (var captureStream = new InMemoryRandomAccessStream())
                {
                    await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);

                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var decoder = await BitmapDecoder.CreateAsync(captureStream);
                        var encoder = await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder);

                        var properties = new BitmapPropertySet
                        {
                            { "System.Photo.Orientation", new BitmapTypedValue(PhotoOrientation.Normal, PropertyType.UInt16) }
                         };
                        await encoder.BitmapProperties.SetPropertiesAsync(properties);

                        await encoder.FlushAsync();
                    }
                }

                return new Uri(file.Path);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.frameProcessingSemaphore.Release();
            }

            return null;
        }
    }
}
