using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Samples
{
    public partial class TestContentVIew : ContentView
    {
        public TestContentVIew()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainTabbedPage());
        }
    }
}
