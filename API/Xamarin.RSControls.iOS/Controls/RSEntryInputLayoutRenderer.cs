
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSEntryInputLayout), typeof(RSEntryInputLayoutInputLayoutRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSEntryInputLayoutInputLayoutRenderer : EntryRenderer
    {
        protected override UITextField CreateNativeControl()
        {
            return new RSUITextField(
                (this.Element as RSEntryInputLayout).Placeholder,
                (this.Element as RSEntryInputLayout).Helper,
                (this.Element as RSEntryInputLayout).CounterMaxLength,
                (this.Element as RSEntryInputLayout).IsPassword,
                (this.Element as RSEntryInputLayout).RSEntryStyle,
                1, Color.Gray.ToUIColor(),
                this.Element.BackgroundColor.ToUIColor(),
                this.Element.Behaviors.Any());
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);


            
            if (Control == null)
                return;

            


            NSRange range;
            var color = Control.AttributedPlaceholder.GetAttribute("NSColor", 0, out range) as UIColor;
            (Control as RSUITextField).PlaceholderColor = color;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = (this.Element as RSEntryInputLayout).Error;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}