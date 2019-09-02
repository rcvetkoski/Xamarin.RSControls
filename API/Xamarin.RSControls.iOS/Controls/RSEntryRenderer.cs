using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            RSEntry entry = (RSEntry)Element;
            //if (!entry.HasBorder)
            //{
            //    Control.BorderStyle = UITextBorderStyle.None;
            //    //Control.Layer.CornerRadius = 10;
            //    Control.TextColor = UIColor.White;
            //}
        }

        //Fix for bug ios not loading dll
        public static void Initialize()
        {

        }
    }
}