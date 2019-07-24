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

[assembly: ExportRenderer(typeof(RSSearchViewInputLayout), typeof(RSSearchViewInputLayoutRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSSearchViewInputLayoutRenderer : ViewRenderer<RSSearchViewInputLayout, TextInputLayout>
    {
        TextInputLayout textInputLayout;

        public RSSearchViewInputLayoutRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSSearchViewInputLayout> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                return;

            textInputLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.RSTextInputLayout, null) as TextInputLayout;


            RSSearchViewRenderer rSSearchViewRenderer = new RSSearchViewRenderer(Context);
            rSSearchViewRenderer.SetElement(Element);
            rSSearchViewRenderer.Control.RemoveFromParent();
            int paddingLeftRight = Extensions.ViewExtensions.IntToDip(10, Context);

            rSSearchViewRenderer.Control.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            rSSearchViewRenderer.Control.SetPadding(paddingLeftRight, rSSearchViewRenderer.Control.PaddingTop, paddingLeftRight, rSSearchViewRenderer.Control.PaddingBottom);
            //rSSearchViewRenderer.SetIsTextInputLayout(true); //Avoid having double warning message in edittext when error enabled in simple control

            textInputLayout.AddView(rSSearchViewRenderer.Control);

            //Hint
            textInputLayout.Hint = rSSearchViewRenderer.Element.Placeholder;

            //Error-Validation
            if (this.Element.Behaviors.Any(x => x is Validators.ValidationBehaviour))
                textInputLayout.ErrorEnabled = true;


            SetNativeControl(textInputLayout);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //if (e.PropertyName == "Error")
            //    textInputLayout.Error = this.Element.Error;
        }
    }
}