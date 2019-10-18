
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


    public class RSUITextField : UITextField
    {
        private string placeholder;
        private bool isPassword;
        private CALayer border;
        private CALayer filledBorder;
        private CALayer masklayer;
        private UILabel floatingHint;
        private UILabel errorLabel;
        private UILabel helperLabel;
        private UILabel counterLabel;
        private int counterMaxLength;
        private int counter;
        private RSEntryStyleSelectionEnum rsEntryStyle;
        public string HelperText { get; set; }
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
                errorLabel.Text = value;

                bool animationCompleted = false;

                if (!string.IsNullOrEmpty(value))
                {
                    errorEnabled = true;

                    Animate(0.2f, 0, UIViewAnimationOptions.CurveEaseIn, (() =>
                    {
                        if(!string.IsNullOrEmpty(HelperText))
                            helperLabel.Alpha = 0.0f;

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
                        if (!string.IsNullOrEmpty(HelperText))
                        {
                            helperLabel.Frame = new CGRect(helperLabel.Frame.X, helperLabel.Frame.Y + 5, helperLabel.Frame.Width, helperLabel.Frame.Height);
                            helperLabel.Alpha = 1.0f;
                        }

                        errorLabel.Alpha = 0.0f;

                        if (this.IsEditing)
                        {
                            border.BorderColor = this.activeColor.CGColor;
                            floatingHint.TextColor = this.activeColor;
                        }
                        else
                        {
                            border.BorderColor = this.borderColor.CGColor;
                            floatingHint.TextColor = this.borderColor;
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
        private UIColor backgroundColor;
        private nfloat borderWidth;
        public UIColor PlaceholderColor { get; set; }

        //Floating hint constraints
        NSLayoutConstraint leadingConstraintFloating;
        NSLayoutConstraint centerYAnchorConstraintFloating;
        NSLayoutConstraint leadingConstraintNonFloating;
        NSLayoutConstraint centerYAnchorConstraintNonFloating;


        //Padding values
        private nfloat topPadding;
        private nfloat bottomPadding;
        private nfloat underBorderSpaceHeight;

        //Device orientation observer
        private NSObject deviceRotationObserver;


        //Constructor
        public RSUITextField(string placeholder, string helperText, int counterMaxLength, bool isPassword, RSEntryStyleSelectionEnum rsEntryStyle, nfloat borderWidth, UIColor borderColor, UIColor backgroundColor, bool hasError)
        {
            this.placeholder = placeholder;
            this.HelperText = helperText;
            this.isPassword = isPassword;
            this.counterMaxLength = counterMaxLength;
            this.borderColor = borderColor;
            this.borderWidth = borderWidth;
            this.activeColor = UIColor.SystemBlueColor;
            this.rsEntryStyle = rsEntryStyle;
            this.backgroundColor = backgroundColor;

            if (this.rsEntryStyle == RSEntryStyleSelectionEnum.RoundedBorder)
            {
                this.topPadding = 15;
                this.bottomPadding = 25;
                this.underBorderSpaceHeight = 20;
            }
            else if(this.rsEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.topPadding = 20;
                this.bottomPadding = 20;
                this.underBorderSpaceHeight = 15;
            }


            this.AutocorrectionType = UITextAutocorrectionType.No;

            //Counter needs to be called before CreateErrorLabel and CReateHelperLabel
            if(this.counterMaxLength != -1)
                CreateCounterLabel();

            if (hasError)
                CreateErrorLabel();

            if (!string.IsNullOrEmpty(HelperText))
                CreateHelperLabel();

            if(this.isPassword)
                CreatePassword();

            CreateFloatingHint();

            if(this.rsEntryStyle == RSEntryStyleSelectionEnum.RoundedBorder)
                CreateRoundedBorder();
            else if(this.rsEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                CreateFilledBorder();

            //Edit text events
            this.Started += RSUITextField_Started;
            this.Ended += RSUITextField_Ended;
            this.EditingChanged += RSUITextField_EditingChanged;

            //Set event for device orientation change so we can reset border frame and mask
            deviceRotationObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);
        }

        private void RSUITextField_EditingChanged(object sender, EventArgs e)
        {
            if (this.counterMaxLength != -1)
                SetCounter();
        }

        //Edit started
        private void RSUITextField_Started(object sender, EventArgs e)
        {
            this.border.BorderWidth += 1;

            if (!errorEnabled)
            {
                this.floatingHint.TextColor = this.activeColor;
                this.border.BorderColor = this.activeColor.CGColor;
            }

            UpdateFloatingLabel();
        }

        //Edit finished
        private void RSUITextField_Ended(object sender, EventArgs e)
        {
            this.border.BorderWidth -= 1;

            if (!errorEnabled)
            {
                if (IsFloating())
                    this.floatingHint.TextColor = this.borderColor;
                else
                    this.floatingHint.TextColor = PlaceholderColor;

                this.border.BorderColor = this.borderColor.CGColor;
            }

            UpdateFloatingLabel();
        }

        //Return floating state
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

        //Rounded Border
        private void CreateFilledBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor =  UIColor.SystemGray6Color.CGColor,
                CornerRadius = 6,
                ZPosition = -1 // So its behind floating label
            };

            border.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;


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
                TextColor = this.borderColor,
                BackgroundColor = UIColor.Clear,
                Text = this.placeholder,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(floatingHint);


            leadingConstraintFloating = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f);

            if (this.rsEntryStyle == RSEntryStyleSelectionEnum.RoundedBorder)
                centerYAnchorConstraintFloating = floatingHint.CenterYAnchor.ConstraintEqualTo(this.TopAnchor, 5f);
            else if (this.rsEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                centerYAnchorConstraintFloating = floatingHint.CenterYAnchor.ConstraintEqualTo(this.TopAnchor, 10f);

            leadingConstraintNonFloating = floatingHint.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f);
            centerYAnchorConstraintNonFloating = floatingHint.TopAnchor.ConstraintEqualTo(this.TopAnchor, this.topPadding);
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
            errorLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f).Active = true;
            errorLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;

            if (counterLabel != null)
                errorLabel.TrailingAnchor.ConstraintEqualTo(counterLabel.LeadingAnchor, -8f).Active = true;
            else
                errorLabel.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -8f).Active = true;
        }

        //Create helper label
        private void CreateHelperLabel()
        {

            helperLabel = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(12),
                TextColor = this.borderColor,
                Text = this.HelperText,
                BackgroundColor = UIColor.Clear,
                Alpha = 1.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(helperLabel);

            //Constraints
            helperLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f).Active = true;
            helperLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;

            if(counterLabel != null)
                helperLabel.TrailingAnchor.ConstraintEqualTo(counterLabel.LeadingAnchor, - 8f).Active = true;
            else
                helperLabel.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -8f).Active = true;
        }

        //Create helper label
        private void CreateCounterLabel()
        {

            counterLabel = new UILabel()
            {
                Font = UIFont.SystemFontOfSize(12),
                TextColor = this.borderColor,
                BackgroundColor = UIColor.Clear,
                Alpha = 1.0f,
                TextAlignment = UITextAlignment.Right,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(counterLabel);

            //Constraints
            counterLabel.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -8f).Active = true;
            counterLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;
        }

        //Set Counter
        private void SetCounter()
        {
            this.counter = this.Text.Length;
            counterLabel.Text = string.Format("{0}/{1}", this.counter, this.counterMaxLength);

            if(this.counter > this.counterMaxLength)
            {
                counterLabel.TextColor = UIColor.SystemRedColor;
                this.border.BorderColor = UIColor.SystemRedColor.CGColor;
                this.floatingHint.TextColor = UIColor.SystemRedColor;
                this.errorEnabled = true;
            }
            else
            {
                counterLabel.TextColor = this.borderColor;


                if(string.IsNullOrEmpty(this.ErrorMessage) && this.IsFirstResponder)
                {
                    this.border.BorderColor = this.activeColor.CGColor;
                    this.floatingHint.TextColor = this.activeColor;
                    this.errorEnabled = false;
                }
                else if (string.IsNullOrEmpty(this.ErrorMessage) && !this.IsFirstResponder)
                {
                    this.border.BorderColor = this.borderColor.CGColor;
                    this.floatingHint.TextColor = this.borderColor;
                    this.errorEnabled = false;
                }
                else
                {
                    this.border.BorderColor = UIColor.SystemRedColor.CGColor;
                    this.floatingHint.TextColor = UIColor.SystemRedColor;
                    this.errorEnabled = true;
                }
            }
        }

        //Create Password
        private void CreatePassword()
        {
            bool isVisible = false;

            UIButton uIButtonPassword = new UIButton();

            uIButtonPassword.SetImage(UIImage.FromBundle("EyeInvisible"), UIControlState.Normal);
            uIButtonPassword.ImageView.TintColor = this.borderColor;
            uIButtonPassword.TouchUpInside += (sender, e) =>
            {
                UIView.Transition(uIButtonPassword.ImageView, 0.2,
                    UIViewAnimationOptions.TransitionCrossDissolve,
                    () =>
                    {
                        if (isVisible)
                        {
                            this.SecureTextEntry = true;
                            isVisible = false;
                            uIButtonPassword.SetImage(UIImage.FromBundle("EyeInvisible"), UIControlState.Normal);
                        }
                        else
                        {
                            isVisible = true;
                            this.SecureTextEntry = false;
                            uIButtonPassword.SetImage(UIImage.FromBundle("EyeVisible"), UIControlState.Normal);
                        }
                    },
                    null
                );

                //var colorAnimation = CABasicAnimation.FromKeyPath("backgroundColor");
                //colorAnimation.SetFrom(UIColor.LightGray.CGColor);
                //colorAnimation.Duration = 0.2f;  
                //uIButtonPassword.Layer.AddAnimation(colorAnimation, "ColorPulse");
            };

            this.RightView = uIButtonPassword;
            this.RightViewMode = UITextFieldViewMode.Always;
        }

        //Set Rounded border frame and mask
        public void SetBorder()
        {
            nfloat padding;
            if (!string.IsNullOrEmpty(this.placeholder))
                padding = 5;
            else
                padding = 0;


            //Border frame
            border.Frame = new CGRect(0, 5, this.Frame.Width, this.Frame.Height - this.underBorderSpaceHeight);


            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();

            //Whole border except top (border width)
            maskPath.AddRect(new CGRect(x: 0, y: this.borderWidth + 1, width: border.Frame.Width, height: border.Frame.Height));

            //Top border right of floating label area
            maskPath.AddRect(new CGRect(x: (floatingHint.Frame.X + floatingHint.Frame.Width) + padding, y: 0, width: border.Frame.Width - floatingHint.Frame.Width, height: this.border.Frame.Height));

            //Left border
            maskPath.AddRect(new CGRect(0, 0, floatingHint.Frame.X - padding, border.Frame.Height));

            //Top border libne full
            if (!IsFloating())
                maskPath.AddRect(new CGRect(x: 0, y: 0, width: border.Frame.Width, height: border.Frame.Height));


            //borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        //Set FIlled border frame
        public void SetFilledBorder()
        {
            //Border frame
            border.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height - this.underBorderSpaceHeight);

            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();

            //Top rounded border mask
            maskPath.AddRoundedRect(new CGRect(borderWidth + 1, borderWidth + 1, this.Frame.Width - (2 * borderWidth + 2), this.Frame.Height - this.underBorderSpaceHeight), 6, 6);

            //Bottom edges make them straight
            maskPath.AddRect(new CGRect(x: borderWidth + 1, y: this.border.Frame.Height / 2, width: this.Frame.Width - (2 * borderWidth + 2), height: this.border.Frame.Height));

            //Bottom border
            maskPath.AddRect(new CGRect(x: 0, y: this.border.Frame.Height, width: this.Frame.Width, height: this.borderWidth));


            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        //Only set at start to place hint
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

            //Update placement
            this.LayoutIfNeeded();

            //Draw border
            if (this.rsEntryStyle == RSEntryStyleSelectionEnum.RoundedBorder)
                SetBorder();
            else if (this.rsEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                SetFilledBorder();
        }

        //Not used
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

        //Floating hint placement at runtime
        private void FloatingHintConstraintsPlacement()
        {
            if (IsFloating())
            {
                this.LayoutIfNeeded();
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
                this.LayoutIfNeeded();
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
                    floatingHint.Hidden = false;
                    this.Placeholder = floatingHint.Text;
                });
            }
        }

        //Update floating hint
        private void UpdateFloatingLabel()
        {
            FloatingHintConstraintsPlacement();

            if (this.rsEntryStyle == RSEntryStyleSelectionEnum.RoundedBorder)
                SetBorder();
            else if (this.rsEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                SetFilledBorder();
        }

        //Device orientation change event
        private void DeviceRotated(NSNotification notification)
        {
            //Reset border frame and mask
            //if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.PortraitUpsideDown)
            // SetBorder();

            SetNeedsDisplay();
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

        //Text padding
        public override CGRect TextRect(CGRect forBounds)
        {
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(this.topPadding, 8, this.bottomPadding, 8));
        }

        //Edit rectangle padding
        public override CGRect EditingRect(CGRect forBounds)
        {
            return InsetRect(base.EditingRect(forBounds), new UIEdgeInsets(this.topPadding, 8, this.bottomPadding, 8));
        }

        //Right view padding
        public override CGRect RightViewRect(CGRect forBounds)
        {
            return new CGRect(
                base.RightViewRect(forBounds).X - 8,
                base.RightViewRect(forBounds).Y - 5,
                base.RightViewRect(forBounds).Width,
                base.RightViewRect(forBounds).Height);
        }

        //Left view padding
        public override CGRect LeftViewRect(CGRect forBounds)
        {
            return new CGRect(
                base.LeftViewRect(forBounds).X + 8,
                this.Center.Y - 5 - (base.LeftViewRect(forBounds).Height / 2),
                base.LeftViewRect(forBounds).Width,
                base.LeftViewRect(forBounds).Height);
        }

        //Draw method
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            //Init here because text property not yet set in create floatinghint method
            FloatingHintInitPlacement();

            if (this.counterMaxLength != -1)
                SetCounter();
        }

        //Remove any events when closed
        protected override void Dispose(bool disposing)
        {
            //Remove observer
            NSNotificationCenter.DefaultCenter.RemoveObserver(deviceRotationObserver);

            //Remove events
            this.Started -= RSUITextField_Started;
            this.Ended -= RSUITextField_Ended;
            this.EditingChanged -= RSUITextField_EditingChanged;

            base.Dispose(disposing);
        }
    }
}