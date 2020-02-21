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

            RSPopup rsPopup = new RSPopup("RSPopup !", "RSMessageewhgjfhwfgwjhwwjgwghwrghwrwrjf" +
                "wjfhewkjfhewjfhewfhewjfhewfkewhfjewhfjkewhfkjewhfkjewkew" +
                "weflewflkewfhlkewfehwfjehwjfkehwkjfewkewljwgwrkhgrwqéhkg" +
                "wgjgqkghjkreghkrjgkrwgkjrwhgkrwjhgrgjkrhgwrkghwrgwrghwr" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "ewfewfeeewfefewfwewfewfewf" +
                "qefewfewfewfewwefewfew" +
                "wefewfewfewewfewfewewfw" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "" +
                "" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "wlgknwrgnw jkngwrngjh  kehrégkéwreghwkreghkjwrghkérgrwhgjkhghwrégkwrj" +
                "Rade");
            //rsPopup.SetPopupPosition(sender as View);
            //rsPopup.SetPopupPosition((float)(sender as View).X, (float)(sender as View).Bounds.Location.Y);
            rsPopup.SetDimAmount(0.5f);
            rsPopup.Show();
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);

        }
    }
}
