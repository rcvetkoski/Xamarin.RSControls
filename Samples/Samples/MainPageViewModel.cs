using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;

namespace Samples
{
    public class MainPageViewModel : INotifyPropertyChanged
    {

        public Command RSCommand { get; set; }


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

            SelectedPerson = listPicker.FirstOrDefault();

            RSCommand = new Command(RSCommandMethod, CanExec);
        }

        public void RSCommandMethod(object obj)
        {
            RSCommand.ChangeCanExecute();
        }

        public bool CanExec(object obj)
        {
            if (Lolo == "Troll")
                return true;
            else
                return false;
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

        private string lol;
        public string Lolo
        {
            get
            {
                return lol;
            }
            set
            {
                lol = value;
                OnPropertyChanged("Lolo");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void TestMethod(object obj, string str)
        {
            //RSPopup rSPopup = new RSPopup("Title", "Message rthtrhttrhtrhtr hrthrt rthrt rth rth rgreggregreregergergregregergreg ergerg ergergre ergerg erg ");
            //rSPopup.SetPopupSize(Xamarin.RSControls.Enums.RSPopupSizeEnum.WrapContent, Xamarin.RSControls.Enums.RSPopupSizeEnum.WrapContent);
            //rSPopup.SetPopupPositionRelativeTo(this.rSControl as Xamarin.Forms.View, Xamarin.RSControls.Enums.RSPopupPositionSideEnum.Top);
            //rSPopup.SetDimAmount(0f);
            //rSPopup.SetMargin(40, 0, 40, 0);
            //rSPopup.Show();
        }
        public void TestMethod2(object obj, object obj2)
        {
            RSPopup rSPopup = new RSPopup("Title", "Message");
            rSPopup.SetPopupSize(Xamarin.RSControls.Enums.RSPopupSizeEnum.WrapContent, Xamarin.RSControls.Enums.RSPopupSizeEnum.WrapContent);
            rSPopup.SetPopupPositionRelativeTo(obj2 as Xamarin.Forms.View, Xamarin.RSControls.Enums.RSPopupPositionSideEnum.Top);
            rSPopup.SetDimAmount(0f);
            rSPopup.SetMargin(40, 0, 40, 0);
            rSPopup.Show();
        }

    }


    public class Person : INotifyPropertyChanged
    {
        private string name { get; set; }
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
