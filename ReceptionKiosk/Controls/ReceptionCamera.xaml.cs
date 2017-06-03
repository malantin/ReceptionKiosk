﻿using ReceptionKiosk.Helpers;
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
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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

        ObservableCollection<BitmapWrapper> _pictures;

        public ObservableCollection<BitmapWrapper> Pictures { get { return _pictures; } set { _pictures = value; } }

        //Is the camera preview active?
        private bool _isPreviewing;

        private DisplayRequest _displayRequest = new DisplayRequest();
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);

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
            IsPreviewing = true;
            try
            {
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                _displayRequest.RequestActive();
                await SetVideoEncodingToHighestResolution();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
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
            }
            catch (System.IO.FileLoadException)
            {
                //TODO Handle the exception
            }

        }

        /// <summary>
        /// Stops the camera preview and releases all resources
        /// </summary>
        /// <returns></returns>
        public async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
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

                //using (var captureStream = new InMemoryRandomAccessStream())
                //{
                //    await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);

                //    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                //    {
                //        var decoder = await BitmapDecoder.CreateAsync(captureStream);
                //        var encoder = await BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder);

                //        var properties = new BitmapPropertySet
                //        {
                //            { "System.Photo.Orientation", new BitmapTypedValue(PhotoOrientation.Normal, PropertyType.UInt16) }
                //         };
                //        await encoder.BitmapProperties.SetPropertiesAsync(properties);

                //        await encoder.FlushAsync();
                //    }
                //}

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
