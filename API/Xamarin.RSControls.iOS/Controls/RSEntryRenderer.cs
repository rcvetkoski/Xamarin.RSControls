using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using Xamarin.RSControls.Helpers;
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

            ////Delete placeholder as we use floating hint instead
            //Element.Placeholder = "";


            var frame = this.Frame;

            var sizeRequest = (Element as Forms.View).Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);

            var lol = this.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = (this.Element as RSEntry).Error;
            }

            if(e.PropertyName == "Text" && !(sender as Forms.View).IsFocused)
                (this.Control as RSUITextField).UpdateView();
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

        //Shadow
        private nfloat borderRadius;
        private nfloat shadowRadius;
        private CGPath borderShadowPath;

        //Border
        private CALayer border;
        private CALayer filledBorder;

        //Labels
        private CustomCATextLayer floatingHint;
        private CustomCATextLayer errorLabel;
        private CustomCATextLayer helperLabel;
        private CustomCATextLayer counterLabel;
        private nfloat leftRightSpacingLabels;

        //Icons
        private UIView leftIconSeparatorView;
        private nfloat leftIconSeparatorViewWidth;
        private UIView rightIconSeparatorView;
        private nfloat rightIconSeparatorViewWidth;
        private nfloat iconPadding;
        private UIView leftView;
        private nfloat leftViewWidth;
        private UIView leftHelpingView;
        private nfloat leftHelpingViewWidth;
        private UIView leadingView;
        private nfloat leadingViewWidth;
        private UIView trailingView;
        private nfloat trailingViewWidth;
        private UIView rightHelpingView;
        private nfloat rightHelpingViewWidth;
        private UIView rightView;
        private nfloat rightViewWidth;
        private nfloat leftHelpingIconPadding;
        private nfloat rightHelpingIconPadding;

        //Counter
        private int counterMaxLength;
        private int counter;

        //Floating hint position and size
        private bool IsFloating;
        private bool hasInitfinished = false;
        private nfloat floatingHintXPosition;
        private nfloat floatingHintXPositionFloating;
        private nfloat floatingHintXPositionNotFloating;
        private nfloat floatingHintYPosition;
        private nfloat floatingHintYPositionFloating;
        private nfloat floatingHintYPostionNotFloating;
        private CGSize floatinghHintSizeFloating;
        private CGSize floatingHintSizeNotFloating;
        private int floatingHintClipPadding;
        private nfloat floatingFontSize;
        private nfloat maxIconHeight = 0;
        private nfloat requiredWidth;
        private nfloat requiredHeight;


        //Error
        private bool errorEnabled;
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
                        helperLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                           this.Frame.Height);
                    }

                    errorLabel.Opacity = 1.0f;
                    errorLabel.Bounds = new CGRect(0.0f, 0.0f, errorLabel.Size.Width, errorLabel.Size.Height);
                    errorLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + errorLabel.Size.Width / 2,
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
                        helperLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                           this.Frame.Height - helperLabel.Size.Height / 2 - 1);
                    }

                    errorLabel.Opacity = 0.0f;
                    errorLabel.Bounds = new CGRect(0.0f, 0.0f, errorLabel.Size.Width, errorLabel.Size.Height);
                    errorLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + errorLabel.Size.Width / 2,
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

        //Padding values
        private CGRect borderPosition;
        private nfloat textSpacingFromBorderTop;
        private nfloat textSpacingFromBorderBottom;
        private nfloat topPadding;
        private nfloat bottomPadding;
        private nfloat leftPadding;
        private nfloat rightPadding;
        private Thickness padding;

        //Store temp frame size and check if changed in layout subviews, if changed => screen has been probably rotated => update ui
        private CGRect tempSize;

        //Constructor
        public RSUITextField(IRSControl rSControl)
        {
            this.rSControl = rSControl;

            this.ClipsToBounds = true;

            if (rSControl.FontSize < 12)
                this.floatingFontSize = (nfloat)rSControl.FontSize;
            else
                this.floatingFontSize = 12;

            floatingHintClipPadding = !string.IsNullOrEmpty(rSControl.Placeholder) ? 3 : 0;
            this.borderRadius = rSControl.BorderRadius;
            this.shadowRadius = rSControl.ShadowRadius;
            this.counterMaxLength = this.rSControl.CounterMaxLength;
            this.borderColor = this.rSControl.BorderColor.ToUIColor();
            this.borderWidth = rSControl.BorderWidth;
            this.borderWidthFocused = rSControl.BorderWidth + 1;
            this.activeColor = UIColor.SystemBlueColor;
            this.textSpacingFromBorderTop = 8;
            this.textSpacingFromBorderBottom = 14;
            leftRightSpacingLabels = 13;
            this.iconPadding = 8;
            leftHelpingIconPadding = rSControl.LeftHelpingIcon != null ? 10 + iconPadding : iconPadding;
            rightHelpingIconPadding = rSControl.RightHelpingIcon != null ? 10 + iconPadding : iconPadding;

            //Padding
            padding = new Thickness(8, 17);
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


            //Create Floating hint
            CreateFloatingHint();

            //Create border
            CreateBorder();

            //SetIcons
            CreateIcons();

            //Edit text events
            this.Started += RSUITextField_Started;
            this.Ended += RSUITextField_Ended;
            this.EditingChanged += RSUITextField_EditingChanged;

            //AdjustSize();
        }

        //Adjust Size if needed
        private void AdjustSize()
        {
            var sizeRequestNative = this.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

            //Width
            int errorLabelWidth = 0;
            int helperLabelWidth = helperLabel != null ? (int)helperLabel.Size.Width : 0;
            int counterLabelWidth =  counterLabel != null ? (int)counterLabel.Size.Width + (int)iconPadding * 2: 0;
            int labelsWidth;

            foreach (var behavior in (rSControl as Forms.View).Behaviors)
            {
                if (behavior is Validators.ValidationBehaviour)
                {
                    foreach (var iValidator in (behavior as Validators.ValidationBehaviour).Validators)
                    {
                        //errorLabel.String = iValidator.Message;
                        //if (errorLabelWidth < errorLabel.Size.Width)
                        //    errorLabelWidth = (int)errorLabel.Size.Width;
                    }
                }
            }

            if (helperLabelWidth > errorLabelWidth)
                labelsWidth = (int)(helperLabelWidth + counterLabelWidth + leadingViewWidth + trailingViewWidth + leftRightSpacingLabels * 2 + leftViewWidth + rightViewWidth);
            else
                labelsWidth = (int)(errorLabelWidth + counterLabelWidth + leadingViewWidth + trailingViewWidth + leftRightSpacingLabels * 2 + leftViewWidth + rightViewWidth);


            if (labelsWidth > floatingHintSizeNotFloating.Width + leftPadding + rightPadding + leftViewWidth)
                requiredWidth = labelsWidth;
            else
                requiredWidth = (int)(floatingHintSizeNotFloating.Width + leftPadding + rightPadding);


            //Height
            requiredHeight = maxIconHeight + textSpacingFromBorderTop + textSpacingFromBorderBottom + iconPadding * 2;

            if(requiredHeight > sizeRequestNative.Request.Height)
            {
                (rSControl as Forms.View).HeightRequest = requiredHeight;
            }
            if (requiredWidth > sizeRequestNative.Request.Width)
            {
                (rSControl as Forms.View).WidthRequest = requiredWidth;
            }
        }

        //Set Padding
        private void SetPadding()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                this.leftPadding = (nfloat)padding.Left + (nfloat)rSControl.Padding.Left;
                this.rightPadding = (nfloat)padding.Right + (nfloat)rSControl.Padding.Right;
                this.topPadding = (nfloat)padding.Top + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = (nfloat)padding.Bottom + (textSpacingFromBorderBottom - textSpacingFromBorderTop) + (nfloat)rSControl.Padding.Bottom;
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.leftPadding = (nfloat)padding.Left + (nfloat)rSControl.Padding.Left;
                this.rightPadding = (nfloat)padding.Right + (nfloat)rSControl.Padding.Right;
                this.topPadding = (nfloat)padding.Top + (nfloat)rSControl.Padding.Top; // 25
                this.bottomPadding = (nfloat)padding.Bottom + (nfloat)rSControl.Padding.Bottom; // 20
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
            {
                this.leftPadding = (nfloat)padding.Left + (nfloat)rSControl.Padding.Left;
                this.rightPadding = (nfloat)padding.Right + (nfloat)rSControl.Padding.Right;
                this.topPadding = (nfloat)padding.Top + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = (nfloat)padding.Bottom + textSpacingFromBorderTop / 2 + (nfloat)rSControl.Padding.Bottom; // Add arbitrary padding value to align it better
            }
        }


        //Set Icons
        private UIView CreateIcon(RSEntryIcon icon)
        {
            UIView convertedView = Extensions.ViewExtensions.ConvertFormsToNative(icon.View, CGRect.Empty);
            this.AddSubview(convertedView);
            UITapGestureRecognizer gest = new UITapGestureRecognizer(() => { CreateAndExecuteIconMethod(icon); AddRippleEffect(convertedView); });
            convertedView.AddGestureRecognizer(gest);

            //Set max height of icon so we can resize RSEntry if necessary
            if (maxIconHeight < convertedView.Frame.Height)
                maxIconHeight = convertedView.Frame.Height;

            return convertedView;
        }
        private void CreateIcons()
        {
            if (this.rSControl.LeadingIcon != null)
            {
                leadingView = CreateIcon(rSControl.LeadingIcon);
                leadingViewWidth = leadingView.Frame.Width + iconPadding;
            }
            if (this.rSControl.LeftIcon != null)
            {
                leftView = CreateIcon(rSControl.LeftIcon);
                leftViewWidth = rSControl.LeftHelpingIcon != null ? leftView.Frame.Width + leftHelpingIconPadding : leftView.Frame.Width + iconPadding;

                if (this.rSControl.LeftHelpingIcon != null)
                {
                    leftHelpingView = CreateIcon(rSControl.LeftHelpingIcon);
                    leftHelpingViewWidth = leftHelpingView.Frame.Width + iconPadding;
                }
            }
            if (rSControl.LeftIcon != null && rSControl.LeftHelpingIcon != null)
            {
                if(rSControl.HasLeftIconSeparator)
                {
                    leftIconSeparatorView = new UIView(new CGRect(leftPadding + leadingViewWidth + leftViewWidth / 2, 0, 1, 22)) { BackgroundColor = UIColor.LightGray };
                    this.AddSubview(leftIconSeparatorView);
                }

                leftIconSeparatorViewWidth = 1;
            }
            /////////////////////////////////////////////////////////////////////////
            if (rSControl.RightIcon != null && rSControl.RightHelpingIcon != null)
            {
                if(rSControl.HasRighIconSeparator)
                {
                    rightIconSeparatorView = new UIView(new CGRect(0, 0, 1, 22)) { BackgroundColor = UIColor.LightGray };
                    this.AddSubview(rightIconSeparatorView);
                }

                rightIconSeparatorViewWidth = 1;
            }
            if (rSControl.RightIcon != null)
            {
                //RightIcon
                rightView = CreateIcon(rSControl.RightIcon);
                rightViewWidth = rSControl.RightHelpingIcon != null ? rightView.Frame.Width + rightHelpingIconPadding : rightView.Frame.Width + iconPadding;

                //RightHelpingIcon
                if (rSControl.RightHelpingIcon != null)
                {
                    rightHelpingView = CreateIcon(rSControl.RightHelpingIcon);
                    rightHelpingViewWidth = rightHelpingView.Frame.Width + iconPadding;
                }
            }
            if (this.rSControl.TrailingIcon != null)
            {
                trailingView = CreateIcon(rSControl.TrailingIcon);
                trailingViewWidth = trailingView.Frame.Width + iconPadding;
            }
        }
        //Update Icons position
        private void UpdateIconPosition(UIView view, nfloat left)
        {
            view.Frame = new CGRect(left,
                                    floatingHintYPostionNotFloating - view.Frame.Height / 2,
                                    view.Frame.Width,
                                    view.Frame.Height);
        }
        private void UpdateIconsPosition()
        {
            if (leadingView != null)
                UpdateIconPosition(leadingView, left: 0);

            if (leftView != null)
                UpdateIconPosition(leftView, left: leftPadding + leadingViewWidth);

            if(leftIconSeparatorView != null)
                UpdateIconPosition(leftIconSeparatorView, left: leftPadding + leadingViewWidth + leftViewWidth - leftHelpingIconPadding / 2);

            if (leftHelpingView != null)
                UpdateIconPosition(leftHelpingView, left: leftPadding + leadingViewWidth + leftViewWidth);

            /////////////////////////////////////////////////////

            if (rightHelpingView != null)
                UpdateIconPosition(rightHelpingView, left: Frame.Right - rightPadding - trailingViewWidth - rightViewWidth - rightHelpingViewWidth - rightIconSeparatorViewWidth - rightHelpingIconPadding + iconPadding + rightHelpingIconPadding);

            if (rightIconSeparatorView != null)
                UpdateIconPosition(rightIconSeparatorView, left: Frame.Right - rightPadding - trailingViewWidth - rightViewWidth - rightIconSeparatorViewWidth + rightHelpingIconPadding / 2);

            if (rightView != null)
                UpdateIconPosition(rightView, left: Frame.Right - rightPadding - trailingViewWidth - rightViewWidth + rightHelpingIconPadding);

            if (trailingView != null)
                UpdateIconPosition(trailingView, left: Frame.Right - trailingViewWidth + iconPadding);
        }
        //Icon tap method
        private void CreateAndExecuteIconMethod(RSEntryIcon rsIcon)
        {
            if (rsIcon.Source == null)
                rsIcon.Source = (rSControl as Forms.View).BindingContext;

            MethodInfo methodInfo;

            if (rsIcon.Bindings.Any())
            {
                List<object> objects = new List<object>();
                foreach (RSCommandParameter rSCommandParameter in rsIcon.Bindings)
                {
                    //objects.Add(GetDeepPropertyValue(binding.Source, binding.Path));
                    objects.Add(rSCommandParameter.CommandParameter);
                }

                Type[] types = new Type[rsIcon.Bindings.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    types[i] = objects[i].GetType();
                }

                methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, types);
                ExecuteCommand(methodInfo, rsIcon.Source, objects.ToArray());
            }
            else if (rsIcon.CommandParameter != null)
            {
                methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, new Type[] { rsIcon.CommandParameter.GetType() });
                ExecuteCommand(methodInfo, rsIcon.Source, rsIcon.CommandParameter);
            }
            else
            {
                if (rsIcon.Command != null)
                {
                    methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, new Type[] { });
                    ExecuteCommand(methodInfo, rsIcon.Source, rsIcon.CommandParameter);
                }
            }
        }
        private void ExecuteCommand(MethodInfo methodInfo, object source, object[] parameters)
        {
            if (methodInfo != null)
            {
                if (parameters.Any())
                {
                    methodInfo.Invoke(source, parameters);
                }
                else
                    methodInfo.Invoke(source, null);
            }
        }
        private void ExecuteCommand(MethodInfo methodInfo, object source, object parameter)
        {
            if (methodInfo != null)
            {
                if (parameter != null)
                {
                    methodInfo.Invoke(source, new object[] { parameter });
                }
                else
                    methodInfo.Invoke(source, null);
            }
        }
        //Add ripple effect to icon click
        private void AddRippleEffect(UIView uIView)
        {
            var path = UIBezierPath.FromOval(new CGRect(0, 0, uIView.Bounds.Size.Width, uIView.Bounds.Size.Height));

            var shapePosition = new CGPoint(uIView.Bounds.Size.Width / 2, uIView.Bounds.Size.Height / 2);

            var rippleShape = new CAShapeLayer();
            rippleShape.Bounds = new CGRect(0, 0, uIView.Bounds.Size.Width, uIView.Bounds.Size.Height);
            rippleShape.Path = path.CGPath;
            rippleShape.FillColor = UIColor.LightGray.CGColor;
            rippleShape.StrokeColor = UIColor.LightGray.CGColor;
            rippleShape.LineWidth = 1;
            rippleShape.Position = shapePosition;
            rippleShape.Opacity = 0;

            uIView.Layer.AddSublayer(rippleShape);

            var scaleAnim = CABasicAnimation.FromKeyPath("transform.scale");
            scaleAnim.SetFrom(NSValue.FromCATransform3D(CATransform3D.Identity));
            scaleAnim.SetTo(NSValue.FromCATransform3D(CATransform3D.MakeScale(2, 2, 1)));

            var opacityAnim = CABasicAnimation.FromKeyPath("opacity");
            opacityAnim.SetFrom(NSNumber.FromDouble(1));
            opacityAnim.SetTo(NSNumber.FromDouble(0));

            var animation = new CAAnimationGroup(); 
            animation.Animations = new CAAnimation[] { scaleAnim, opacityAnim };
            animation.TimingFunction = CAMediaTimingFunction.FromName(new NSString(CAMediaTimingFunction.EaseOut));
            animation.Duration = 0.4;
            
            animation.RepeatCount = 1;
            animation.RemovedOnCompletion = true;

            rippleShape.AddAnimation(animation, "rippleEffect");
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

            floatingHintXPosition = floatingHintXPositionFloating;
            floatingHintYPosition = floatingHintYPositionFloating;

            AnimateFloatingHint(floatingFontSize);

            UpdateBorder();
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
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor();
                else
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderColor.ToCGColor();

                this.border.BorderColor = this.borderColor.CGColor;
            }

            floatingHintXPosition = floatingHintXPositionNotFloating;
            floatingHintYPosition = floatingHintYPostionNotFloating;

            if (!IsFloating)
                AnimateFloatingHint(rSControl.FontSize);

            UpdateBorder();
        }
        //Floating hint Animation properties
        private void AnimateFloatingHint(double toValue)
        {
            //Animation is trigered automatically when changing this properties
            floatingHint.FontSize = (nfloat)toValue;
            floatingHint.Position = new CGPoint(floatingHintXPosition, floatingHintYPosition);
        }


        //Create border
        private void CreateBorder()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                CreateOutlinedBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                CreateFilledBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                CreateUnderlineBorder();
        }
        private void CreateOutlinedBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = rSControl.BorderFillColor == Forms.Color.Default ? UIColor.Clear.CGColor : rSControl.BorderFillColor.ToCGColor(),
                CornerRadius = this.borderRadius,
                ZPosition = -1 // So its behind floating label
            };

            if (rSControl.ShadowEnabled)
            {
                border.ShadowColor = rSControl.ShadowColor.ToCGColor();
                border.ShadowOpacity = 0.5f;
                border.ShadowRadius = this.shadowRadius;
                border.ShadowOffset = new CGSize(0f, this.borderWidth);
            }
            
            this.Layer.AddSublayer(border);
        }
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
                BackgroundColor = rSControl.BorderFillColor == Forms.Color.Default ? Forms.Color.FromHex("#OA000000").ToCGColor() : rSControl.BorderFillColor.ToCGColor(),
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


            this.Layer.AddSublayer(border);
            this.Layer.AddSublayer(filledBorder);
        }
        private void CreateUnderlineBorder()
        {
            border = new CALayer()
            {
                BorderColor = this.borderColor.CGColor,
                BorderWidth = this.borderWidth,
                BackgroundColor = UIColor.Gray.CGColor,
                ZPosition = -1 // So its behind floating label
            };

            //this.BackgroundColor = rSControl.BorderFillColor.ToUIColor();

            filledBorder = new CALayer()
            {
                BackgroundColor = rSControl.BorderFillColor == Forms.Color.Default ? Forms.Color.FromHex("#OA000000").ToCGColor() : rSControl.BorderFillColor.ToCGColor(),
                ZPosition = -2 // So its behind floating label
            };

            if (rSControl.ShadowEnabled)
            {
                filledBorder.ShadowColor = rSControl.ShadowColor.ToCGColor();
                filledBorder.ShadowOpacity = 0.5f;
                filledBorder.ShadowRadius = this.shadowRadius;
                filledBorder.ShadowOffset = new CGSize(0f, 0.5f);
            }

            this.Layer.AddSublayer(border);
            this.Layer.AddSublayer(filledBorder);
        }
        //Update border
        private void UpdateBorder()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
                UpdateOutlinedBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
                UpdateFilledBorder();
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
                UpdateUnderlineBorder();
        }
        private void UpdateOutlinedBorder()
        {
            var frame = this.Frame;

            //border
            borderPosition.X = leadingViewWidth;
            borderPosition.Y = textSpacingFromBorderTop;
            borderPosition.Width = frame.Width - leadingViewWidth - trailingViewWidth;
            borderPosition.Height = frame.Height - textSpacingFromBorderTop - textSpacingFromBorderBottom;


            //Border frame
            border.Frame = new CGRect(x: borderPosition.X, y: borderPosition.Y, width: borderPosition.Width, height: borderPosition.Height);


            CAShapeLayer borderMask = new CAShapeLayer();
            CGPath maskPath = new CGPath();


            //Whole border except top (border width)
            maskPath.AddRect(new CGRect(x: 0, y: this.borderWidth + floatinghHintSizeFloating.Height / 2, width: frame.Width, height: frame.Height));

            //Top border right of floating label area
            maskPath.AddRect(new CGRect(x: leftRightSpacingLabels + floatinghHintSizeFloating.Width + floatingHintClipPadding,
                                        y: 0,
                                        width: frame.Width - floatingHintClipPadding,
                                        height: frame.Height));

            //Left border
            maskPath.AddRect(new CGRect(x: 0, y: 0, leftRightSpacingLabels - floatingHintClipPadding, frame.Height));

            //Top border libne full
            if (!IsFloating)
                maskPath.AddRect(new CGRect(x: 0, y: 0, width: frame.Width, height: frame.Height));


            //borderMask.FillRule = new NSString("kCAFillRuleEvenOdd");
            borderMask.Path = maskPath;
            this.border.Mask = borderMask;
        }
        private void UpdateFilledBorder()
        {
            var frame = this.Frame;

            //border
            borderPosition.X = leadingViewWidth;
            borderPosition.Y = 0;
            borderPosition.Width = frame.Width - leadingViewWidth - trailingViewWidth;
            borderPosition.Height = frame.Height - textSpacingFromBorderBottom;

            //Border frame
            filledBorder.Frame = new CGRect(borderPosition.X, borderPosition.Y, borderPosition.Width, borderPosition.Height);
            border.Frame = new CGRect(borderPosition.X, borderPosition.Y + borderPosition.Height - border.BorderWidth, borderPosition.Width, border.BorderWidth); //Underline
        }
        private void UpdateUnderlineBorder()
        {
            var frame = this.Frame;

            //border
            borderPosition.X = leadingViewWidth;
            borderPosition.Y = 0;
            borderPosition.Width = frame.Width - leadingViewWidth - trailingViewWidth;
            borderPosition.Height = frame.Height - textSpacingFromBorderBottom;

            //Border frame
            filledBorder.Frame = new CGRect(borderPosition.X, borderPosition.Y, borderPosition.Width, borderPosition.Height);
            border.Frame = new CGRect(borderPosition.X, borderPosition.Y + borderPosition.Height - border.BorderWidth, borderPosition.Width, border.BorderWidth); //Underline
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
            floatingHintSizeNotFloating = floatingHint.Size;

            floatingHint.AllowsEdgeAntialiasing = true;
            floatingHint.ContentsScale = UIScreen.MainScreen.Scale;
            floatingHint.ForegroundColor = rSControl.PlaceholderColor.ToCGColor();
            floatingHint.BackgroundColor = UIColor.Clear.CGColor;
            floatingHint.Wrapped = false;

            floatingHint.Bounds = new CGRect(0.0f, 0.0f, floatingHint.Size.Width, floatingHint.Size.Height);

            this.Layer.AddSublayer(floatingHint);
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


            this.Layer.AddSublayer(errorLabel);
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
            this.Layer.AddSublayer(helperLabel);
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
            counterLabel.String = string.Format("{0}/{1}", this.counter, this.counterMaxLength);

            this.Layer.AddSublayer(counterLabel);


            bool existingBehaviour = false;

            foreach (var behavior in (rSControl as Forms.View).Behaviors)
            {
                if (behavior is Validators.ValidationBehaviour)
                {
                    if ((behavior as Validators.ValidationBehaviour).PropertyName == "Text")
                    {
                        (behavior as Validators.ValidationBehaviour).Validators.Add(new Validators.CounterValidation() { CounterMaxLength = rSControl.CounterMaxLength });
                        existingBehaviour = true;
                    }
                }
            }

            if (!existingBehaviour)
            {
                Validators.ValidationBehaviour counterValidationBehaviour = new Validators.ValidationBehaviour() { PropertyName = "Text" };
                counterValidationBehaviour.Validators.Add(new Validators.CounterValidation() { CounterMaxLength = rSControl.CounterMaxLength });
                (rSControl as Forms.View).Behaviors.Add(counterValidationBehaviour);
            }
        }
        //Set Counter
        private void SetCounter()
        {
            this.counter = this.Text.Length;
            counterLabel.String = string.Format("{0}/{1}", this.counter, this.counterMaxLength);

            counterLabel.Bounds = new CGRect(0.0f, 0.0f, counterLabel.Size.Width, counterLabel.Size.Height);
            counterLabel.Position = new CGPoint(this.Frame.Width - trailingViewWidth - leftRightSpacingLabels - counterLabel.Size.Width / 2,
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
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderColor.ToCGColor();
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
        //Update counter position
        private void UpdateCounterPosition()
        {
            counterLabel.Bounds = new CGRect(0.0f, 0.0f, counterLabel.Size.Width, counterLabel.Size.Height);
            counterLabel.Position = new CGPoint(this.Frame.Width - trailingViewWidth - leftRightSpacingLabels - counterLabel.Size.Width / 2,
                                                this.Frame.Height - counterLabel.Size.Height / 2 - 1);
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


        //Upade floating hint position coordinates
        private void floatingHintPositionUpdate()
        {
            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                //X
                floatingHintXPositionFloating = this.TextRect(Bounds).X + floatingHintSizeNotFloating.Width / 2;
                if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.Center)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).GetMidX();
                else if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.End)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Right - floatingHintSizeNotFloating.Width / 2;
                else
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Left + floatingHintSizeNotFloating.Width / 2;


                //Y
                floatingHintYPositionFloating = this.TextRect(this.Bounds).GetMidY() - textSpacingFromBorderBottom / 2 - floatinghHintSizeFloating.Height / 2;
                floatingHintYPostionNotFloating = this.TextRect(this.Bounds).GetMidY() - textSpacingFromBorderBottom / 2;
            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                //X
                floatingHintXPositionFloating = leadingViewWidth + floatingHintSizeNotFloating.Width / 2 + leftRightSpacingLabels;
                if(rSControl.HorizontalTextAlignment == Forms.TextAlignment.Center)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).GetMidX();
                else if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.End)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Right - floatingHintSizeNotFloating.Width / 2;
                else
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Left + floatingHintSizeNotFloating.Width / 2;


                //Y
                floatingHintYPositionFloating = borderPosition.Y + (floatingHintSizeNotFloating.Height - floatinghHintSizeFloating.Height) / 2;
                floatingHintYPostionNotFloating = this.TextRect(this.Bounds).GetMidY();
            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                //X
                floatingHintXPositionFloating = this.TextRect(Bounds).X + floatingHintSizeNotFloating.Width / 2;
                if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.Center)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).GetMidX();
                else if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.End)
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Right - floatingHintSizeNotFloating.Width / 2;
                else
                    floatingHintXPositionNotFloating = this.TextRect(this.Bounds).Left + floatingHintSizeNotFloating.Width / 2;


                //Y
                floatingHintYPositionFloating = this.TextRect(this.Bounds).GetMidY() - textSpacingFromBorderBottom / 2 - floatinghHintSizeFloating.Height / 2;
                floatingHintYPostionNotFloating = this.TextRect(this.Bounds).GetMidY();
            }


            if (IsFloating)
            {
                floatingHint.FontSize = floatingFontSize;
                floatingHintXPosition = floatingHintXPositionFloating;
                floatingHintYPosition = floatingHintYPositionFloating;
            }
            else
            {
                floatingHint.FontSize = (nfloat)this.rSControl.FontSize;
                floatingHintXPosition = floatingHintXPositionNotFloating;
                floatingHintYPosition = floatingHintYPostionNotFloating;
            }
        }
        //Only set at start to place hint
        private void FloatingHintFramePlacement()
        {
            floatingHint.Position = new CGPoint(floatingHintXPosition, floatingHintYPosition);
        }
        //Used by external classes
        private void ForceFloatingHintFloatOrNot()
        {
            if (!string.IsNullOrEmpty(this.Text) && !IsFloating)
            {
                this.IsFloating = true;
                floatingHintXPosition = floatingHintXPositionFloating;
                floatingHintYPosition = floatingHintYPositionFloating;
                AnimateFloatingHint(floatingFontSize);
            }
            else if (string.IsNullOrEmpty(this.Text) && !errorEnabled && IsFloating)
            {
                this.IsFloating = false;
                floatingHintXPosition = floatingHintXPositionNotFloating;
                floatingHintYPosition = floatingHintYPostionNotFloating;
                AnimateFloatingHint(this.rSControl.FontSize);
            }
        }


        //Helper for padding
        private UIEdgeInsets SetInsetRect()
        {
            nfloat left = leftPadding + leadingViewWidth + leftViewWidth + leftHelpingViewWidth;
            nfloat top = topPadding;
            nfloat bottom = bottomPadding;
            nfloat right = rightPadding + trailingViewWidth + rightViewWidth + rightHelpingViewWidth;

            return new UIEdgeInsets(top, left, bottom, right);
        }
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
            return InsetRect(base.TextRect(forBounds), SetInsetRect());
        }

        public override CGRect EditingRect(CGRect forBounds)
        {
            return InsetRect(base.EditingRect(forBounds), SetInsetRect());
        }
        //Placeholder padding
        public override CGRect PlaceholderRect(CGRect forBounds)
        {
            return InsetRect(base.PlaceholderRect(forBounds), SetInsetRect());
        }
        //// Not used -- Update cursor if needed
        //public override CGRect GetCaretRectForPosition(UITextPosition position)
        //{
        //    var cursorPosition = base.GetCaretRectForPosition(position);
        //    if(this.Text == string.Empty && this.TextAlignment == UITextAlignment.Center)
        //        cursorPosition.X = floatingHintXPositionNotFloating - leftPadding * 2;

        //    return cursorPosition;
        //}


        //Draw placeholder for intristic content size but give it transparent color
        public override NSAttributedString AttributedPlaceholder
        {
            get
            {
                if(base.AttributedPlaceholder != null)
                    return new NSAttributedString(base.AttributedPlaceholder.Value, null, UIColor.Clear);
                else
                    return new NSAttributedString("", null, UIColor.Clear);
            }
            set => base.AttributedPlaceholder = value;
        }

        //Draw
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
        }

        //Used in RSControl property changed so it update when per example text changes from external code or view
        public void UpdateView()
        {
            ForceFloatingHintFloatOrNot();
            UpdateBorder();
        }


        /// <summary>
        /// Update UI and handle rotation
        /// </summary>
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (tempSize != this.Frame)
            {
                //Init floatingHint X and Y values
                if (!hasInitfinished)
                {
                    tempSize = this.Frame;

                    //Set is floating hint or not
                    if (rSControl.IsPlaceholderAlwaysFloating)
                        IsFloating = true;
                    else if (this.IsFirstResponder || !string.IsNullOrEmpty(this.Text) || errorEnabled)
                        IsFloating = true;
                    else
                        IsFloating = false;


                    //Upade floating hint position coordinates
                    //floatingHintPositionUpdate();

                    if (errorLabel != null)
                        errorLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                                           this.Frame.Height - errorLabel.Size.Height / 2 - 1);

                    if (helperLabel != null)
                        helperLabel.Position = new CGPoint(leadingViewWidth + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                           this.Frame.Height - helperLabel.Size.Height / 2 - 1);

                    //Counter
                    if (this.counterMaxLength != -1)
                        SetCounter();


                    //Init here because text property not yet set in create floatinghint method
                    //FloatingHintFramePlacement();


                    //Update icons position if any
                    //UpdateIconsPosition();


                    hasInitfinished = true;
                }

                UpdateBorder();
                floatingHintPositionUpdate();
                FloatingHintFramePlacement();
                //UpdateBorder();
                UpdateIconsPosition();
                if (counterLabel != null)
                    UpdateCounterPosition();

                tempSize = this.Frame;
            }
        }

        /// <summary>
        /// Remove any events when closed
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
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
            if (hint == null)
                hint = string.Empty;

            this.familyName = familyName;

            //CTFontDescriptorAttributes cTFontDescriptorAttributes = new CTFontDescriptorAttributes();
            //cTFontDescriptorAttributes.Traits = new CTFontTraits();
            //cTFontDescriptorAttributes.FamilyName = familyName;
            //cTFontDescriptorAttributes.Traits.SymbolicTraits = CTFontSymbolicTraits.Italic;
            //CTFontDescriptor cTFontDescriptor = new CTFontDescriptor(cTFontDescriptorAttributes);
            //CTFont ctFont = new CTFont(cTFontDescriptor, this.FontSize);
            //this.SetFont(ctFont);

            this.SetFont(familyName);

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
                //font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Italic);
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