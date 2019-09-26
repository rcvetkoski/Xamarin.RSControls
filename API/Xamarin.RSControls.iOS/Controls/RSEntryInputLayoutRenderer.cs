
using CoreGraphics;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSEntryInputLayout), typeof(RSEntryInputLayoutInputLayoutRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSEntryInputLayoutInputLayoutRenderer : EntryRenderer
    {
        protected override UITextField CreateNativeControl()
        {
            return new CustomUITextField(this.Element.FontSize);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if(this.Control != null)
            {
                var lol = this.Control;
            }
        }
    }


    public class CustomUITextField : UITextField
    {
        private CoreAnimation.CALayer cALayer;

        public CustomUITextField(double fontSize)
        {
            this.BorderStyle = UITextBorderStyle.None;
            //this.BackgroundColor = UIColor.Cyan;
            CreateErrorField(fontSize);
        }


        private void CreateErrorField(double fontSize)
        {
            UILabel errorLabel = new UILabel()
            {
                Font = UIFont.SystemFontOfSize((float)fontSize / 3),
                TextColor = UIColor.Red,
                Text = "Field cannot be empty !",
                BackgroundColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };


            CoreAnimation.CATextLayer cATextLayer = new CoreAnimation.CATextLayer();
            cATextLayer.String = "Field cannot be empty !";
            cATextLayer.ForegroundColor = UIColor.Red.CGColor;
            cATextLayer.FontSize = 10;
            cATextLayer.Frame = this.Bounds;
            cATextLayer.ContentsScale = UIScreen.MainScreen.Scale;
            UIView underline = new UIView();
            underline.BackgroundColor = UIColor.LightGray;
            underline.TranslatesAutoresizingMaskIntoConstraints = false;

            this.Layer.AddSublayer(cATextLayer);
            this.AddSubview(errorLabel);


            //this.AddSubview(underline);


            //Constraints
            errorLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f).Active = true;
            //errorLabel.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor).Active = true;
            errorLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;

            //Constraints
            //underline.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor).Active = true;
            //underline.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor).Active = true;
            //underline.BottomAnchor.ConstraintEqualTo(errorLabel.TopAnchor).Active = true;
            //underline.HeightAnchor.ConstraintEqualTo(constant: 1f).Active = true;


            //this.Layer.CornerRadius = 6;
            //this.Layer.BorderWidth = 1;
            //this.Layer.BorderColor = UIColor.LightGray.CGColor;


            cALayer = new CoreAnimation.CALayer();
            cALayer.BorderColor = UIColor.Black.CGColor;
            cALayer.BorderWidth = 1;
            cALayer.CornerRadius = 6;

            this.Layer.AddSublayer(cALayer);
        }


        public override CGRect TextRect(CGRect forBounds)
        {
            float marginOffsetMin = (float)(this.Font.LineHeight * 0.10);
            float marginOffsetHigh = (float)(this.Font.LineHeight * 0.30);

            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(7, 7, 22, 7));
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            float marginOffsetMin = (float)(this.Font.LineHeight * 0.10);
            float marginOffsetHigh = (float)(this.Font.LineHeight * 0.30);

            return InsetRect(base.EditingRect(forBounds), new UIEdgeInsets(7, 7, 22, 7));
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            float marginOffsetMin = (float)(this.Font.LineHeight * 0.30);

            cALayer.Frame = new CGRect(this.Frame.X, this.Frame.Y, this.Frame.Width, this.Frame.Height - 15);

        }


        private static CGRect InsetRect(CGRect rect, UIEdgeInsets insets)
        {
            return new CGRect(
                rect.X + insets.Left,
                rect.Y + insets.Top,
                rect.Width - insets.Left - insets.Right,
                rect.Height - insets.Top - insets.Bottom);
        }
    }
}