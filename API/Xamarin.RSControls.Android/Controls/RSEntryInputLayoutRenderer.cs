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
        private RSFormsEditText nativeEditText;

        public RSEntryInputLayoutRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSEntryInputLayout> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                return;

            TextInputLayout inputLayout = CreateNativeControl();
            inputLayout.Hint = Element.Placeholder;


            SetNativeControl(inputLayout);
        }

        protected override TextInputLayout CreateNativeControl()
        {            
            var textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            nativeEditText = new RSFormsEditText(Context, Element);
            textInputLayout.AddView(nativeEditText);

            //nativeEditText.SetRawInputType(InputTypes.TextVariationPassword);
            //nativeEditText.TransformationMethod = PasswordTransformationMethod.Instance;

            return textInputLayout;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            nativeEditText.OnElementPropertyChanged(e.PropertyName);
        }
    }
}