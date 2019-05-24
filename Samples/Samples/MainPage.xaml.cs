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
            //    RSNumericEntryInputLayout rSNumericEntryInputLayout = new RSNumericEntryInputLayout() { CustomUnit = "KG", Value = "i", Placeholder = i.ToString() };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}

            //for (int i = 0; i < 1000; i++)
            //{
            //    Entry entry = new Entry() { Text = "i", Placeholder = i.ToString() };

            //    stack.Children.Add(entry);
            //}
        }
    }
}
