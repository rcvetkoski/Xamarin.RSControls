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
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Page1());
        }

        private void Button_Clicked2(object sender, EventArgs e)
        {
            //(this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(1);

            RSPopup rsPopup = new RSPopup("RSPopup !", "RSMessage");
            rsPopup.SetPopupPosition(sender as View);
            RSEntry entry = new RSEntry() {Placeholder = "Enter some text", HorizontalOptions = LayoutOptions.FillAndExpand};
            entry.SetBinding(Entry.TextProperty, new Binding("Lolo", BindingMode.TwoWay) { Source = this.BindingContext });
            entry.TextChanged += Entry_TextChanged;

            StackLayout stackLayout = new StackLayout();
            stackLayout.Children.Add(entry);

            rsPopup.SetCustomView(stackLayout);
            rsPopup.SetDimAmount(0f);
            rsPopup.Show();
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.BindingContext as MainPageViewModel).RSCommand.ChangeCanExecute();
        }
    }
}
