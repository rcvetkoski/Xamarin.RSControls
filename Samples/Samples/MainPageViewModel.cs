using System.ComponentModel;

namespace Samples
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            NumericEntryValue = 2657.239;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
