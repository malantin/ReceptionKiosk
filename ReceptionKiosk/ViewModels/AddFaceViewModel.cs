using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using ReceptionKiosk.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace ReceptionKiosk.ViewModels
{
    public class AddFaceViewModel : Observable
    {
        /// <summary>
        /// FaceServiceClient Instanz
        /// </summary>
        private FaceServiceClient FaceService { get; set; }

        #region Commands

        public ICommand AddPersonCommand { get; set; }

        public ICommand BrowsePictureCommand { get; set; }

        #endregion //Commands

        #region Properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private string _newFaceName;
        public string NewFaceName
        {
            get { return _newFaceName; }
            set { Set(ref _newFaceName, value); }
        }

        private PersonGroup _selectedPersonGroup;
        public PersonGroup SelectedPersonGroup
        {
            get { return _selectedPersonGroup; }
            set { Set(ref _selectedPersonGroup, value); }
        }

        #endregion

        /// <summary>
        /// Liste aller verfügbaren Gruppen
        /// </summary>
        public ObservableCollection<PersonGroup> PersonGroups { get; private set; }

        /// <summary>
        /// Liste aller Bilder
        /// </summary>
        public ObservableCollection<SoftwareBitmapSource> Pictures { get; private set; }

        public AddFaceViewModel()
        {
            PersonGroups = new ObservableCollection<PersonGroup>();
            Pictures = new ObservableCollection<SoftwareBitmapSource>();

            AddPersonCommand = new RelayCommand(async () => await ExecuteAddPersonCommand());
            BrowsePictureCommand = new RelayCommand(async () => await ExecuteBrowsePictureCommandAsync());
        }

        #region BrowsePictureCommand

        private async Task ExecuteBrowsePictureCommandAsync()
        {
            try
            {
                FileOpenPicker fileOpenPicker = new FileOpenPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary, ViewMode = PickerViewMode.Thumbnail };
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".bmp");
                IReadOnlyList<StorageFile> selectedFiles = await fileOpenPicker.PickMultipleFilesAsync();
                
                if (selectedFiles != null)
                {
                    foreach (var item in selectedFiles)
                    {
                        SoftwareBitmap softwareBitmap;

                        using (IRandomAccessStream stream = await item.OpenAsync(FileAccessMode.Read))
                        {
                            // Create the decoder from the stream
                            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                            // Get the SoftwareBitmap representation of the file
                            softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                            var softwareBitmapSource = new SoftwareBitmapSource();
                            await softwareBitmapSource.SetBitmapAsync(softwareBitmap);

                            Pictures.Add(softwareBitmapSource);
                        }

                        //var bitmap = new SoftwareBitmap()
                        //using (var stream = await item.OpenReadAsync())
                        //{
                        //    await bitmap.SetSourceAsync(stream);
                        //    Pictures.Add(bitmap);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        #endregion //BrowsePictureCommand

        private async Task ExecuteAddPersonCommand()
        {
            string br = "";
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;

            if (FaceService == null)
                FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();

            var personGroupResult = await FaceService.ListPersonGroupsAsync();
            personGroupResult.OrderBy(pg => pg.Name);
            personGroupResult.ForEach(pg => PersonGroups.Add(pg));

            IsLoading = false;
        }
    }
}
