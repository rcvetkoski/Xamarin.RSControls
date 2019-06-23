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

            listPicker = new List<Person>() { new Person() { Name = "Paris" }, new Person() { Name = "Azarenka" }, new Person() { Name = "Jake" } };

            selectedPerson = listPicker.FirstOrDefault();

            var list = new List<DateSelectionModeEnum>() { DateSelectionModeEnum.Default, DateSelectionModeEnum.Month, DateSelectionModeEnum.MonthYear, DateSelectionModeEnum.Year};


            obsCollectionPicker = new ObservableCollection<string>() { "Tom", "Jerry", "Buldog" };

            listPickerEnum = list.OrderBy(x => x);

            selectedDateMode = DateSelectionModeEnum.Month;

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


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Person
    {
        public string Name { get; set; }
    }
}
