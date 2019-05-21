using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        public RSEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            var nativeEditText = (global::Android.Widget.EditText)Control;
            nativeEditText.SetSingleLine(false);

            //Set border
            Extensions.ViewExtensions.DrawBorder(nativeEditText, Context, (Element as RSEntry).BorderColor.ToAndroid());
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if(e.PropertyName == "BorderColor")
                Extensions.ViewExtensions.DrawBorder(Control, Context, (Element as RSEntry).BorderColor.ToAndroid());
        }
    }
}