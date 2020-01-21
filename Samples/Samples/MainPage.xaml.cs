using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;

namespace Samples
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainPageViewModel();

            //for (int i = 0; i < 1000; i++)
            //{
            //    RSDatePickerInputLayout rSNumericEntryInputLayout = new RSDatePickerInputLayout() { NullableDate = DateTime.Now, Placeholder = "Date" + i.ToString() };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}

            //for (int i = 0; i < 1000; i++)
            //{
            //    RSDatePicker rSNumericEntryInputLayout = new RSDatePicker() { NullableDate = DateTime.Now, Placeholder = "Date" + i.ToString(), HasBorder = true };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}

            //for (int i = 0; i < 1000; i++)
            //{
            //    DatePicker rSNumericEntryInputLayout = new DatePicker() { Date = DateTime.Now };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}






            //for (int i = 0; i < 1000; i++)
            //{
            //    Entry rSNumericEntryInputLayout = new Entry() { Text = "lol" };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}

            //for (int i = 0; i < 1000; i++)
            //{
            //    RSEntry rSNumericEntryInputLayout = new RSEntry() { Text = "lol" };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}


            //for (int i = 0; i < 1000; i++)
            //{
            //    RSEntry rSNumericEntryInputLayout = new RSEntry() { Placeholder = "Test", Error = "Error", RSEntryStyle = Xamarin.RSControls.Enums.RSEntryStyleSelectionEnum.OutlinedBorder };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}

            //for (int i = 0; i < 1000; i++)
            //{
            //    RSEntryInputLayout rSNumericEntryInputLayout = new RSEntryInputLayout() { Placeholder = "Test", Error = "Error" };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Page1());
        }

        private void Button_Clicked2(object sender, EventArgs e)
        {
            //(this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(1);
        }
    }
}
