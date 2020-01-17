using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Interfaces;

[assembly: ExportRenderer(typeof(RSNumericUpDown), typeof(RSNumericUpDownRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSNumericUpDownRenderer : RSNumericEntryRenderer
    {
        public RSNumericUpDownRenderer(Context context) : base(context)
        {
        }

        protected override FormsEditText CreateNativeControl()
        {
            if ((this.Element as IRSControl).RightIcon == null)
            {
                (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon() 
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/plus.svg" },
                    Command = "Increase",
                    Source = this.Element
                };
            }

            if ((this.Element as IRSControl).RightIcon != null && (this.Element as IRSControl).RightHelpingIcon == null)
                (this.Element as IRSControl).RightHelpingIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/minus.svg" },
                    Command = "Decrease",
                    Source = this.Element
                };

            return new CustomEditText(Context, this.Element as IRSControl);
        }
    }
}