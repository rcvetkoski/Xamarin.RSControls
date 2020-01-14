using System;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using CoreText;
using Foundation;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;
using Xamarin.RSControls.iOS.Extensions;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            //if(Control != null)
            //{
            //    Control.AttributedPlaceholder = "";
            //}


            //if (!entry.HasBorder)
            //{
            //    Control.BorderStyle = UITextBorderStyle.None;
            //    //Control.Layer.CornerRadius = 10;
            //    Control.TextColor = UIColor.White;
            //}
        }

        protected override UITextField CreateNativeControl()
        {
            return new RSUITextField(this.Element as IRSControl);
        }

        //Fix for bug ios not loading dll
        public static void Initialize()
        {

        }
    }

    public class RSUITextField : UITextField
    {
        private IRSControl rSControl;
        private nfloat borderRadius;
        private CALayer border;
        private CustomCATextLayer floatingHint;
        private UILabel errorLabel;
        private UILabel helperLabel;
        private UILabel counterLabel;
        private nfloat leftRightSpacingLabels;
        private int counterMaxLength;
        private int counter;
        private bool errorEnabled;
        private bool IsFloating;
        private bool hasInitfinished = false;
        private bool isFloatingHintAnimating;
        private nfloat floatingHintXPostion;
        private nfloat floatingHintXPostionFloating;
        private nfloat floatingHintXPostionNotFloating;
        private nfloat floatingHintYPostion;
        private nfloat floatingHintYPostionFloating;
        private nfloat floatingHintYPostionNotFloating;
        private CGSize floatinghHintSizeFloating;
        private CGSize floatinghHintSizeNotFloating;
        private nfloat floatingHintMaskPadding;
        private nfloat floatingFontSize;
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
                        if (!string.IsNullOrEmpty(this.rSControl.Helper))
                            helperLabel.Alpha = 0.0f;

                        errorLabel.Frame = new CGRect(errorLabel.Frame.X, errorLabel.Frame.Y + 5, errorLabel.Frame.Width, errorLabel.Frame.Height);
                        errorLabel.Alpha = 1.0f;
                        border.BorderColor = UIColor.SystemRedColor.CGColor;
                        floatingHint.ForegroundColor = UIColor.SystemRedColor.CGColor;
                    }), null);
                }
                else
                {
                    errorEnabled = false;

                    Animate(0.3f, 0, UIViewAnimationOptions.CurveEaseOut, (() =>
                    {
                        if (!string.IsNullOrEmpty(this.rSControl.Helper))
                        {
                            helperLabel.Frame = new CGRect(helperLabel.Frame.X, helperLabel.Frame.Y + 5, helperLabel.Frame.Width, helperLabel.Frame.Height);
                            helperLabel.Alpha = 1.0f;
                        }

                        errorLabel.Alpha = 0.0f;

                        if (this.IsEditing)
                        {
                            border.BorderColor = this.activeColor.CGColor;
                            floatingHint.ForegroundColor = this.activeColor.CGColor;
                        }
                        else
                        {
                            border.BorderColor = this.borderColor.CGColor;
                            floatingHint.ForegroundColor = this.borderColor.CGColor;
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
        private nfloat borderWidthFocused;
        public UIColor PlaceholderColor { get; set; }


        //Padding values
        private nfloat topSpacing;
        private nfloat bottomSpacing;
        private nfloat topPadding;
        private nfloat bottomPadding;
        private nfloat leftPadding;
        private nfloat rightPadding;


        //Animation
        private CABasicAnimation cATextFontSizeAnimator;

        //Device orientation observer
        private NSObject deviceRotationObserver;


        //Constructor
        public RSUITextField(IRSControl rSControl)
        {
            this.rSControl = rSControl;
            if (PlaceholderColor == null)
                PlaceholderColor = Color.Gray.ToUIColor();

            if(rSControl.FontSize < 12)
                this.floatingFontSize = (nfloat)rSControl.FontSize;
            else
                this.floatingFontSize = 12;

            this.floatingHintMaskPadding = 3;
            this.borderRadius = rSControl.BorderRadius / 2; //divided by 2 to get android equivalent value
            this.counterMaxLength = this.rSControl.CounterMaxLength;
            this.borderColor = this.rSControl.BorderColor.ToUIColor();
            this.borderWidth = 1;
            this.borderWidthFocused = 2;
            this.activeColor = UIColor.SystemBlueColor;
            this.topSpacing = 5;
            this.bottomSpacing = 20;
            this.leftRightSpacingLabels = 5;

            //Padding
            SetPadding();


            this.AutocorrectionType = UITextAutocorrectionType.No;


            //Counter needs to be called before CreateErrorLabel and CReateHelperLabel
            if (this.counterMaxLength != -1)
                CreateCounterLabel();

            if (this.rSControl.HasError)
                CreateErrorLabel();

            if (!string.IsNullOrEmpty(this.rSControl.Helper))
                CreateHelperLabel();

            if (this.rSControl.IsPassword)
                CreatePassword();

            CreateFloatingHint();

            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                CreateRoundedBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                CreateFilledBorder();

            //Edit text events
            this.Started += RSUITextField_Started;
            this.Ended += RSUITextField_Ended;
            this.EditingChanged += RSUITextField_EditingChanged;

            //Set event for device orientation change so we can reset border frame and mask
            deviceRotationObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);
        }

        //Set Padding
        private void SetPadding()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 20 + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = 25 + (nfloat)rSControl.Padding.Bottom;
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 35 + (nfloat)rSControl.Padding.Top; 
                this.bottomPadding = 20 + (nfloat)rSControl.Padding.Bottom; 
            }
        }



        private void RSUITextField_EditingChanged(object sender, EventArgs e)
        {
            if (this.counterMaxLength != -1)
                SetCounter();
        }

        //Edit started
        private void RSUITextField_Started(object sender, EventArgs e)
        {
            this.IsFloating = true;
            this.border.BorderWidth = this.borderWidthFocused;

            if (!errorEnabled)
            {
                this.floatingHint.ForegroundColor = this.activeColor.CGColor;
                this.border.BorderColor = this.activeColor.CGColor;
            }

            floatingHintXPostion = floatingHintXPostionFloating;
            floatingHintYPostion = floatingHintYPostionFloating;

            AnimateFloatingHint(floatingFontSize);

            UpdateFloatingLabel();
        }

        //Edit finished
        private void RSUITextField_Ended(object sender, EventArgs e)
        {
            if (rSControl.IsPlaceholderAlwaysFloating)
                IsFloating = true;
            else if (this.IsFirstResponder || !string.IsNullOrEmpty(this.Text) || errorEnabled)
                IsFloating = true;
            else
                IsFloating = false;

            this.border.BorderWidth = this.borderWidth;

            if (!errorEnabled)
            {
                if (IsFloating)
                    this.floatingHint.ForegroundColor = this.borderColor.CGColor;
                else
                    this.floatingHint.ForegroundColor = PlaceholderColor.CGColor;

                this.border.BorderColor = this.borderColor.CGColor;
            }

            floatingHintXPostion = floatingHintXPostionNotFloating;
            floatingHintYPostion = floatingHintYPostionNotFloating;

            if(!IsFloating)
                AnimateFloatingHint(rSControl.FontSize);

            UpdateFloatingLabel();



            //CABasicAnimation cABasicAnimation = new CABasicAnimation();
            //cABasicAnimation.KeyPath = "fontSize";
            //cABasicAnimation.SetTo(NSNumber.FromDouble(rSControl.FontSize));
            //cABasicAnimation.SetFrom(NSNumber.FromDouble(12));
            //cABasicAnimation.Duration = 0.2;
            //cABasicAnimation.FillMode = CAFillMode.Forwards;
            //cABasicAnimation.RemovedOnCompletion = false;
            //cABasicAnimation.AutoReverses = false;
            //floatingHint.AddAnimation(cABasicAnimation, "basic");
        }

        private void AnimateFloatingHint(double toValue)
        {
            //Animation is trigered automatically when changing this properties
            floatingHint.FontSize = (nfloat)toValue;
            floatingHint.Position = new CGPoint(floatingHintXPostion, floatingHintYPostion);
        }


        //Rounded Border
        private void CreateRoundedBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = UIColor.Clear.CGColor,
                CornerRadius = this.borderRadius,
                ZPosition = -1 // So its behind floating label
            };


            this.Layer.AddSublayer(border);
        }
        //Rounded Border
        private void CreateFilledBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = UIColor.SystemGray6Color.CGColor,
                CornerRadius = this.borderRadius,
                ZPosition = -1 // So its behind floating label
            };

            border.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;


            this.Layer.AddSublayer(border);
        }


        //Create floatingHint
        private void CreateFloatingHint()
        {
            floatingHint = new CustomCATextLayer(rSControl.Placeholder, this.Font.FamilyName);

            //Init floatinhHint sizes
            floatingHint.FontSize = floatingFontSize;
            floatinghHintSizeFloating = floatingHint.Size;
            floatingHint.FontSize = (nfloat)this.rSControl.FontSize;
            floatinghHintSizeNotFloating = floatingHint.Size;

            //floatingHint.TextAlignmentMode = CATextLayerAlignmentMode.Natural;

            floatingHint.Bounds = new CGRect(0.0f, 0.0f, floatingHint.Size.Width, floatingHint.Size.Height);

            
            floatingHint.AllowsEdgeAntialiasing = true;
            floatingHint.ContentsScale = UIScreen.MainScreen.Scale;
            floatingHint.ForegroundColor = UIColor.Black.CGColor;
            floatingHint.BackgroundColor = UIColor.Clear.CGColor;

            floatingHint.Wrapped = false;

            //Font size animation
            cATextFontSizeAnimator = new CABasicAnimation();
            //cATextFontSizeAnimator.KeyPath = "fontSize";


            this.Layer.AddSublayer(floatingHint);
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
                Text = this.rSControl.Helper,
                BackgroundColor = UIColor.Clear,
                Alpha = 1.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            this.AddSubview(helperLabel);

            //Constraints
            helperLabel.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 8f).Active = true;
            helperLabel.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;

            if (counterLabel != null)
                helperLabel.TrailingAnchor.ConstraintEqualTo(counterLabel.LeadingAnchor, -8f).Active = true;
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

            if (this.counter > this.counterMaxLength)
            {
                counterLabel.TextColor = UIColor.SystemRedColor;
                this.border.BorderColor = UIColor.SystemRedColor.CGColor;
                this.floatingHint.ForegroundColor = UIColor.SystemRedColor.CGColor;
                this.errorEnabled = true;
            }
            else
            {
                counterLabel.TextColor = this.borderColor;


                if (string.IsNullOrEmpty(this.ErrorMessage) && this.IsFirstResponder)
                {
                    this.border.BorderColor = this.activeColor.CGColor;
                    this.floatingHint.ForegroundColor = this.activeColor.CGColor;
                    this.errorEnabled = false;
                }
                else if (string.IsNullOrEmpty(this.ErrorMessage) && !this.IsFirstResponder)
                {
                    this.border.BorderColor = this.borderColor.CGColor;
                    this.floatingHint.ForegroundColor = this.borderColor.CGColor;
                    this.errorEnabled = false;
                }
                else
                {
                    this.border.BorderColor = UIColor.SystemRedColor.CGColor;
                    this.floatingHint.ForegroundColor = UIColor.SystemRedColor.CGColor;
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



        //Set rounded border frame and mask
        public void UpdateOutlinedBorder()
        {
            //Border frame
            border.Frame = new CGRect(0, topSpacing, this.Frame.Width, this.Frame.Height - this.bottomSpacing);


            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();

            //Whole border except top (border width)
            maskPath.AddRect(new CGRect(x: 0, y: this.borderWidth + 1, width: border.Frame.Width, height: border.Frame.Height));

            //Top border right of floating label area
            maskPath.AddRect(new CGRect(x: (this.leftPadding + leftRightSpacingLabels + this.floatingHintMaskPadding + floatingHint.Size.Width), y: 0, width: border.Frame.Width, height: this.border.Frame.Height));

            //Left border
            maskPath.AddRect(new CGRect(0, 0, this.leftPadding + leftRightSpacingLabels - this.floatingHintMaskPadding, border.Frame.Height));

            //Top border libne full
            if (!IsFloating)
                maskPath.AddRect(new CGRect(x: 0, y: 0, width: border.Frame.Width, height: border.Frame.Height));


            //borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        //Set FIlled border frame
        public void UpdateFilledBorder()
        {
            //Border frame
            border.Frame = new CGRect(0, topSpacing, this.Frame.Width, this.Frame.Height - this.bottomSpacing);

            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();

            //Top rounded border mask
            maskPath.AddRoundedRect(new CGRect(borderWidth + 1, borderWidth + 1, this.Frame.Width - (2 * borderWidth + 2), this.Frame.Height - this.bottomSpacing), borderRadius, borderRadius);

            //Bottom edges make them straight
            maskPath.AddRect(new CGRect(x: borderWidth + 1, y: this.border.Frame.Height / 2, width: this.Frame.Width - (2 * borderWidth + 2), height: this.border.Frame.Height));

            //Bottom border
            maskPath.AddRect(new CGRect(x: 0, y: this.border.Frame.Height, width: this.Frame.Width, height: this.borderWidth));


            borderMask.Path = maskPath;
            border.Mask = borderMask;
        }

        //Only set at start to place hint
        private void FloatingHintFramePlacement()
        {
            floatingHint.Position = new CGPoint(floatingHintXPostion, floatingHintYPostion);
        }


        //Update floating hint
        public void UpdateFloatingLabel()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                UpdateOutlinedBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                UpdateFilledBorder();
        }

        //Device orientation change event
        private void DeviceRotated(NSNotification notification)
        {
            //Reset border frame and mask
            //if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.PortraitUpsideDown)
            // SetBorder();

            SetNeedsDisplay();
        }


        //Helper for padding
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
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding, this.bottomPadding, this.rightPadding));
        }
        //Placeholder padding
        public override CGRect PlaceholderRect(CGRect forBounds)
        {
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding, this.bottomPadding, this.rightPadding));
        }
        //Edit rectangle padding
        public override CGRect EditingRect(CGRect forBounds)
        {
            return InsetRect(base.EditingRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding, this.bottomPadding, this.rightPadding));
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

            if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                UpdateOutlinedBorder();
            else if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                UpdateFilledBorder();


            //Init floatingHint X and Y values
            if (!hasInitfinished)
            {
                if (rSControl.IsPlaceholderAlwaysFloating)
                    IsFloating = true;
                else if (this.IsFirstResponder || !string.IsNullOrEmpty(this.Text) || errorEnabled)
                    IsFloating = true;
                else
                    IsFloating = false;



                if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;
                    floatingHintXPostionNotFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = border.Position.Y - floatinghHintSizeFloating.Height / 2;
                    floatingHintYPostionNotFloating = border.Position.Y + topSpacing + borderWidth * 2;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2 + leftRightSpacingLabels;
                    floatingHintXPostionNotFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = 0 + topSpacing + (floatinghHintSizeNotFloating.Height - floatinghHintSizeFloating.Height) / 2;
                    floatingHintYPostionNotFloating = border.Position.Y + borderWidth * 2;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding;
                    floatingHintXPostionNotFloating = this.leftPadding;

                    //Y
                    floatingHintYPostionFloating = this.topSpacing + floatinghHintSizeNotFloating.Width / 2;
                    floatingHintYPostionNotFloating = border.Position.Y;
                }


                if (IsFloating)
                {
                    floatingHint.FontSize = floatingFontSize;
                    floatingHintXPostion = floatingHintXPostionFloating;
                    floatingHintYPostion = floatingHintYPostionFloating;
                }
                else
                {
                    floatingHint.FontSize = (nfloat)this.rSControl.FontSize;
                    floatingHintXPostion = floatingHintXPostionNotFloating;
                    floatingHintYPostion = floatingHintYPostionNotFloating;
                }


                //if (errorPaint != null)
                //    errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent() + 2;

                //if (helperPaint != null)
                //    helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent() + 2;

                //if (counterPaint != null)
                //    counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent() + 2;

                //Init here because text property not yet set in create floatinghint method
                FloatingHintFramePlacement();

                hasInitfinished = true;
            }



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


    public class CustomCATextLayer : CATextLayer
    {
        public CGSize Size;
        private string familyName;
        private NSString nSString;

        public override void Clone(CALayer other)
        {
            base.Clone(other);
        }

        [return: Release]
        public override NSObject Copy()
        {
            return base.Copy();
        }

        public CustomCATextLayer(IntPtr handle) : base(handle)
        {

        }

        public CustomCATextLayer(NSCoder coder) : base(coder)
        {

        }

        public CustomCATextLayer(NSObjectFlag f) : base(f)
        {

        }

        public CustomCATextLayer(string hint, string familyName)
        {
            this.familyName = familyName;
            this.SetFont(familyName);
            nSString = new NSString(hint);
            this.String = nSString;

            //UIStringAttributes attrs = new UIStringAttributes();
            //attrs.Font = UIFont.FromName(familyName, this.FontSize);
            //Size = nSString.GetSizeUsingAttributes(attrs);
        }

        public override nfloat FontSize
        {
            get
            {
                return base.FontSize;
            }
            set
            {
                base.FontSize = value;

                if (string.IsNullOrEmpty(familyName))
                    return;

                UIStringAttributes attrs = new UIStringAttributes() { Font = UIFont.FromName(familyName, value) };
                Size = nSString.GetSizeUsingAttributes(attrs);
            }
        }
    }
}