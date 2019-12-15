using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.RSControls.Enums;

namespace Samples
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            NumericEntryValue = 2657.239;

            listPicker = new List<Person>() { new Person() { Name = "Paris" }, new Person() { Name = "Azarenka" }, new Person() { Name = "Jake" }
            , new Person() { Name = "Natasha" }, new Person() { Name = "Pierre" }, new Person() { Name = "Rade" }, new Person() { Name = "Stefan" }
            , new Person() { Name = "John" }, new Person() { Name = "Pero" }, new Person() { Name = "Nathan" }, new Person() { Name = "Cartamn" }
            , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
            , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
            , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
            , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }
            , new Person() { Name = "Emmanuel" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }, new Person() { Name = "Jake" }};
            selectedPerson = listPicker.FirstOrDefault();
            SelectedPersons = new ObservableCollection<Person>();
            SelectedPersons.Add(listPicker.FirstOrDefault());

            var list = new List<DateSelectionModeEnum>() { DateSelectionModeEnum.Default, DateSelectionModeEnum.Month, DateSelectionModeEnum.MonthYear, DateSelectionModeEnum.Year };


            obsCollectionPicker = new ObservableCollection<string>() { "Tom", "Jerry", "Buldog" };
            ObsCollectionPickerSelectedItems = new ObservableCollection<string>();

            listPickerEnum = list.OrderBy(x => x);
            ListPickerEnumSelectedItems = new List<DateSelectionModeEnum>();

            selectedDateMode = DateSelectionModeEnum.Month;


            SomeDate = new DateTime(2011, 1, 1);
            MaxDate = new DateTime(2011, 6, 1);
            MinDate = new DateTime(2009, 7, 1);

            Lolo = "lolo";
        }

        private double numericEntryValue;
        public double NumericEntryValue
        {
            get
            {
                return numericEntryValue;
            }
            set
            {
                numericEntryValue = value;
                OnPropertyChanged("NumericEntryValue");
            }
        }

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

        private List<Person> listPicker;
        public List<Person> ListPicker
        {
            get
            {
                return listPicker;
            }
            set
            {
                listPicker = value;
                OnPropertyChanged("ListPicker");
            }
        }


        private IEnumerable<DateSelectionModeEnum> listPickerEnum;
        public IEnumerable<DateSelectionModeEnum> ListPickerEnum
        {
            get
            {
                return listPickerEnum;
            }
            set
            {
                listPickerEnum = value;
                OnPropertyChanged("ListPickerEnum");
            }
        }
        private List<DateSelectionModeEnum> listPickerEnumSelectedItems;
        public List<DateSelectionModeEnum> ListPickerEnumSelectedItems
        {
            get
            {
                return listPickerEnumSelectedItems;
            }
            set
            {
                listPickerEnumSelectedItems = value;
                OnPropertyChanged("ListPickerEnumSelectedItems");
            }
        }

        private DateSelectionModeEnum selectedDateMode;
        public DateSelectionModeEnum SelectedDateMode
        {
            get
            {
                return selectedDateMode;
            }
            set
            {
                selectedDateMode = value;
                OnPropertyChanged("SelectedDateMode");
            }
        }



        private ObservableCollection<string> obsCollectionPicker;
        public ObservableCollection<string> ObsCollectionPicker
        {
            get
            {
                return obsCollectionPicker;
            }
            set
            {
                obsCollectionPicker = value;
                OnPropertyChanged("ObsCollectionPicker");
            }
        }
        private ObservableCollection<string> obsCollectionPickerSelectedItems;
        public ObservableCollection<string> ObsCollectionPickerSelectedItems
        {
            get
            {
                return obsCollectionPickerSelectedItems;
            }
            set
            {
                obsCollectionPickerSelectedItems = value;
                OnPropertyChanged("ObsCollectionPickerSelectedItems");
            }
        }


        private DateTime? someDate;
        public DateTime? SomeDate
        {
            get
            {
                return someDate;
            }
            set
            {
                someDate = value;
                OnPropertyChanged("SomeDate");
            }
        }

        public DateTime MaxDate { get; set; }
        public DateTime MinDate { get; set; }


        public string Lolo { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void TestMethod(string obj)
        {

        }

    }


    public class Person
    {
        public string Name { get; set; }
    }
}
