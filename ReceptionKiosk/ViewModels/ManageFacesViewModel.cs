using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

using ReceptionKiosk.Helpers;

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;

namespace ReceptionKiosk.ViewModels
{
    public class ManageFacesViewModel : Observable
    {
        //TODO: Abändern
        private FaceServiceClient fsc = new FaceServiceClient("XXXXXXXXXXXXXXXX", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

        /// <summary>
        /// Liste aller verfügbaren Gruppen
        /// </summary>
        public ObservableCollection<PersonGroup> PersonGroups { get; private set; }

        /// <summary>
        /// Alle Personen einer Gruppe
        /// </summary>
        public ObservableCollection<Person> Persons { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        public ICommand LoadPersonsCommand { get; private set; }

        /// <summary>
        /// CTOR
        /// </summary>
        public ManageFacesViewModel()
        {
            PersonGroups = new ObservableCollection<PersonGroup>();
            LoadPersonsCommand = new RelayCommand(ExecuteLoadPersons, CanExecuteLoadPersons);

            Initialize();
        }

        private void ExecuteLoadPersons()
        {
            throw new NotImplementedException();
        }

        private bool CanExecuteLoadPersons()
        {
            throw new NotImplementedException();
        }

        public async void Initialize()
        {
            IsLoading = true;
            
            var personGroups = await fsc.ListPersonGroupsAsync();
            personGroups.OrderBy(pg => pg.Name);
            personGroups.ForEach(pg => PersonGroups.Add(pg));

            IsLoading = false;
        }
    }
}
