using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSNumericEntryInputLayout), typeof(RSNumericEntryInputLayoutRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSNumericEntryInputLayoutRenderer : ViewRenderer<RSNumericEntryInputLayout, TextInputLayout>
    {
        TextInputLayout textInputLayout;

        public RSNumericEntryInputLayoutRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSNumericEntryInputLayout> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            RSNumericEntryRenderer renderer = new RSNumericEntryRenderer(Context);
            renderer.SetElement(Element);
            renderer.Control.RemoveFromParent();
            renderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            textInputLayout.AddView(renderer.Control);

            //Hint
            textInputLayout.Hint = renderer.Element.Placeholder;

            //Error-Validation
            if(this.Element.Behaviors.Any(x=> x is Validators.ValidationBehaviour))
                textInputLayout.ErrorEnabled = true;

            //Set field as Password
            if (this.Element.IsPassword)
            {
                Element.IsPassword = true;
                textInputLayout.PasswordVisibilityToggleEnabled = true;
            }

            SetNativeControl(textInputLayout);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
                textInputLayout.Error = this.Element.Error;
        }
    }
}