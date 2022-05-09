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
            return new RSUITextField(this.Element as IRSControl);
        }
    }
}
