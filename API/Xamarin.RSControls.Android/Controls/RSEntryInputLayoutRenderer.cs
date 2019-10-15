using System.ComponentModel;
using System.Linq;
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
        TextInputLayout textInputLayout;

        public RSEntryInputLayoutRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSEntryInputLayout> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            RSNumericEntryRenderer renderer = new RSNumericEntryRenderer(Context);
            renderer.SetElement(Element);
            renderer.Control.RemoveFromParent();
            int paddingLeftRight = Extensions.ViewExtensions.IntToDip(10, Context);

            renderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            renderer.Control.SetPadding(paddingLeftRight, renderer.Control.PaddingTop, paddingLeftRight, renderer.Control.PaddingBottom);
            renderer.SetIsTextInputLayout(true); //Avoid having double warning message in edittext when error enabled in simple control
            textInputLayout.AddView(renderer.Control);

            //Hint
            textInputLayout.Hint = renderer.Element.Placeholder;

            if(this.Element.CounterMaxLength != -1)
            {
                textInputLayout.CounterEnabled = true;
                textInputLayout.CounterMaxLength = 7;
            }

            if(!string.IsNullOrEmpty(this.Element.Helper))
            {
                textInputLayout.HelperText = this.Element.Helper;
                textInputLayout.HelperTextEnabled = true;
            }

            //textInputLayout.BoxBackgroundColor = Color.Yellow.ToAndroid();
            //textInputLayout.BoxStrokeColor = Color.Pink.ToAndroid();
            //textInputLayout.SetBoxCornerRadii(50, 50, 50, 50);

            //Error-Validation
            if (this.Element.Behaviors.Any(x => x is Validators.ValidationBehaviour))
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