using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Samples.ViewModels
{
    public class RSPickerViewModel : BaseViewModel
    {
        public RSPickerViewModel()
        {
            Persons = new List<Person>()
            { 
                new Person() { Name = "Paris" }, new Person() { Name = "Azarenka" }, new Person() { Name = "Jake" }
                , new Person() , new Person() { Name = "Pierre" }, new Person() { Name = "Rade" }, new Person() { Name = "Stefan" }
                , new Person() { Name = "John" }, new Person() { Name = "Pero" }, new Person() { Name = "Nathan" }, new Person() { Name = "Cartamn" }
                , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
                , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
                , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
                , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
                , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
            };

            SelectedPerson = Persons.FirstOrDefault(x => x.Name == "Jake");
            SelectedPersons = new ObservableCollection<Person>();
        }

        public List<Person> Persons { get; set; }

        private Person selectedPerson;
        public Person SelectedPerson
        {
            get
            {
                return selectedPerson;
            }
            set
            {
                selectedPerson = value;
                OnPropertyChanged("SelectedPerson");
            }
        }


        private ObservableCollection<Person> selectedPersons;
        public ObservableCollection<Person> SelectedPersons
        {
            get
            {
                return selectedPersons;
            }
            set
            {
                selectedPersons = value;
                OnPropertyChanged("SelectedPersons");
            }
        }
    }
}
