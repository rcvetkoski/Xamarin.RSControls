using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Interfaces;

[assembly: ExportRenderer(typeof(RSNumericUpDown), typeof(RSNumericUpDownRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSNumericUpDownRenderer : RSNumericEntryRenderer
    {
        public RSNumericUpDownRenderer(Context context) : base(context)
        {
        }

        protected override FormsEditText CreateNativeControl()
        {
            return new CustomEditText(Context, this.Element as IRSControl);
        }
    }
}