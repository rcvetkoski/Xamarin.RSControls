using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSEnumPickerInputLayout<>), typeof(RSEnumPickerInputLayoutRenderer))]
[assembly: ExportRenderer(typeof(RSPickerInputLayout), typeof(RSPickerInputLayoutRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPickerInputLayoutRenderer : Forms.Platform.Android.AppCompat.ViewRenderer<RSPickerInputLayout, TextInputLayout>
    {
        private TextInputLayout textInputLayout;
        private RSPickerRenderer renderer;


        public RSPickerInputLayoutRenderer(Context context) : base(context)
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<RSPickerInputLayout> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            renderer = new RSPickerRenderer(Context);
            renderer.SetElement(Element);
            renderer.Control.RemoveFromParent();
            renderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            int paddingLeftRight = Extensions.ViewExtensions.IntToDip(10, Context);
            renderer.Control.SetPadding(paddingLeftRight, renderer.Control.PaddingTop, paddingLeftRight, renderer.Control.PaddingBottom);
            renderer.SetIsTextInputLayout(true); //Avoid having double warning message in edittext when error enabled in simple control

            textInputLayout.AddView(renderer.Control);


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
    }


    public class RSEnumPickerInputLayoutRenderer : Forms.Platform.Android.AppCompat.ViewRenderer<RSPickerBase, TextInputLayout>
    {
        private TextInputLayout textInputLayout;
        private RSPickerRenderer renderer;


        public RSEnumPickerInputLayoutRenderer(Context context) : base(context)
        {

        }


        protected override void OnElementChanged(ElementChangedEventArgs<RSPickerBase> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;

            renderer = new RSPickerRenderer(Context);
            renderer.SetElement(Element as RSPickerBase);
            renderer.Control.RemoveFromParent();
            renderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            int paddingLeftRight = Extensions.ViewExtensions.IntToDip(10, Context);
            renderer.Control.SetPadding(paddingLeftRight, renderer.Control.PaddingTop, paddingLeftRight, renderer.Control.PaddingBottom);
            renderer.SetIsTextInputLayout(true); //Avoid having double warning message in edittext when error enabled in simple control

            textInputLayout.AddView(renderer.Control);


            //Error-Validation
            if ((this.Element as RSPickerBase).Behaviors.Any(x => x is Validators.ValidationBehaviour))
                textInputLayout.ErrorEnabled = true;


            SetNativeControl(textInputLayout);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
                textInputLayout.Error = (this.Element as RSPickerBase).Error;
        }
    }
}