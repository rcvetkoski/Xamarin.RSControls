using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSDatePickerInputLayout), typeof(RSDatePickerInputLayoutRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSDatePickerInputLayoutRenderer : Forms.Platform.Android.AppCompat.ViewRenderer<RSDatePickerInputLayout, TextInputLayout>
    {
        private TextInputLayout textInputLayout;
        private RSDatePickerRenderer renderer;

        public RSDatePickerInputLayoutRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSDatePickerInputLayout> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            renderer = new RSDatePickerRenderer(Context);
            renderer.SetElement(Element);
            renderer.Control.RemoveFromParent();
            renderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            int paddingLeftRight = Extensions.ViewExtensions.IntToDip(10, Context);
            renderer.Control.SetPadding(paddingLeftRight, renderer.Control.PaddingTop, paddingLeftRight, renderer.Control.PaddingBottom);
            renderer.SetIsTextInputLayout(true); //Avoid having double warning message in edittext when error enabled in simple control

            textInputLayout.AddView(renderer.Control);

            //Hint
            textInputLayout.Hint = renderer.Element.Placeholder;

            //Error-Validation
            if (this.Element.Behaviors.Any(x => x is Validators.ValidationBehaviour))
                textInputLayout.ErrorEnabled = true;


            SetNativeControl(textInputLayout);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
                textInputLayout.Error = this.Element.Error;
        }

        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            //Force onConfigChange for datepicker so it will set the good orientation for the layout
            renderer.DispatchConfigurationChanged(newConfig);
        }
    }
}