using Samples.ViewModels;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;

namespace Samples
{
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            //Entry entry = new Entry();
            //entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";

            //RSEditor rSEditor = new RSEditor() { Placeholder = "Enter some text", AutoSize = EditorAutoSizeOption.TextChanges };

            RSPopup rsPopup = new RSPopup("RSPopup Test wefwef w Test !", "Message");
            //rsPopup.SetCustomView(rSEditor);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.MatchParent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Bottom);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction(" Remove ", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainViewModel).RSCommand, (this.BindingContext as MainViewModel).Lolo);


            rsPopup.Show();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        ~Page1()
        {
        }
    }
}
