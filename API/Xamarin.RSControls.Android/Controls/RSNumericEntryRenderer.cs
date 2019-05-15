using Android.Content;
using Xamarin.Forms;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;

[assembly: ExportRenderer(typeof(RSNumericEntry), typeof(RSNumericEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSNumericEntryRenderer : RSEntryRenderer
    {
        public RSNumericEntryRenderer(Context context) : base(context)
        {
        }
    }
}