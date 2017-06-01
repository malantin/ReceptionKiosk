using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.ViewModels
{
    public class ManageFacesViewModel : Observable
    {
        /// <summary>
        /// FaceServiceClient Instanz
        /// </summary>
        private FaceServiceClient faceService { get; set; }

        #region Commands

        /// <summary>
        /// Button Gruppe hinzufuegen
        /// </summary>
        public ICommand AddGroupCommand { get; private set; }

        #endregion //Commands

        #region Collections

        /// <summary>
        /// Liste aller verfügbaren Gruppen
        /// </summary>
        public ObservableCollection<PersonGroup> PersonGroups { get; private set; }

        /// <summary>
        /// Alle Personen einer Gruppe
        /// </summary>
        public ObservableCollection<Person> Persons { get; set; }

        /// <summary>
        /// Alle Bilder zu einer Person
        /// </summary>
        public ObservableCollection<PersonFace> PersonFaces { get; set; }

        #endregion //Collections

        #region Properties

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        //TODO: Später an UI_Element binden
        private PersonGroup _selectedPersonGroup;
        public PersonGroup SelectedPersonGroup
        {
            get { return _selectedPersonGroup; }
            set { Set(ref _selectedPersonGroup, value); }
        }

        //TODO: Später an UI_Element binden
        private Person _selectedPerson;
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set { Set(ref _selectedPerson, value); }
        }

        private string _countOfFaces;
        public string CountOfFaces
        {
            get { return _countOfFaces; }
            set { Set(ref _countOfFaces, value); }
        }

        private string _groupToAdd;
        public string GroupToAdd
        {
            get { return _groupToAdd; }
            set { Set(ref _groupToAdd, value); }
        }


        #endregion //Properties

        /// <summary>
        /// CTOR
        /// </summary>
        public ManageFacesViewModel()
        {
            //ObservableCollection initialisieren
            PersonGroups = new ObservableCollection<PersonGroup>();
            Persons = new ObservableCollection<Person>();
            PersonFaces = new ObservableCollection<PersonFace>();

            //Command initialisieren
            AddGroupCommand = new RelayCommand(async () => await ExecuteAddGroupCommandAsync(), CanExecuteAddGroupCommand);
        }

        #region AddGroupCommand
        private async Task ExecuteAddGroupCommandAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(GroupToAdd))
                    throw new ArgumentNullException(nameof(GroupToAdd), "Please enter a group name.");

                await faceService.CreatePersonGroupAsync(Guid.NewGuid().ToString(), GroupToAdd);
                await (new MessageDialog("Your group is added.")).ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fehler");
                await dialog.ShowAsync();
            }
        }

        private bool CanExecuteAddGroupCommand()
        {
            return !string.IsNullOrEmpty(GroupToAdd);
        }
        #endregion //AddGroupCommand

        public async void OnPersonGroupChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                var listBoxPersonGroup = sender as ListBox;

                if (listBoxPersonGroup?.SelectedItem != null)
                {
                    if (listBoxPersonGroup.SelectedItem is PersonGroup)
                    {
                        SelectedPersonGroup = listBoxPersonGroup.SelectedItem as PersonGroup;
                        if (SelectedPersonGroup != null)
                        {
                            var persons = await faceService.ListPersonsAsync(SelectedPersonGroup.PersonGroupId);
                            persons.ForEach(p => Persons.Add(p));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fehler");
                await dialog.ShowAsync();
            }
        }

        public async void OnPersonChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                var listBoxPerson = sender as ListBox;

                if (listBoxPerson?.SelectedItem != null)
                {
                    if (listBoxPerson.SelectedItem is Person)
                    {
                        SelectedPerson = listBoxPerson.SelectedItem as Person;
                        if (SelectedPerson != null)
                        {
                            ////TODO: Muss noch implementiert werden
                            //foreach (Guid faceId in SelectedPerson.PersistedFaceIds)
                            //{
                            //    var personFace = await fsc.GetPersonFaceAsync(SelectedPersonGroup.PersonGroupId, SelectedPerson.PersonId, faceId);
                            //    PersonFaces.Add(personFace);
                            //}
                            CountOfFaces = $"{SelectedPerson.PersistedFaceIds.Count()} trained faces.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fehler");
                await dialog.ShowAsync();
            }
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;

            if (faceService == null)
                faceService = await FaceServiceHelper.CreateNewFaceServiceAsync();

            var personGroups = await faceService.ListPersonGroupsAsync();
            personGroups.OrderBy(pg => pg.Name);
            personGroups.ForEach(pg => PersonGroups.Add(pg));

            IsLoading = false;
        }
    }
}
