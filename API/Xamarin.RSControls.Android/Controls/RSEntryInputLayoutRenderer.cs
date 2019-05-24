using System.ComponentModel;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSEntryInputLayout), typeof(RSEntryInputLayoutRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEntryInputLayoutRenderer : ViewRenderer<RSEntryInputLayout, TextInputLayout>
    {
        public RSEntryInputLayoutRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSEntryInputLayout> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                return;
        }
    }
}