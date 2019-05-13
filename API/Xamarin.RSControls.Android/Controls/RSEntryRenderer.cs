using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Android.Controls;
using Xamarin.RSControls.Controls;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.Android.Controls
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
            nativeEditText.SetSingleLine(true);
        }
    }
}