using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using ReceptionKiosk.Helpers;
using ReceptionKiosk.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ReceptionKiosk.ViewModels
{
    public class ManageFacesViewModel : Observable
    {
        /// <summary>
        /// FaceServiceClient Instance
        /// </summary>
        private FaceServiceClient FaceService { get; set; }

        #region Commands

        /// <summary>
        /// Add Group Command
        /// </summary>
        public RelayCommand AddGroupCommand { get; private set; }

        /// <summary>
        /// Add Person Command
        /// </summary>
        public RelayCommand AddPersonCommand { get; private set; }

        /// <summary>
        /// Delete Command
        /// </summary>
        public RelayCommand DeleteCommand { get; private set; }

        /// <summary>
        /// Train Command
        /// </summary>
        public RelayCommand TrainCommand { get; private set; }

        #endregion //Commands

        #region Collections

        /// <summary>
        /// List of all available groups
        /// </summary>
        public ObservableCollection<PersonGroup> PersonGroups { get; private set; }

        /// <summary>
        /// All persons of a group
        /// </summary>
        public ObservableCollection<Person> Persons { get; set; }

        /// <summary>
        /// All picture for a face
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

        private PersonGroup _selectedPersonGroup;
        public PersonGroup SelectedPersonGroup
        {
            get { return _selectedPersonGroup; }
            set { Set(ref _selectedPersonGroup, value); DeleteCommand.OnCanExecuteChanged(); TrainCommand.OnCanExecuteChanged(); }
        }

        private Person _selectedPerson;
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set { Set(ref _selectedPerson, value); DeleteCommand.OnCanExecuteChanged(); }
        }

        private PersonGroup _selectedGroupToAddPerson;
        public PersonGroup SelectedGroupToAddPerson
        {
            get { return _selectedGroupToAddPerson; }
            set { Set(ref _selectedGroupToAddPerson, value); AddPersonCommand.OnCanExecuteChanged(); }
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
            set { Set(ref _groupToAdd, value); AddGroupCommand.OnCanExecuteChanged(); }
        }

        private string _personToAdd;
        public string PersonToAdd
        {
            get { return _personToAdd; }
            set { Set(ref _personToAdd, value); }
        }

        #endregion //Properties

        /// <summary>
        /// CTOR
        /// </summary>
        public ManageFacesViewModel()
        {
            //ObservableCollection initialize
            PersonGroups = new ObservableCollection<PersonGroup>();
            Persons = new ObservableCollection<Person>();
            PersonFaces = new ObservableCollection<PersonFace>();

            //Command initialize
            AddGroupCommand = new RelayCommand(async () => await ExecuteAddGroupCommandAsync(), CanExecuteAddGroupCommandAsync);
            AddPersonCommand = new RelayCommand(async () => await ExecuteAddPersonCommandAsync(), CanExecuteAddPersonCommandAsync);

            DeleteCommand = new RelayCommand(async () => await ExecuteDeleteCommandAsync(), CanExecuteDeleteCommandAsync);
            TrainCommand = new RelayCommand(async () => await ExecuteTrainCommandAsync(), CanExecuteTrainCommandAsync);

        }

        private async Task ExecuteTrainCommandAsync()
        {
            try
            {
                IsLoading = true;
                await FaceService.TrainPersonGroupAsync(SelectedPersonGroup.PersonGroupId);
                IsLoading = false;
                await MessageDialogHelper.MessageDialogAsync($"Group {SelectedPersonGroup.Name} has successfully been trained.");
            }
            catch (Exception e)
            {
                await MessageDialogHelper.MessageDialogAsync("Group could not be trained", e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public bool CanExecuteTrainCommandAsync()
        { return SelectedPersonGroup != null; }

        #region AddGroupCommand
        private async Task ExecuteAddGroupCommandAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(GroupToAdd))
                    throw new ArgumentNullException(nameof(GroupToAdd), "Please enter a group name.");

                //Remember which group was selected by its unique ID
                string tempSelectedGroupId = null;
                if (SelectedPersonGroup != null)
                {
                    tempSelectedGroupId = SelectedPersonGroup.PersonGroupId;
                }

                await FaceService.CreatePersonGroupAsync(Guid.NewGuid().ToString(), GroupToAdd);
                await (new MessageDialog($"'{GroupToAdd}' successfully added.")).ShowAsync();

                //Cleanup UI
                GroupToAdd = string.Empty;
                await LoadGroupsAsync();

                //Set the selected group back to the group we had selected before
                if (tempSelectedGroupId != null)
                {
                    foreach (var group in PersonGroups)
                    {
                        if (group.PersonGroupId.Equals(tempSelectedGroupId))
                        {
                            SelectedPersonGroup = group;
                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Group could not be added.");
                await dialog.ShowAsync();
            }
        }

        private bool CanExecuteAddGroupCommandAsync()
        {
            //TODO: Only for developement reasons
            //return !string.IsNullOrEmpty(GroupToAdd);
            return true;
        }
        #endregion //AddGroupCommand

        #region AddPersonCommand
        private async Task ExecuteAddPersonCommandAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(PersonToAdd))
                    throw new ArgumentNullException(nameof(GroupToAdd), "Please enter a person name.");

                if (SelectedGroupToAddPerson == null)
                    throw new ArgumentNullException(nameof(SelectedGroupToAddPerson), "Please select a group.");

                await FaceService.CreatePersonAsync(SelectedGroupToAddPerson.PersonGroupId, PersonToAdd);
                await MessageDialogHelper.MessageDialogAsync($"'{PersonToAdd}' successfully added.");

                //Cleanup UI
                PersonToAdd = string.Empty;
                if (SelectedPersonGroup != null) //Load Persons if some selected a group
                    await LoadPersonsOfGroupAsync(SelectedPersonGroup.PersonGroupId);
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fehler");
                await dialog.ShowAsync();
            }
        }

        private bool CanExecuteAddPersonCommandAsync()
        {
            return SelectedGroupToAddPerson != null;
        }
        #endregion //AddPersonCommand

        #region DeleteCommand
        private async Task ExecuteDeleteCommandAsync()
        {
            if (SelectedPersonGroup != null)
            {
                if (SelectedPerson != null)
                {
                    await MessageDialogHelper.ConfirmDialogAsync("Deleting Person",
                                                                $"Are you sure you want to delete '{SelectedPerson.Name}'?",
                                                                async () =>
                                                                {
                                                                    await FaceService.DeletePersonAsync(SelectedPersonGroup.PersonGroupId, SelectedPerson.PersonId);
                                                                    await MessageDialogHelper.MessageDialogAsync("Deleting Person", $"'{SelectedPerson.Name}' successfully deleted.");
                                                                },
                                                                async () => await MessageDialogHelper.MessageDialogAsync("Deleting Person", "Deleting was canceled by user."));

                    //Cleanup
                    SelectedPerson = null;
                    await LoadPersonsOfGroupAsync(SelectedPersonGroup.PersonGroupId);
                }
                else
                {
                    //Delete PersonGroup
                    await MessageDialogHelper.ConfirmDialogAsync("Deleting Group",
                                                                $"Are you sure you want to delete '{SelectedPersonGroup.Name}'?",
                                                                async () =>
                                                                    {
                                                                        await FaceService.DeletePersonGroupAsync(SelectedPersonGroup.PersonGroupId);
                                                                        await MessageDialogHelper.MessageDialogAsync("Deleting Group", $"'{SelectedPersonGroup.Name}' successfully deleted.");
                                                                    },
                                                                async () => await MessageDialogHelper.MessageDialogAsync("Deleting Group", "Deleting was canceled by user."));

                    //Cleanup
                    SelectedPersonGroup = null;
                    await LoadGroupsAsync();
                }
            }
        }

        private bool CanExecuteDeleteCommandAsync()
        {
            return SelectedPersonGroup != null || SelectedPerson != null;
        }
        #endregion //DeleteCommand

        #region OnChangedEvents

        public async void OnPersonGroupChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (SelectedPersonGroup != null)
                {
                    await LoadPersonsOfGroupAsync(SelectedPersonGroup.PersonGroupId);
                }
                else
                {
                    //DOTO Handle this case
                    //await MessageDialogHelper.MessageDialogAsync("Didn't select a group, please try again later.");
                }
            }
            catch (Exception ex)
            {
                await MessageDialogHelper.MessageDialogAsync("Exception", ex.Message);
            }
        }

        public async void OnPersonChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (SelectedPerson != null)
                    CountOfFaces = $"{SelectedPerson.PersistedFaceIds.Count()} trained faces.";
                else
                    CountOfFaces = "Please select a person.";
            }
            catch (Exception ex)
            {
                await MessageDialogHelper.MessageDialogAsync("Exception", ex.Message);
            }
        }

        #endregion //OnChangedEvents

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                if (FaceService == null)
                    FaceService = await FaceServiceHelper.CreateNewFaceServiceAsync();

                await LoadGroupsAsync();

                IsLoading = false;
            }
            catch (FaceAPIException ex)//Handle API-Exception
            {
                await MessageDialogHelper.MessageDialogAsync(ex.ErrorMessage);
            }
            catch (Exception ex)
            {
                await MessageDialogHelper.MessageDialogAsync(ex.Message);
            }
        }

        /// <summary>
        /// Loads groups and cleanup the ObservableCollection
        /// </summary>
        /// <returns></returns>
        private async Task LoadGroupsAsync()
        {
            PersonGroups.Clear();
            var fscPersonGroups = await FaceService.ListPersonGroupsAsync();
            fscPersonGroups.OrderBy(pg => pg.Name).ForEach(pg => PersonGroups.Add(pg));
        }

        /// <summary>
        /// Loads persons and cleanup ObservableCollection
        /// </summary>
        /// <returns></returns>
        private async Task LoadPersonsOfGroupAsync(string groupID)
        {
            Persons.Clear();

            var persons = await FaceService.ListPersonsAsync(groupID);
            persons.OrderBy(p => p.Name).ForEach(p => Persons.Add(p));
        }
    }
}
