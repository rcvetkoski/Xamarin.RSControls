using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;

namespace Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
            this.BindingContext = new MainPageViewModel();

            //for (int i = 0; i < 1; i++)
            //{
            //    RSNumericEntryInputLayout rSNumericEntryInputLayout = new RSNumericEntryInputLayout() { CustomUnit = "KG", Value = "i", Placeholder = i.ToString() };

            //    stack.Children.Add(rSNumericEntryInputLayout);
            //}
        }


        private void Button_Clicked(object sender, EventArgs e)
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
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Bottom);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction(" Remove ", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }
    }
}