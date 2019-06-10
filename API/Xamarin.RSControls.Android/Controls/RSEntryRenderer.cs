using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        private bool isTextInputLayout;

        public RSEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error" && !isTextInputLayout)
                this.Control.Error = (this.Element as RSEntry).Error;

            //Draw border or not
            if ((this.Element as RSEntry).HasBorder)
                Extensions.ViewExtensions.DrawBorder(this.Control, Context, global::Android.Graphics.Color.Black);
        }

        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
        }
    }
}