using System.Linq;
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

        protected override UITextField CreateNativeControl()
        {
            return new RSUITextField(
                (this.Element as RSEntry).Placeholder,
                (this.Element as RSEntry).Helper,
                (this.Element as RSEntry).CounterMaxLength,
                (this.Element as RSEntry).IsPassword,
                (this.Element as RSEntry).RSEntryStyle,
                1, Color.Gray.ToUIColor(),
                this.Element.BackgroundColor.ToUIColor(),
                this.Element.Behaviors.Any());
        }

        //Fix for bug ios not loading dll
        public static void Initialize()
        {

        }
    }
}