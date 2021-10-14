using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSNumericUpDown), typeof(RSNumericUpDownRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSNumericUpDownRenderer : RSNumericEntryRenderer
    {
        public RSNumericUpDownRenderer()
        {
            
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Value" && !(sender as Forms.View).IsFocused)
                (this.Control as RSUITextField).UpdateView();
        }

        protected override UITextField CreateNativeControl()
        {
            if((this.Element as RSNumericUpDown).RSNumericUpDownStyle == Enums.RSNumericUpDownStyleEnum.Right)
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
                {
                    (this.Element as IRSControl).RightHelpingIcon = new Helpers.RSEntryIcon()
                    {
                        View = new RSSvgImage() { Source = "Samples/Data/SVG/minus.svg" },
                        Command = "Decrease",
                        Source = this.Element
                    };
                }
            }
            else if((this.Element as RSNumericUpDown).RSNumericUpDownStyle == Enums.RSNumericUpDownStyleEnum.Split)
            {
                (this.Element as IRSControl).LeftIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/minus.svg" },
                    Command = "Decrease",
                    Source = this.Element
                };

                (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/plus.svg" },
                    Command = "Increase",
                    Source = this.Element
                };
            }
            else
            {
                (this.Element as IRSControl).LeftIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/minus.svg" },
                    Command = "Decrease",
                    Source = this.Element
                };

                (this.Element as IRSControl).LeftHelpingIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/plus.svg" },
                    Command = "Increase",
                    Source = this.Element
                };
            }


            return new RSUITextField(this.Element as IRSControl) { };
        }
    }
}
