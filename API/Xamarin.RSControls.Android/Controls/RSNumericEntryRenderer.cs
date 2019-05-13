using Android.Content;
using Xamarin.Forms;
using Xamarin.RSControls.Android.Controls;
using Xamarin.RSControls.Controls;

[assembly: ExportRenderer(typeof(RSNumericEntry), typeof(RSNumericEntryRenderer))]
namespace Xamarin.RSControls.Android.Controls
{
    public class RSNumericEntryRenderer : RSEntryRenderer
    {
        public RSNumericEntryRenderer(Context context) : base(context)
        {
        }
    }
}