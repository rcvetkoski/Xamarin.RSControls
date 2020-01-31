using System;
using System.ComponentModel;
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

            //Delete placeholder as we use floating hint instead
            Element.Placeholder = "";
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = (this.Element as RSEntry).Error;
            }
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
        private CGPath borderShadowPath;
        private CustomCATextLayer floatingHint;
        private CustomCATextLayer errorLabel;
        private CustomCATextLayer helperLabel;
        private CustomCATextLayer counterLabel;
        private nfloat leftRightSpacingLabels;
        private int counterMaxLength;
        private int counter;
        private bool errorEnabled;
        private bool IsFloating;
        private bool hasInitfinished = false;
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
                    return errorLabel.String;
                else
                    return string.Empty;
            }
            set
            {
                errorLabel.String = value;

                if (!string.IsNullOrEmpty(value))
                {
                    errorEnabled = true;

                    if (!string.IsNullOrEmpty(this.rSControl.Helper))
                    {
                        helperLabel.Opacity = 0.0f;
                        helperLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                           this.Frame.Height - bottomSpacing);
                    }

                    errorLabel.Opacity = 1.0f;
                    errorLabel.Bounds = new CGRect(0.0f, 0.0f, errorLabel.Size.Width, errorLabel.Size.Height);
                    errorLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                                      this.Frame.Height - errorLabel.Size.Height / 2 - 1);

                    border.BorderColor = UIColor.SystemRedColor.CGColor;
                    floatingHint.ForegroundColor = UIColor.SystemRedColor.CGColor;
                }
                else
                {
                    errorEnabled = false;

                    if (!string.IsNullOrEmpty(this.rSControl.Helper))
                    {
                        helperLabel.Opacity = 1.0f;
                        helperLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                           this.Frame.Height - helperLabel.Size.Height / 2 - 1);
                    }

                    errorLabel.Opacity = 0.0f;
                    errorLabel.Bounds = new CGRect(0.0f, 0.0f, errorLabel.Size.Width, errorLabel.Size.Height);
                    errorLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                                      this.Frame.Height);

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
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                CreateUnderlineBorder();


            //SetIcons
            CreateIcons();


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
                this.bottomPadding = 27.5f + (nfloat)rSControl.Padding.Bottom;
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 30 + (nfloat)rSControl.Padding.Top; 
                this.bottomPadding = 20 + (nfloat)rSControl.Padding.Bottom; 
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
            {
                this.leftPadding = 2 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 20 + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = 20 + (nfloat)rSControl.Padding.Bottom;
            }
        }
        //Set Icons
        private void CreateIcons()
        {
            nfloat correctiveY = 0;

            if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                correctiveY = (topSpacing - bottomSpacing) / 2;
            else if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                correctiveY = (topSpacing - bottomSpacing) / 2;
            else if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                correctiveY = (topSpacing - bottomSpacing) / 2;

            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = "Samples/Data/SVG/calendar.svg", HeightRequest = 22, WidthRequest = 22, Color = Color.Gray };
            var convertedView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new CGRect(0, 0, 22, 22));
            UIView outerView = new UIView(new CGRect(0, 0, 22 + rightPadding, 22 - correctiveY)) { BackgroundColor = UIColor.Clear};
            outerView.AddSubview(convertedView);
            this.RightView = outerView;
            this.RightViewMode = UITextFieldViewMode.Always;
            UITapGestureRecognizer uITapGestureRecognizer = new UITapGestureRecognizer(() => {
                // Do something
                this.BecomeFirstResponder();
            });
            this.RightView.AddGestureRecognizer(uITapGestureRecognizer);
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
        //Edit changed
        private void RSUITextField_EditingChanged(object sender, EventArgs e)
        {
            if (this.counterMaxLength != -1)
                SetCounter();
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
        //Floating hint Animation properties
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
                BorderColor = UIColor.DarkGray.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = rSControl.BorderFillColor != Color.FromHex("#OA000000") ? rSControl.BorderFillColor.ToCGColor() : UIColor.Clear.CGColor,
                CornerRadius = this.borderRadius,
                ZPosition = -1 // So its behind floating label
            };

            //Border shadow if enabled
            //borderShadowPath = new CGPath();

            if(rSControl.ShadowEnabled)
            {
                border.ShadowColor = UIColor.Gray.CGColor;
                border.ShadowOpacity = 0.5f;
                border.ShadowRadius = 2;
                border.ShadowOffset = new CGSize(0f, 0.5f);
            }


            this.Layer.AddSublayer(border);
        }
        //Filled Border
        private void CreateFilledBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = rSControl.BorderFillColor.ToCGColor(),
                CornerRadius = this.borderRadius,
                ZPosition = -1 // So its behind floating label
            };

            border.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;

            if(rSControl.ShadowEnabled)
            {
                border.ShadowColor = UIColor.Gray.CGColor;
                border.ShadowOpacity = 0.5f;
                border.ShadowRadius = 2;
                border.ShadowOffset = new CGSize(0f, 0.5f);
            }


            this.Layer.AddSublayer(border);
        }
        //Underline Border
        private void CreateUnderlineBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = UIColor.SystemGray6Color.CGColor,
                ZPosition = -1 // So its behind floating label
            };

            this.BackgroundColor = rSControl.BorderFillColor.ToUIColor();

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

            floatingHint.AllowsEdgeAntialiasing = true;
            floatingHint.ContentsScale = UIScreen.MainScreen.Scale;
            floatingHint.ForegroundColor = borderColor.CGColor;
            floatingHint.BackgroundColor = UIColor.Clear.CGColor;
            floatingHint.Wrapped = false;

            floatingHint.Bounds = new CGRect(0.0f, 0.0f, floatingHint.Size.Width, floatingHint.Size.Height);

            this.Layer.AddSublayer(floatingHint);
        }
        //Create error label
        private void CreateErrorLabel()
        {
            errorLabel = new CustomCATextLayer(this.ErrorMessage, this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = UIColor.SystemRedColor.CGColor,
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false,
            };



            this.Layer.AddSublayer(errorLabel);
        }
        //Create helper label
        private void CreateHelperLabel()
        {
            helperLabel = new CustomCATextLayer(rSControl.Helper, this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = this.borderColor.CGColor,
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false,
            };

            helperLabel.Bounds = new CGRect(0.0f, 0.0f, helperLabel.Size.Width, helperLabel.Size.Height);

            this.Layer.AddSublayer(helperLabel);
        }
        //Create helper label
        private void CreateCounterLabel()
        {
            counterLabel = new CustomCATextLayer("", this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = this.borderColor.CGColor,
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false,
            };

            this.Layer.AddSublayer(counterLabel);
        }
        //Set Counter
        private void SetCounter()
        {
            this.counter = this.Text.Length;
            counterLabel.String = string.Format("{0}/{1}", this.counter, this.counterMaxLength);

            counterLabel.Bounds = new CGRect(0.0f, 0.0f, counterLabel.Size.Width, counterLabel.Size.Height);
            counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - leftRightSpacingLabels - counterLabel.Size.Width / 2,
                                                this.Frame.Height - counterLabel.Size.Height / 2 - 1);

            if (this.counter > this.counterMaxLength)
            {
                counterLabel.ForegroundColor = UIColor.SystemRedColor.CGColor;
                this.border.BorderColor = UIColor.SystemRedColor.CGColor;
                this.floatingHint.ForegroundColor = UIColor.SystemRedColor.CGColor;
                this.errorEnabled = true;
            }
            else
            {
                counterLabel.ForegroundColor = this.borderColor.CGColor;

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
            nfloat shadowOffset = 10;

            //Border frame
            border.Frame = new CGRect(0, topSpacing, this.Frame.Width, this.Frame.Height - this.bottomSpacing);

            var frame = this.Frame;

            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();


            //Whole border except top (border width)
            maskPath.AddRect(new CGRect(x: -shadowOffset, y: this.borderWidth + floatinghHintSizeFloating.Height / 2, width: frame.Width + shadowOffset * 2, height: frame.Height));

            //Top border right of floating label area
            maskPath.AddRect(new CGRect(x: (this.leftPadding + leftRightSpacingLabels + this.floatingHintMaskPadding + floatingHint.Size.Width),
                                        y: -shadowOffset,
                                        width: frame.Width,
                                        height: frame.Height));

            //Left border
            maskPath.AddRect(new CGRect(-shadowOffset, -shadowOffset, this.leftPadding + leftRightSpacingLabels - this.floatingHintMaskPadding + shadowOffset, frame.Height));

            //Top border libne full
            if (!IsFloating)
                maskPath.AddRect(new CGRect(x: -shadowOffset, y: -shadowOffset, width: frame.Width + shadowOffset * 2, height: frame.Height));


            //borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            this.border.Mask = borderMask;
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
        //Set Underline border frame
        public void UpdateUnderlineBorder()
        {
            //Border frame
            border.Frame = new CGRect(0, this.Frame.Height - this.bottomSpacing + topSpacing - border.BorderWidth, this.Frame.Width, border.BorderWidth);
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
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                UpdateUnderlineBorder();
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


        //Draw
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            //Update border
            if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                UpdateOutlinedBorder();
            else if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                UpdateFilledBorder();
            else if (rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                UpdateUnderlineBorder();


            //Init floatingHint X and Y values
            if (!hasInitfinished)
            {
                //border shadow
                //borderShadowPath.AddRoundedRect(new CGRect(x: 0, y: 0, width: Frame.Width, height: Frame.Height - bottomSpacing), borderRadius, borderRadius);
                //border.ShadowPath = borderShadowPath;


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
                    floatingHintYPostionFloating = (this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2) - floatinghHintSizeFloating.Height / 2 - 2;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2 ;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2 + leftRightSpacingLabels;
                    floatingHintXPostionNotFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = 0 + topSpacing + (floatinghHintSizeNotFloating.Height - floatinghHintSizeFloating.Height) / 2;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding) / 2;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;
                    floatingHintXPostionNotFloating = this.leftPadding + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = (this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2) - floatinghHintSizeFloating.Height / 2 - 2;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding) / 2;
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


                if (errorLabel != null)
                    errorLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                                       this.Frame.Height - errorLabel.Size.Height / 2 - 1);

                if (helperLabel != null)
                    helperLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                       this.Frame.Height - helperLabel.Size.Height / 2 - 1);

                //if (counterLabel != null)
                //    counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - leftRightSpacingLabels - counterLabel.Size.Width / 2,
                //                                       this.Frame.Height - counterLabel.Size.Height / 2 - 1);


                //Init here because text property not yet set in create floatinghint method
                FloatingHintFramePlacement();


                hasInitfinished = true;
            }

            //Counter
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
            UIFont uIFont = UIFont.ItalicSystemFontOfSize(this.FontSize);
            nSString = new NSString(hint);
            
            this.String = nSString;
           
        }

        public override string String
        {
            get => base.String;
            set
            {
                if(value == null)
                    nSString = (NSString)string.Empty;
                else
                    nSString = (NSString)value;

                base.String = nSString;
                UIFont font = UIFont.FromName(familyName, this.FontSize);
                font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Italic);
                UIStringAttributes attrs = new UIStringAttributes() { Font = font };
                Size = nSString.GetSizeUsingAttributes(attrs);
            }
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