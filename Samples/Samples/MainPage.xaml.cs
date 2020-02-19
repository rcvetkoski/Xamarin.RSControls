using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Interfaces;

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

            RSPopup rsPopup = new RSPopup("RSPopup !", "RSMessage");
            rsPopup.SetBackgroundColor(Color.Cyan);
            rsPopup.SetPopupPosition(sender as View);
            rsPopup.SetPopupPosition((float)(sender as View).X, (float)(sender as View).Bounds.Location.Y);

            var grid = new Grid() { BackgroundColor = Color.Pink, HeightRequest = 100 };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var topLeft = new Label { Text = "Top Left" };
            var topRight = new Label { Text = "Top Right" };
            var bottomLeft = new Label { Text = "Bottom Left" };
            var bottomRight = new Label { Text = "Bottom Right" };

            grid.Children.Add(topLeft, 0, 0);
            grid.Children.Add(topRight, 1, 0);
            grid.Children.Add(bottomLeft, 0, 1);
            grid.Children.Add(bottomRight, 1, 1);

            //rsPopup.SetCustomView(new Label() { Text = "label trol", BackgroundColor = Color.Green});

            rsPopup.SetCustomView(grid);

            rsPopup.Show();
        }
    }
}
