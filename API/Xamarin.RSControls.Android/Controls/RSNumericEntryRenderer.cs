using Android.Content;
using Xamarin.Forms;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Interfaces;
using Android.Text.Method;

[assembly: ExportRenderer(typeof(RSNumericEntry), typeof(RSNumericEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSNumericEntryRenderer : RSEntryRenderer
    {
        public RSNumericEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                this.Control.KeyListener = DigitsKeyListener.GetInstance(true, true); // I know this is deprecated, but haven't had time to test the code without this line, I assume it will work without
                this.Control.InputType = global::Android.Text.InputTypes.ClassNumber | global::Android.Text.InputTypes.NumberFlagDecimal;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}