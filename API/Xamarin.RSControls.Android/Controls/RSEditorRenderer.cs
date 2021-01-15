using Android.Content;
using Android.Views;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Interfaces;

[assembly: ExportRenderer(typeof(RSEditor), typeof(RSEditorRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEditorRenderer : EditorRenderer, global::Android.Views.View.IOnTouchListener
    {
        public RSEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            if (this.Element is IRSControl)
                this.Element.Placeholder = "";


            if(Control != null)
            {
                //This fixes scroll problem when inside scrollview
                //this.Control.SetOnTouchListener(this);

                //This fixes xamarin forms bug when next button available in keyboard => cannot pass focus to unfocusable object 
                Control.ImeOptions = global::Android.Views.InputMethods.ImeAction.Done;
                Control.EditorAction += Control_EditorAction;
            }
        }

        private void Control_EditorAction(object sender, global::Android.Widget.TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == global::Android.Views.InputMethods.ImeAction.Done)
            {
                Control.ClearFocus();
            }
            e.Handled = false;

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
                (this.Control as CustomEditText).ErrorMessage = (this.Element as RSEditor).Error;
        }

        protected override FormsEditText CreateNativeControl()
        {
            return new CustomEditText(Context, this.Element as IRSControl);
        }

        public bool OnTouch(global::Android.Views.View v, global::Android.Views.MotionEvent e)
        {
            v.Parent?.RequestDisallowInterceptTouchEvent(true);
            if ((e.Action & global::Android.Views.MotionEventActions.Up) != 0 && (e.ActionMasked & global::Android.Views.MotionEventActions.Up) != 0)
            {
                v.Parent?.RequestDisallowInterceptTouchEvent(false);
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if(Control != null)
                Control.EditorAction -= Control_EditorAction;

            base.Dispose(disposing);
        }
    }
}