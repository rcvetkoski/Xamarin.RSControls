using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Samples
{
    public partial class TabbedPage1 : ContentPage
    {
        public TabbedPage1()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainPage());
        }
    }
}
