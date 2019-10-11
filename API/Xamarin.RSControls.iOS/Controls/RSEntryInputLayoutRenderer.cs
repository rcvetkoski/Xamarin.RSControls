
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
            return new RSUITextField(this.Element as RSEntryInputLayout, 1, Color.LightGray.ToUIColor(), this.Element.BackgroundColor.ToCGColor(), this.Element.Behaviors.Any());
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                return;

            //(Control as RSUITextField).AttributedPlaceholder = new NSAttributedString(Control.Placeholder, null, UIColor.Clear);

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


    public class RSUITextField : UITextField
    {
        private RSEntryInputLayout rSEntryInputLayout;
        private CALayer border;
        private CALayer masklayer;
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

                if (!string.IsNullOrEmpty(value))
                {
                    errorEnabled = true;
                    errorLabel.Text = value;

                    Animate(0.2f, 0, UIViewAnimationOptions.CurveEaseIn, (() =>
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
                        if (this.IsEditing)
                        {
                            border.BorderColor = this.activeColor.CGColor;
                            floatingHint.TextColor = this.activeColor;
                        }
                        else
                        {
                            border.BorderColor = this.borderColor.CGColor;
                            floatingHint.TextColor = UIColor.DarkTextColor;
                        }

                    }), completion: () => { animationCompleted = true; });

                    if (animationCompleted)
                        errorLabel.Text = value;
                }
            }
        }

        //Graphics
        private UIColor activeColor;
        private UIColor borderColor;
        private nfloat borderWidth;
        public UIColor PlaceholderColor { get; set; }

        NSLayoutConstraint leadingConstraintFloating;
        NSLayoutConstraint centerYAnchorConstraintFloating;
        NSLayoutConstraint leadingConstraintNonFloating;
        NSLayoutConstraint centerYAnchorConstraintNonFloating;


        //Constructor
        public RSUITextField(RSEntryInputLayout rSEntryInputLayout, nfloat borderWidth, UIColor borderColor, CGColor backgroundColor, bool hasError)
        {
            this.rSEntryInputLayout = rSEntryInputLayout;
            this.borderColor = borderColor;
            this.borderWidth = borderWidth;
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

            UpdateFloatingLabel();
        }

        private void RSUITextField_Ended(object sender, EventArgs e)
        {
            if (!errorEnabled)
            {
                if (IsFloating())
                    this.floatingHint.TextColor = UIColor.DarkTextColor;
                else
                    this.floatingHint.TextColor = PlaceholderColor;

                this.border.BorderColor = this.borderColor.CGColor;
            }

            UpdateFloatingLabel();
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
                BackgroundColor = UIColor.Clear.CGColor,
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
            floatingHint = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(12),
                TextColor = UIColor.DarkTextColor,
                BackgroundColor = UIColor.Clear,
                Text = this.rSEntryInputLayout.Placeholder,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(floatingHint);


            leadingConstraintFloating = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 11f);
            centerYAnchorConstraintFloating = floatingHint.CenterYAnchor.ConstraintEqualTo(this.TopAnchor, 5f);

            leadingConstraintNonFloating = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f);
            centerYAnchorConstraintNonFloating = floatingHint.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor, -5f);
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


            //borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        private void FloatingHintInitPlacement()
        {
            if (IsFloating())
            {
                floatingHint.Font = UIFont.SystemFontOfSize(12);
                leadingConstraintFloating.Active = true;
                centerYAnchorConstraintFloating.Active = true;
            }
            else
            {
                floatingHint.Hidden = true;
                this.floatingHint.TextColor = PlaceholderColor;
                leadingConstraintNonFloating.Active = true;
                centerYAnchorConstraintNonFloating.Active = true;
            }
        }

        private void FloatingHintFramePlacement()
        {
            if (IsFloating())
            {
                nfloat x = this.Frame.X + 11; //Place at left, than add padding of 11
                nfloat y = this.Frame.Y + 5 - (floatingHint.Frame.Height / 2); //First set to border top y then move by hint half height to be in middle
                nfloat w = floatingHint.Frame.Width;
                nfloat h = floatingHint.Frame.Height;

                this.LayoutIfNeeded();
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                {
                    floatingHint.Frame = new CGRect(x, y, w, h);
                }), null);
            }
            else
            {
                nfloat x = this.Frame.X + 11; //Place at left, than add padding of 11
                nfloat y = this.Frame.GetMidY() + 5 - (floatingHint.Frame.Height / 2); //First set to border top y then move by hint half height to be in middle
                nfloat w = floatingHint.Frame.Width;
                nfloat h = floatingHint.Frame.Height;

                this.LayoutIfNeeded();
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                {
                    floatingHint.Frame = new CGRect(x, y, w, h);
                }), null);
            }
        }

        private void FloatingHintConstraintsPlacement()
        {
            if (IsFloating())
            {
                if (leadingConstraintNonFloating != null && centerYAnchorConstraintNonFloating != null)
                {
                    leadingConstraintNonFloating.Active = false;
                    centerYAnchorConstraintNonFloating.Active = false;
                }
                this.Placeholder = "";
                floatingHint.Hidden = false;
                floatingHint.Font = UIFont.SystemFontOfSize(12);
                leadingConstraintFloating.Active = true;
                centerYAnchorConstraintFloating.Active = true;

                //Animate
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                {
                    this.LayoutIfNeeded();
                }), null);
            }
            else
            {
                if (leadingConstraintFloating != null && centerYAnchorConstraintFloating != null)
                {
                    leadingConstraintFloating.Active = false;
                    centerYAnchorConstraintFloating.Active = false;
                }
                floatingHint.Font = UIFont.SystemFontOfSize(this.Font.PointSize);
                leadingConstraintNonFloating.Active = true;
                centerYAnchorConstraintNonFloating.Active = true;

                //Animate
                Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    this.LayoutIfNeeded();
                },
                completion: () =>
                {
                    floatingHint.Hidden = true;
                    this.Placeholder = floatingHint.Text;
                });
            }
        }

        //Update floating hint
        private void UpdateFloatingLabel()
        {
            FloatingHintConstraintsPlacement();

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

            //Init here because text property not yet set in create floatinghint method
            FloatingHintInitPlacement();
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