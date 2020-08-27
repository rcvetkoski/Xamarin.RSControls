using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
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


        private void Button_Clicked1(object sender, EventArgs e)
        {
            Editor entry = new Editor();
            entry.Placeholder = "Forms View";
            entry.AutoSize = EditorAutoSizeOption.TextChanges;
            //entry.HorizontalOptions = LayoutOptions.FillAndExpand;
            //entry.BackgroundColor = Color.Yellow;
            //entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";

            ListView listView = new ListView();
            listView.HeightRequest = 150;
            listView.HorizontalOptions = LayoutOptions.Center;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message Trolewoflwefeowl j2f2fk 2n2jfn \r\n j2k qwdwqdqwwqd");
            //rsPopup.SetCustomView(entry);
            rsPopup.SetDimAmount(0.6f);
            rsPopup.SetIsModal(false);
            rsPopup.HasCloseButton = true;
            rsPopup.SetMargin(0, 0, 0, 0);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Left);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);

            rsPopup.Show();

        }

        private void Button_Clicked2(object sender, EventArgs e)
        {
            //(this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(1);


            RSEditor rSEditor = new RSEditor() {Placeholder = "Enter some text", AutoSize= EditorAutoSizeOption.TextChanges};
            rSEditor.AutoSize = EditorAutoSizeOption.TextChanges;
            rSEditor.Helper = "Helper !";
            rSEditor.SetBinding(RSEditor.TextProperty, new Binding("Lolo", BindingMode.TwoWay) { Source = this.BindingContext });
            rSEditor.TextChanged += Entry_TextChanged;
            RSEntry entry2 = new RSEntry() { Placeholder = "Enter some text", Helper = "Helper !"};
            Grid stack = new Grid() { BackgroundColor = Color.Cyan};
            stack.Children.Add(rSEditor, 0, 0);
            stack.Children.Add(entry2, 1, 0);
            stack.Padding = new Thickness(30);

            RSEntry rSEntry = new RSEntry();
            rSEntry.Text = "sdiufhesfeofèeifwehweofhewofhoewf";
            //rSEntry.SetBinding(RSEntry.TextProperty, new Binding("Lolo", BindingMode.TwoWay) { Source = this.BindingContext });
            rSEntry.HorizontalOptions = LayoutOptions.Start;

            Label label = new Label();
            label.Text = "Troloeole Mehotn";

            Entry entry = new Entry(); 
            entry.Text = "Custom entry rreregregergregreergregregrereg";

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message");
            //rsPopup.SetCustomView(entry);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            //rsPopup.HasCloseButton = true;
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Right);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);



            rsPopup.Show();

        }


        private void Button_Clicked3(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";


            RSPopup rsPopup = new RSPopup("", "Message Tewowl ewolo werew we weg ewweg egw weg ewg wegwe gew g gwe gewg  weg");
            //rsPopup.SetCustomView(entry); 
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Left);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked4(object sender, EventArgs e)
        {
            //Entry entry = new Entry();
            //entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";

            //RSEditor rSEditor = new RSEditor() { Placeholder = "Enter some text", AutoSize = EditorAutoSizeOption.TextChanges };

            RSPopup rsPopup = new RSPopup("RSPopup Test wefwef w Test !", "Message");
            //rsPopup.SetCustomView(rSEditor);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.MatchParent, 150);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Over);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction(" Remove ", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked5(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";
            entry.HorizontalOptions = LayoutOptions.FillAndExpand;

            ListView listView = new ListView();
            listView.HeightRequest = 150;
            listView.HorizontalOptions = LayoutOptions.Center;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message wef fw fewfwe wef wef ew ewfwe wef ewfewf ewfewwefwe fewf wef ewf ewf wef wef ewf we");
            rsPopup.SetCustomView(listView);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Top);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked6(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";
            entry.HorizontalOptions = LayoutOptions.FillAndExpand;

            ListView listView = new ListView();
            listView.HeightRequest = 150;
            listView.HorizontalOptions = LayoutOptions.Center;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("", "");
            rsPopup.SetCustomView(listView);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Bottom);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.BindingContext as MainPageViewModel).RSCommand.ChangeCanExecute();
        }
    }
}
