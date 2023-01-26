using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Samples.ViewModels
{
    public class RSSVGImageViewModel : BaseViewModel
    {
        public RSSVGImageViewModel()
        {
            TapCommand = new Command(Tap);
        }

        bool areVisible;
        public bool AreVisible
        {
            get
            {
                return areVisible;  
            }
            set
            {
                if(areVisible != value)
                {
                    areVisible = value;
                    OnPropertyChanged(nameof(AreVisible));
                }
            }
        }

        public ICommand TapCommand { get; set; }
        private void Tap()
        {
            AreVisible = AreVisible == true ? AreVisible = false : true;
        }
    }
}
