using Samples.Helpers;
using Samples.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Samples
{
    public partial class App : Application
    {
        public static IStatusBarColor StatusBarColor;

        public App()
        {
            InitializeComponent();

            StatusBarColor = DependencyService.Get<IStatusBarColor>();

            MainPage = new NavigationPage(new RSEntryPage());
            //MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
