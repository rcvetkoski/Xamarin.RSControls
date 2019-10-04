
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.ComponentModel;
using System.Linq;
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
            return new RSUITextField(this.Element.FontSize, 1, Color.LightGray.ToUIColor(), this.Element.BackgroundColor.ToCGColor(), this.Element.Behaviors.Any());
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
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


    public class RSUITextField : UITextField
    {
        private CALayer border;
        private CALayer masklayer;
        private CATextLayer floatingHintCAText;
        private UILabel floatingHint;
        private UILabel errorLabel;
        private bool errorEnabled;

        public string ErrorMessage
        {
            get
            {
                if (errorLabel != null)
                    return errorLabel.Text;
                else
                    return string.Empty;
            }
            set
            {
                bool animationCompleted = false;

                if(!string.IsNullOrEmpty(value))
                {
                    errorEnabled = true;
                    errorLabel.Text = value;

                    Animate(0.1f, 0, UIViewAnimationOptions.CurveEaseIn, (() =>
                    {
                        errorLabel.Frame = new CGRect(errorLabel.Frame.X, errorLabel.Frame.Y + 5, errorLabel.Frame.Width, errorLabel.Frame.Height);
                        errorLabel.Alpha = 1.0f;
                        border.BorderColor = UIColor.SystemRedColor.CGColor;
                        floatingHint.TextColor = UIColor.SystemRedColor;
                    }), null);
                }
                else
                {
                    errorEnabled = false;

                    Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                    {
                        errorLabel.Alpha = 0.0f;
                        if(this.IsEditing)
                        {
                            border.BorderColor = this.activeColor.CGColor;
                            floatingHint.TextColor = this.activeColor;
                        }
                        else
                        {
                            border.BorderColor = this.borderColor.CGColor;
                            floatingHint.TextColor = UIColor.DarkTextColor;
                        }

                    }), completion: ()=> { animationCompleted = true; } ) ;

                    if (animationCompleted)
                        errorLabel.Text = value;
                }
            }
        }


        //Graphics
        private UIColor activeColor;
        private UIColor borderColor;
        private CGColor backgroundColor;
        private nfloat borderWidth;


        NSLayoutConstraint floatingHintLeadingAnchorConstraintFloating;
        NSLayoutConstraint floatingHintCenterYAnchorConstraintFloating;
        NSLayoutConstraint floatingHintLeadingAnchorConstraint;
        NSLayoutConstraint floatingHintCenterYAnchorConstraint;


        //Constructor
        public RSUITextField(double fontSize, nfloat borderWidth, UIColor borderColor, CGColor backgroundColor, bool hasError)
        {
            this.borderColor = borderColor;
            this.borderWidth = borderWidth;
            this.backgroundColor = backgroundColor;
            this.activeColor = UIColor.SystemBlueColor;

            if (hasError)
                CreateErrorLabel();

            CreateFloatingHint();
            CreateRoundedBorder();

            this.Started += RSUITextField_Started;
            this.Ended += RSUITextField_Ended;

            //Set event for device orientation change so we can reset border frame and mask
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);
        }

        private void RSUITextField_Started(object sender, EventArgs e)
        {
            if (!errorEnabled)
            {
                this.floatingHint.TextColor = this.activeColor;
                this.border.BorderColor = this.activeColor.CGColor;
            }

            this.LayoutIfNeeded();
            Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
            {
                UpdateFloatingLabel();
                this.LayoutIfNeeded();
            }), null);
        }


        private void RSUITextField_Ended(object sender, EventArgs e)
        {
            if (!errorEnabled)
            {
                this.floatingHint.TextColor = UIColor.DarkTextColor;
                this.border.BorderColor = this.borderColor.CGColor;
            }

            this.LayoutIfNeeded();
            Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
            {
                UpdateFloatingLabel();
                this.LayoutIfNeeded();
            }), null);
        }

        private bool IsFloating()
        {
            if (!string.IsNullOrEmpty(this.Text) || errorEnabled || this.IsFirstResponder)
                return true;
            else
                return false;
        }


        //Rounded Border
        private void CreateRoundedBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = UIColor.Yellow.CGColor,
                CornerRadius = 6,
                ZPosition = -1 // So its behind floating label
            };


            //Mask
            masklayer = new CALayer();
            masklayer.BorderColor = this.borderColor.CGColor;
            masklayer.BorderWidth = this.borderWidth;


            this.Layer.AddSublayer(border);
        }

        //Create floatingHint
        private void CreateFloatingHint()
        {
            //floatingHintCAText = new CATextLayer()
            //{
            //    String = "Test",
            //    FontSize = 12,
            //    ForegroundColor = UIColor.Green.CGColor,
            //    ContentsScale = UIScreen.MainScreen.Scale,
            //    Frame = new CGRect(0, 0, 50, 30),
            //    ZPosition = 1
            //};
            //this.Layer.AddSublayer(floatingHintCAText);

            floatingHint = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(12),
                TextColor = UIColor.DarkTextColor,
                BackgroundColor = UIColor.Clear,
                Text = "Test",
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(floatingHint);

            //Constraints floating
            floatingHintLeadingAnchorConstraintFloating = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 11f); //With left padding of 11
            floatingHintCenterYAnchorConstraintFloating = floatingHint.CenterYAnchor.ConstraintEqualTo(this.TopAnchor, 5f); //Move 5 to match border position

            //Constraints non floating
            floatingHintLeadingAnchorConstraint = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 11f); //With left padding of 11
            floatingHintCenterYAnchorConstraint = floatingHint.BottomAnchor.ConstraintEqualTo(this.BottomAnchor, -25f); //Give bottomAnchor and move it by bottom margin


            if (IsFloating())
            {
                floatingHintLeadingAnchorConstraintFloating.Active = true;
                floatingHintCenterYAnchorConstraintFloating.Active = true;
            }
            else
            {
                floatingHintLeadingAnchorConstraint.Active = true;
                floatingHintCenterYAnchorConstraint.Active = true;
            }
        }

        //Create error label
        private void CreateErrorLabel()
        {

            errorLabel = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(12),
                TextColor = UIColor.SystemRedColor,
                Text = this.ErrorMessage,
                BackgroundColor = UIColor.Clear,
                Alpha = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(errorLabel);

            //Constraints
            errorLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 11f).Active = true;
            errorLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;
        }

        //Set border frame and mask
        public void SetBorder()
        {
            //Border frame
            //We set bottom padding to 25 so we leave 5 for errollabel at bottom
            border.Frame = new CGRect(this.Frame.X, this.Frame.Y + 5, this.Frame.Width, this.Frame.Height - 20);


            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();

            //Whole border except top (border width)
            maskPath.AddRect(new CGRect(x: border.Frame.X, y: this.Frame.Y + this.borderWidth, width: border.Frame.Width, height: border.Frame.Height - this.borderWidth));

            //Top border right of floating label area
            maskPath.AddRect(new CGRect(x: (floatingHint.Frame.X + floatingHint.Frame.Width) + 5, y: this.Frame.Y, width: border.Frame.Width - floatingHint.Frame.Width, height: this.borderWidth));

            //Left border
            maskPath.AddRect(new CGRect(border.Frame.X, this.Frame.Y, floatingHint.Frame.X - 5, this.borderWidth));

            //Top border libne full
            if (!IsFloating())
                maskPath.AddRect(new CGRect(x: border.Frame.X, y: this.Frame.Y, width: border.Frame.Width - floatingHint.Frame.Width, height: this.borderWidth));


            borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        //Update floating hint
        private void UpdateFloatingLabel()
        {
            if (IsFloating())
            {
                floatingHintLeadingAnchorConstraint.Active = false;
                floatingHintCenterYAnchorConstraint.Active = false;

                floatingHintLeadingAnchorConstraintFloating.Active = true;
                floatingHintCenterYAnchorConstraintFloating.Active = true;

                //floatingHint.Font = UIFont.SystemFontOfSize(12);
                this.LayoutIfNeeded();
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                {
                    floatingHint.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);

                    this.LayoutIfNeeded();
                }), null);

                this.AttributedPlaceholder = new NSAttributedString(this.Placeholder, null, UIColor.Clear);
            }
            else
            {
                floatingHintLeadingAnchorConstraintFloating.Active = false;
                floatingHintCenterYAnchorConstraintFloating.Active = false;

                floatingHintLeadingAnchorConstraint.Active = true;
                floatingHintCenterYAnchorConstraint.Active = true;

                var scaleFactor = this.Font.PointSize / 12;
                this.LayoutIfNeeded();
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                {
                    floatingHint.Transform = CGAffineTransform.MakeScale(scaleFactor, scaleFactor);

                    this.LayoutIfNeeded();
                }), null);

                //floatingHint.Font = UIFont.SystemFontOfSize(this.Font.PointSize);
                this.AttributedPlaceholder = new NSAttributedString(this.Placeholder, null, UIColor.Clear);
            }

            SetBorder();
        }

        //Device orientation change event
        private void DeviceRotated(NSNotification notification)
        {
            //Reset border frame and mask
            if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.PortraitUpsideDown)
                SetBorder();
        }

        //Helper
        private static CGRect InsetRect(CGRect rect, UIEdgeInsets insets)
        {
            return new CGRect(
                rect.X + insets.Left,
                rect.Y + insets.Top,
                rect.Width - insets.Left - insets.Right,
                rect.Height - insets.Top - insets.Bottom);
        }

        //Overides
        public override CGRect TextRect(CGRect forBounds)
        {
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(16, 8, 25, 8));
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return InsetRect(base.EditingRect(forBounds), new UIEdgeInsets(16, 8, 25, 8));
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            UpdateFloatingLabel();
        }


        protected override void Dispose(bool disposing)
        {
            //Remove observer
            NSNotificationCenter.DefaultCenter.RemoveObserver(new NSString("UIDeviceOrientationDidChangeNotification"));

            //Remove events
            this.Started -= RSUITextField_Started;
            this.Ended -= RSUITextField_Ended;

            base.Dispose(disposing);
        }
    }
}