using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSEditor), typeof(RSEditorRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSEditorRenderer : EditorRenderer
    {
        public RSEditorRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    // Instantiate the native control and assign it to the Control property with
                    // the SetNativeControl method


                }
                // Configure the control and subscribe to event handlers

                //Delete placeholder as we use floating hint instead
                Element.Placeholder = "";
            }


            if (e.NewElement == null)
                return;

            //(Element as Forms.View).HeightRequest = 90;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextView).ErrorMessage = (this.Element as RSEditor).Error;
            }

            //if (e.PropertyName == "Text" && !(sender as Forms.View).IsFocused)
            //    (this.Control as RSUITextField).UpdateView();
        }

        protected override UITextView CreateNativeControl()
        {
            return new RSUITextView(this.Element as IRSControl);
        }
    }

    public class RSUITextView : UITextView
    {
        private IRSControl rSControl;
        private nfloat borderRadius;
        private nfloat shadowRadius;
        private UIView borderContainer;
        private CALayer border;
        private CALayer filledBorder;
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

                    //if (this.IsEditing)
                    //{
                    //    border.BorderColor = this.activeColor.CGColor;
                    //    floatingHint.ForegroundColor = this.activeColor.CGColor;
                    //}
                    //else
                    //{
                    //    border.BorderColor = this.borderColor.CGColor;
                    //    floatingHint.ForegroundColor = this.borderColor.CGColor;
                    //}
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

        UIView mask;


        //Constructor
        public RSUITextView(IRSControl rSControl)
        {
            this.rSControl = rSControl;
            if (PlaceholderColor == null)
                PlaceholderColor = Color.Gray.ToUIColor();

            if (rSControl.FontSize < 12)
                this.floatingFontSize = (nfloat)rSControl.FontSize;
            else
                this.floatingFontSize = 12;

            this.floatingHintMaskPadding = 3;
            this.borderRadius = rSControl.BorderRadius;
            this.shadowRadius = rSControl.ShadowRadius;
            this.counterMaxLength = this.rSControl.CounterMaxLength;
            this.borderColor = this.rSControl.BorderColor.ToUIColor();
            this.borderWidth = 1;
            this.borderWidthFocused = 2;
            this.activeColor = UIColor.SystemBlueColor;
            this.topSpacing = 5;
            this.bottomSpacing = 20;
            this.leftRightSpacingLabels = 5;

            //Disable scroll bounce when reaaches limits
            this.Bounces = false;

            //Set default font since is null if editor text empty
            this.Font = UIFont.FromName(".AppleSystemUIFont", (nfloat)rSControl.FontSize);

            //Set to editable
            this.Editable = true;

            //Padding
            SetPadding();


            this.AutocorrectionType = UITextAutocorrectionType.No;

            CreateBorderContainer();

            //Counter needs to be called before CreateErrorLabel and CReateHelperLabel
            if (this.counterMaxLength != -1)
                CreateCounterLabel();

            if (this.rSControl.HasError)
                CreateErrorLabel();

            if (!string.IsNullOrEmpty(this.rSControl.Helper))
                CreateHelperLabel();


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
            this.Started += RSUITextView_Started;
            this.Ended += RSUITextView_Ended;
            this.Changed += RSUITextView_EditingChanged;
            if ((rSControl as RSEditor).AutoSize == EditorAutoSizeOption.Disabled || (rSControl as RSEditor).HeightRequest != -1)
                this.Scrolled += RSUITextView_Scrolled;


            (rSControl as RSEditor).MaxHeight = 200;
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
                this.topPadding = 17 + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = 24.5f + (nfloat)rSControl.Padding.Bottom;
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

            this.TextContainerInset = new UIEdgeInsets(this.topPadding, this.leftPadding - 5, this.bottomPadding, this.rightPadding - 5);
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
            UIView outerView = new UIView(new CGRect(0, 0, 22 + rightPadding, 22 - correctiveY)) { BackgroundColor = UIColor.Clear };
            outerView.AddSubview(convertedView);
            //this.RightView = outerView;
            //this.RightViewMode = UITextFieldViewMode.Always;
            UITapGestureRecognizer uITapGestureRecognizer = new UITapGestureRecognizer(() => {
                // Do something
                this.BecomeFirstResponder();
            });
            //this.RightView.AddGestureRecognizer(uITapGestureRecognizer);
        }


        //Edit started
        private void RSUITextView_Started(object sender, EventArgs e)
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


            //Update border
            this.Layer.SetNeedsDisplay();
        }
        //Edit changed
        private void RSUITextView_EditingChanged(object sender, EventArgs e)
        {
            if (this.counterMaxLength != -1)
                SetCounter();


            //Update border
            this.Layer.SetNeedsDisplay();
        }
        //Edit finished
        private void RSUITextView_Ended(object sender, EventArgs e)
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
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor(); ;

                this.border.BorderColor = this.borderColor.CGColor;
            }

            floatingHintXPostion = floatingHintXPostionNotFloating;
            floatingHintYPostion = floatingHintYPostionNotFloating;


            if (!IsFloating)
                AnimateFloatingHint(rSControl.FontSize);

            //Update border
            this.Layer.SetNeedsDisplay();
        }
        //Floating hint Animation properties
        private void AnimateFloatingHint(double toValue)
        {
            //Animation is trigered automatically when changing this properties
            floatingHint.FontSize = (nfloat)toValue;
            floatingHint.Position = new CGPoint(floatingHintXPostion, floatingHintYPostion);
        }
        //Scrolled
        private void RSUITextView_Scrolled(object sender, EventArgs e)
        {
            var height = this.Frame.Height - bottomSpacing - floatingHintYPostionFloating - floatinghHintSizeFloating.Height / 2;
            mask.Frame = new CGRect(0, floatingHintYPostionFloating + floatinghHintSizeFloating.Height / 2 + this.ContentOffset.Y, Frame.Width, height);
        }


        //Create Border Container
        private void CreateBorderContainer()
        {
            borderContainer = new UIView();
            borderContainer.UserInteractionEnabled = false;
            this.BackgroundColor = UIColor.Clear;
        }
        //Rounded Border
        private void CreateRoundedBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = rSControl.BorderFillColor.ToCGColor(),
                CornerRadius = this.borderRadius,
                ZPosition = -1// So its behind floating label
            };

            if (rSControl.ShadowEnabled)
            {
                border.ShadowColor = rSControl.ShadowColor.ToCGColor();
                border.ShadowOpacity = 0.5f;
                border.ShadowRadius = this.shadowRadius;
                border.ShadowOffset = new CGSize(0f, 0.5f);
            }

            //this.Layer.AddSublayer(border);
            this.borderContainer.Layer.AddSublayer(border);
        }
        //Filled Border
        private void CreateFilledBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = rSControl.BorderFillColor.ToCGColor(),
                ZPosition = -1 // So its behind floating label
            };

            filledBorder = new CALayer()
            {
                BackgroundColor = rSControl.BorderFillColor.ToCGColor(),
                CornerRadius = this.borderRadius,
                ZPosition = -2 // So its behind floating label
            };

            filledBorder.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;

            if (rSControl.ShadowEnabled)
            {
                filledBorder.ShadowColor = rSControl.ShadowColor.ToCGColor();
                filledBorder.ShadowOpacity = 0.5f;
                filledBorder.ShadowRadius = this.shadowRadius;
                filledBorder.ShadowOffset = new CGSize(0f, 0.5f);
            }

            this.borderContainer.Layer.AddSublayer(border);
            this.borderContainer.Layer.AddSublayer(filledBorder);
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

            //this.BackgroundColor = rSControl.BorderFillColor.ToUIColor();

            filledBorder = new CALayer()
            {
                BackgroundColor = rSControl.BorderFillColor.ToCGColor(),
                ZPosition = -2 // So its behind floating label
            };

            if (rSControl.ShadowEnabled)
            {
                filledBorder.ShadowColor = rSControl.ShadowColor.ToCGColor();
                filledBorder.ShadowOpacity = 0.5f;
                filledBorder.ShadowRadius = this.shadowRadius;
                filledBorder.ShadowOffset = new CGSize(0f, 0.5f);
            }


            this.borderContainer.Layer.AddSublayer(border);
            this.borderContainer.Layer.AddSublayer(filledBorder);
        }
        //Create floatingHint
        private void CreateFloatingHint()
        {
            floatingHint = new CustomCATextLayer(rSControl.Placeholder,
                                                 rSControl.PlaceholderStyle.FontFamily != null ? rSControl.PlaceholderStyle.FontFamily : this.Font.FamilyName);

            //Init floatinhHint sizes
            floatingHint.FontSize = floatingFontSize;
            floatinghHintSizeFloating = floatingHint.Size;
            floatingHint.FontSize = (nfloat)this.rSControl.FontSize;
            floatinghHintSizeNotFloating = floatingHint.Size;

            floatingHint.AllowsEdgeAntialiasing = true;
            floatingHint.ContentsScale = UIScreen.MainScreen.Scale;
            floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor();
            floatingHint.BackgroundColor = UIColor.Clear.CGColor;
            floatingHint.Wrapped = false;

            floatingHint.Bounds = new CGRect(0.0f, 0.0f, floatingHint.Size.Width, floatingHint.Size.Height);

            this.borderContainer.Layer.AddSublayer(floatingHint);
        }
        //Create error label
        private void CreateErrorLabel()
        {
            errorLabel = new CustomCATextLayer(this.ErrorMessage,
                                               rSControl.ErrorStyle.FontFamily != null ? rSControl.ErrorStyle.FontFamily : this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = UIColor.SystemRedColor.CGColor,
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false
            };

            this.borderContainer.Layer.AddSublayer(errorLabel);
        }
        //Create helper label
        private void CreateHelperLabel()
        {
            helperLabel = new CustomCATextLayer(rSControl.Helper,
                                                rSControl.HelperStyle.FontFamily != null ? rSControl.HelperStyle.FontFamily : this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = rSControl.HelperStyle.FontColor.ToCGColor(),
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false
            };

            helperLabel.Bounds = new CGRect(0.0f, 0.0f, helperLabel.Size.Width, helperLabel.Size.Height);
            this.borderContainer.Layer.AddSublayer(helperLabel);
        }
        //Create helper label
        private void CreateCounterLabel()
        {
            counterLabel = new CustomCATextLayer("", rSControl.CounterStyle.FontFamily != null ? rSControl.CounterStyle.FontFamily : this.Font.FamilyName)
            {
                FontSize = 12,
                ForegroundColor = rSControl.CounterStyle.FontColor.ToCGColor(),
                Opacity = 1.0f,
                ContentsScale = UIScreen.MainScreen.Scale,
                AllowsEdgeAntialiasing = true,
                Wrapped = false
            };

            this.borderContainer.Layer.AddSublayer(counterLabel);
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
                counterLabel.ForegroundColor = rSControl.CounterStyle.FontColor.ToCGColor();

                if (string.IsNullOrEmpty(this.ErrorMessage) && this.IsFirstResponder)
                {
                    this.border.BorderColor = this.activeColor.CGColor;
                    this.floatingHint.ForegroundColor = this.activeColor.CGColor;
                    this.errorEnabled = false;
                }
                else if (string.IsNullOrEmpty(this.ErrorMessage) && !this.IsFirstResponder)
                {
                    this.border.BorderColor = this.borderColor.CGColor;
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor();
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
             filledBorder.Frame = new CGRect(0, topSpacing, this.Frame.Width, this.Frame.Height - this.bottomSpacing);
             border.Frame = new CGRect(0, topSpacing + this.Frame.Height - this.bottomSpacing - border.BorderWidth, this.Frame.Width, border.BorderWidth);
         }
        //Set Underline border frame
        public void UpdateUnderlineBorder()
        {
            //Border frame
            filledBorder.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height - this.bottomSpacing + topSpacing);
            border.Frame = new CGRect(0, this.Frame.Height - this.bottomSpacing + topSpacing - border.BorderWidth, this.Frame.Width, border.BorderWidth);
        }


        //Only set at start to place hint
        private void FloatingHintFramePlacement()
        {
            floatingHint.Position = new CGPoint(floatingHintXPostion, floatingHintYPostion);
        }
        //Update floating hint
        public void UpdateBorder()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                UpdateOutlinedBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                UpdateFilledBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                UpdateUnderlineBorder();
        }
        private void UpdateHelper()
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            helperLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                               this.Frame.Height - helperLabel.Size.Height / 2 - 1);
            CATransaction.Commit();
        }
        private void UpdateError()
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            errorLabel.Position = new CGPoint(this.leftPadding + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                               this.Frame.Height - errorLabel.Size.Height / 2 - 1);
            CATransaction.Commit();
        }


        //Device orientation change event
        private void DeviceRotated(NSNotification notification)
        {
            //Reset border frame and mask
            //if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.PortraitUpsideDown)
            // SetBorder();

            SetNeedsDisplay();
        }


        //Draw
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            //Update border without animation
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            UpdateBorder();
            CATransaction.Commit();


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
                    floatingHintYPostionFloating = topSpacing + floatinghHintSizeFloating.Height + 3;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2;
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
                    UpdateError();

                if (helperLabel != null)
                    UpdateHelper();

                //if (counterLabel != null)
                //    counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - leftRightSpacingLabels - counterLabel.Size.Width / 2,
                //                                       this.Frame.Height - counterLabel.Size.Height / 2 - 1);


                //Init here because text property not yet set in create floatinghint method
                FloatingHintFramePlacement();

                borderContainer.Frame = new CGRect(0, 0, this.Frame.Width, this.Frame.Height);
                this.Superview.AddSubview(borderContainer);
                borderContainer.Layer.ZPosition = -1;


                if ((rSControl as RSEditor).AutoSize == EditorAutoSizeOption.Disabled || (rSControl as RSEditor).HeightRequest != -1)
                {
                    mask = new UIView();
                    var height = this.Frame.Height - bottomSpacing - floatingHintYPostionFloating - floatinghHintSizeFloating.Height / 2;
                    mask.Frame = new CGRect(0, floatingHintYPostionFloating + floatinghHintSizeFloating.Height / 2 + this.ContentOffset.Y, Frame.Width, height);
                    mask.BackgroundColor = UIColor.Purple;
                    //this.TextInputView.BackgroundColor = UIColor.Yellow;
                    this.TextInputView.MaskView = mask;
                }

                
                hasInitfinished = true;


                UpdateBorder();
            }

            //Update error
            if (errorLabel != null)
                UpdateError();

            //Update helper
            if (helperLabel != null)
                UpdateHelper();

            //Counter
            if (this.counterMaxLength != -1)
                SetCounter();
        }

        //Update ui 
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            UpdateBorder();
        }


        //Remove any events when closed
        protected override void Dispose(bool disposing)
        {
            //Remove observer
            NSNotificationCenter.DefaultCenter.RemoveObserver(deviceRotationObserver);

            //Remove events
            this.Started -= RSUITextView_Started;
            this.Ended -= RSUITextView_Ended;
            this.Changed -= RSUITextView_EditingChanged;
            if ((rSControl as RSEditor).AutoSize == EditorAutoSizeOption.Disabled || (rSControl as RSEditor).HeightRequest != -1)
                this.Scrolled -= RSUITextView_Scrolled;

            base.Dispose(disposing);
        }
    }
}
