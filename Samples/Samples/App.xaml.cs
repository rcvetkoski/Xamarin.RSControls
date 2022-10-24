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

            MainPage = new NavigationPage(new MainPage());
            //MainPage = new MainPage();


            //SetTheme();

            //// Respond to the theme change
            //Application.Current.RequestedThemeChanged += (s, a) =>
            //{
            //    if (a.RequestedTheme == OSAppTheme.Light)
            //    {
            //        StatusBarColor.SetStatusBarColor(Color.White, true);
            //    }
            //    else if (a.RequestedTheme == OSAppTheme.Dark)
            //    {
            //        StatusBarColor.SetStatusBarColor(Color.FromHex("#1C1C1E"), false);
            //    }
            //};
        }

        private void SetTheme()
        {
            if (Current.RequestedTheme == OSAppTheme.Unspecified || Current.RequestedTheme == OSAppTheme.Light)
                StatusBarColor.SetStatusBarColor(Color.White, true);
            else
                StatusBarColor.SetStatusBarColor(Color.FromHex("#1C1C1E"), false);
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
