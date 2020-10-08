﻿using System;
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

            //Delete placeholder as we use floating hint instead
            //Element.Placeholder = "";
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
        private nfloat shadowRadius;
        private CALayer border;
        private CALayer filledBorder;
        private CGPath borderShadowPath;
        private CustomCATextLayer floatingHint;
        private CustomCATextLayer errorLabel;
        private CustomCATextLayer helperLabel;
        private CustomCATextLayer counterLabel;
        private UIView leftIconSeparatorView;
        private UIView rightIconSeparatorView;
        private nfloat iconPadding;
        private nfloat iconsSeparationWidth;
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

        //Padding values
        private nfloat topSpacing;
        private nfloat bottomSpacing;
        private nfloat topPadding;
        private nfloat bottomPadding;
        private nfloat leftPadding;
        private nfloat rightPadding;

        //Device orientation observer
        private NSObject deviceRotationObserver;
        private UIDeviceOrientation orientation;


        //Constructor
        public RSUITextField(IRSControl rSControl)
        {
            this.rSControl = rSControl;

            if (rSControl.FontSize < 12)
                this.floatingFontSize = (nfloat)rSControl.FontSize;
            else
                this.floatingFontSize = 12;
            if (rSControl.Placeholder != null)
                this.floatingHintMaskPadding = 3;
            else
                this.floatingHintMaskPadding = 0;

            this.borderRadius = rSControl.BorderRadius;
            this.shadowRadius = rSControl.ShadowRadius;
            this.counterMaxLength = this.rSControl.CounterMaxLength;
            this.borderColor = this.rSControl.BorderColor.ToUIColor();
            this.borderWidth = rSControl.BorderWidth;
            this.borderWidthFocused = rSControl.BorderWidth + 1;
            this.activeColor = UIColor.SystemBlueColor;
            this.topSpacing = 5;
            this.bottomSpacing = 20;
            this.leftRightSpacingLabels = 5;
            this.iconPadding = 6;
            this.iconsSeparationWidth = 16;

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
            orientation = UIDevice.CurrentDevice.Orientation;

        }


        //Set Padding
        private void SetPadding()
        {
            if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 17 + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = 25f + (nfloat)rSControl.Padding.Bottom;
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
                this.rightPadding = 8 + (nfloat)rSControl.Padding.Right;
                this.topPadding = 25 + (nfloat)rSControl.Padding.Top;
                this.bottomPadding = 20 + (nfloat)rSControl.Padding.Bottom;
            }
            else if (this.rSControl.RSEntryStyle == RSEntryStyleSelectionEnum.Underline)
            {
                this.leftPadding = 8 + (nfloat)rSControl.Padding.Left;
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



            if (this.rSControl.LeadingIcon != null)
            {
                Forms.View leadingSvgIcon = rSControl.LeadingIcon.View;
                leadingView = Extensions.ViewExtensions.ConvertFormsToNative(leadingSvgIcon, 0, floatingHintYPostionNotFloating, rSControl.LeadingIcon.IconWidth, rSControl.LeadingIcon.IconHeight);
                this.AddSubview(leadingView);

                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => {
                    // Do something
                    //this.BecomeFirstResponder();
                    RSPopup rSPopup = new RSPopup("Title", "Message rthtrhttrhtrhtr hrthrt rthrt rth rth rgreggregreregergergregregergreg ergerg ergergre ergerg erg ");
                    rSPopup.SetPopupSize(Enums.RSPopupSizeEnum.WrapContent, Enums.RSPopupSizeEnum.WrapContent);
                    rSPopup.SetPopupPositionRelativeTo(this.rSControl as Forms.View, Enums.RSPopupPositionSideEnum.Top);
                    rSPopup.SetDimAmount(0f);
                    rSPopup.SetMargin(40, 0, 40, 0);
                    rSPopup.Show();

                    CreateAndExecuteIconMethod(this.rSControl.LeadingIcon);
                    AddRippleEffect(leadingView);
                });

                leadingView.AddGestureRecognizer(gest);
                leadingViewWidth = leadingView.Frame.Width + iconPadding;
            }

            if (this.rSControl.LeftIcon != null)
            {
                Forms.View leftSvgIcon = rSControl.LeftIcon.View as Forms.View;
                //RSSvgImage leftSvgIcon = new RSSvgImage() { Source = rSControl.LeftIcon.Path, HeightRequest = 22, WidthRequest = 22, Color = Color.Gray };
                leftView = Extensions.ViewExtensions.ConvertFormsToNative(leftSvgIcon, new CGRect(0, 0, 22, 22));
                this.AddSubview(leftView);
                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => {CreateAndExecuteIconMethod(this.rSControl.LeftIcon); AddRippleEffect(leftView);});
                leftView.AddGestureRecognizer(gest);
                if (rSControl.LeftIcon != null)
                    leftViewWidth = leftView.Frame.Width + iconsSeparationWidth;
                else
                    leftViewWidth = leftView.Frame.Width + iconPadding;
            }

            if (rSControl.HasLeftIconSeparator && rSControl.LeftIcon != null)
            {
                leftIconSeparatorView = new UIView(new CGRect(leftPadding + leadingViewWidth + leftViewWidth, 0, 1, 22)) { BackgroundColor = UIColor.LightGray };
                this.AddSubview(leftIconSeparatorView);
            }

            if (this.rSControl.LeftHelpingIcon != null)
            {
                Forms.View leftHelpingSvgIcon = rSControl.LeftHelpingIcon.View;
                leftHelpingView = Extensions.ViewExtensions.ConvertFormsToNative(leftHelpingSvgIcon, new CGRect(0, 0, 22, 22));
                this.AddSubview(leftHelpingView);
                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => { CreateAndExecuteIconMethod(this.rSControl.LeftHelpingIcon); AddRippleEffect(leftHelpingView); });
                leftHelpingView.AddGestureRecognizer(gest);
                leftHelpingViewWidth = leftHelpingView.Frame.Width + iconPadding;
            }

            if (rSControl.RightHelpingIcon != null)
            {
                Forms.View rightHelpingSvgIcon = rSControl.RightHelpingIcon.View;
                rightHelpingView = Extensions.ViewExtensions.ConvertFormsToNative(rightHelpingSvgIcon, this.Frame.Right, floatingHintYPostionNotFloating, 22, 22);
                this.AddSubview(rightHelpingView);
                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => { CreateAndExecuteIconMethod(this.rSControl.RightHelpingIcon); AddRippleEffect(rightHelpingView); });
                rightHelpingView.AddGestureRecognizer(gest);
                if(rSControl.RightIcon != null)
                    rightHelpingViewWidth = rightHelpingView.Frame.Width + iconsSeparationWidth + iconPadding;
                else
                    rightHelpingViewWidth = rightHelpingView.Frame.Width + iconPadding;
            }

            if (rSControl.HasRighIconSeparator && rSControl.RightIcon != null)
            {
                rightIconSeparatorView = new UIView(new CGRect(0, 0, 1, 22)) { BackgroundColor = UIColor.LightGray };
                this.AddSubview(rightIconSeparatorView);
            }

            if (rSControl.RightIcon != null)
            {
                Forms.View rightSvgIcon = rSControl.RightIcon.View;
                rightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, 0, 0, 22, 22);
                this.AddSubview(rightView);
                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => { CreateAndExecuteIconMethod(this.rSControl.RightIcon); AddRippleEffect(rightView); });
                rightView.AddGestureRecognizer(gest);
                rightViewWidth = rightView.Frame.Width;
            }

            if (this.rSControl.TrailingIcon != null)
            {
                Forms.View trailoingSvgIcon = rSControl.TrailingIcon.View;
                trailingView = Extensions.ViewExtensions.ConvertFormsToNative(trailoingSvgIcon, this.Frame.Right, floatingHintYPostionNotFloating, 22, 22);
                this.AddSubview(trailingView);
                UITapGestureRecognizer gest = new UITapGestureRecognizer(() => { CreateAndExecuteIconMethod(this.rSControl.TrailingIcon); AddRippleEffect(trailingView); });
                trailingView.AddGestureRecognizer(gest);
                trailingViewWidth = trailingView.Frame.Width + iconPadding;
            }
        }
        //Update Icons position
        private void UpdateIconsPosition()
        {
            if (leadingView != null)
                leadingView.Frame = new CGRect(0, floatingHintYPostionNotFloating - 11, 22, 22);

            if (leftView != null)
                leftView.Frame = new CGRect(leftPadding + leadingViewWidth, floatingHintYPostionNotFloating - 11, 22, 22);

            if(leftIconSeparatorView != null)
                leftIconSeparatorView.Frame = new CGRect(leftPadding + leadingViewWidth + leftViewWidth - iconsSeparationWidth / 2, floatingHintYPostionNotFloating - 11, 1, 22);

            if (leftHelpingView != null)
                leftHelpingView.Frame = new CGRect(leftPadding + leadingViewWidth + leftViewWidth, floatingHintYPostionNotFloating - 11, 22, 22);

            if (rightHelpingView != null)
                rightHelpingView.Frame = new CGRect(this.Frame.Right - rightPadding - trailingViewWidth - rightViewWidth - rightHelpingViewWidth + iconPadding, floatingHintYPostionNotFloating - 11, 22, 22);

            if (rightIconSeparatorView != null)
                rightIconSeparatorView.Frame = new CGRect(this.Frame.Right - rightPadding - trailingViewWidth - rightViewWidth - iconsSeparationWidth / 2, floatingHintYPostionNotFloating - 11, 1, 22);

            if (rightView != null)
                rightView.Frame = new CGRect(this.Frame.Right - rightPadding - trailingViewWidth - rightViewWidth, floatingHintYPostionNotFloating - 11, 22, 22);

            if (trailingView != null)
                trailingView.Frame = new CGRect(this.Frame.Right - trailingViewWidth + iconPadding, floatingHintYPostionNotFloating - 11, 22, 22);
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
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor();
                else
                    this.floatingHint.ForegroundColor = rSControl.PlaceholderStyle.FontColor.ToCGColor();

                this.border.BorderColor = this.borderColor.CGColor;
            }

            floatingHintXPostion = floatingHintXPostionNotFloating;
            floatingHintYPostion = floatingHintYPostionNotFloating;

            if (!IsFloating)
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

        //Used by external classes
        public void ForceFloatingHintFloatOrNot()
        {
            if(!string.IsNullOrEmpty(this.Text) && !IsFloating)
            {
                this.IsFloating = true;
                floatingHintXPostion = floatingHintXPostionFloating;
                floatingHintYPostion = floatingHintYPostionFloating;
                AnimateFloatingHint(floatingFontSize);
            }
            else if(string.IsNullOrEmpty(this.Text) && !errorEnabled && IsFloating)
            {
                this.IsFloating = false;
                floatingHintXPostion = floatingHintXPostionNotFloating;
                floatingHintYPostion = floatingHintYPostionNotFloating;
                AnimateFloatingHint(this.rSControl.FontSize);
            }

        }


        //Rounded Border
        private void CreateRoundedBorder()
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

            this.Layer.AddSublayer(counterLabel);
        }
        //Set Counter
        private void SetCounter()
        {
            this.counter = this.Text.Length;
            counterLabel.String = string.Format("{0}/{1}", this.counter, this.counterMaxLength);

            counterLabel.Bounds = new CGRect(0.0f, 0.0f, counterLabel.Size.Width, counterLabel.Size.Height);
            counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - trailingViewWidth - leftRightSpacingLabels - counterLabel.Size.Width / 2,
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
        //Update counter position
        private void UpdateCounterPosition()
        {
            counterLabel.Bounds = new CGRect(0.0f, 0.0f, counterLabel.Size.Width, counterLabel.Size.Height);
            counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - trailingViewWidth - leftRightSpacingLabels - counterLabel.Size.Width / 2,
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



        //Set rounded border frame and mask
        public void UpdateOutlinedBorder()
        {
            nfloat shadowOffset = rSControl.ShadowRadius;

            //Border frame
            border.Frame = new CGRect(leadingViewWidth, topSpacing, this.Frame.Width - leadingViewWidth - trailingViewWidth, this.Frame.Height - this.bottomSpacing);

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
            filledBorder.Frame = new CGRect(leadingViewWidth, topSpacing, this.Frame.Width - leadingViewWidth - trailingViewWidth, this.Frame.Height - this.bottomSpacing);
            border.Frame = new CGRect(leadingViewWidth, topSpacing + this.Frame.Height - this.bottomSpacing - border.BorderWidth, this.Frame.Width - leadingViewWidth - trailingViewWidth, border.BorderWidth);


            //CAShapeLayer borderMask = new CAShapeLayer();
            //CGPath maskPath = new CGPath();

            ////Top rounded border mask
            //maskPath.AddRoundedRect(new CGRect(borderWidth + 1, borderWidth + 1, this.Frame.Width - (2 * borderWidth + 2), this.Frame.Height - this.bottomSpacing), borderRadius, borderRadius);

            ////Bottom edges make them straight
            //maskPath.AddRect(new CGRect(x: borderWidth + 1, y: this.border.Frame.Height / 2, width: this.Frame.Width - (2 * borderWidth + 2), height: this.border.Frame.Height));

            ////Bottom border
            //maskPath.AddRect(new CGRect(x: 0, y: this.border.Frame.Height, width: this.Frame.Width, height: this.borderWidth));


            //borderMask.Path = maskPath;
            //border.Mask = borderMask;
        }
        //Set Underline border frame
        public void UpdateUnderlineBorder()
        {
            //Border frame
            filledBorder.Frame = new CGRect(leadingViewWidth, 0, this.Frame.Width - leadingViewWidth - trailingViewWidth, this.Frame.Height - this.bottomSpacing + topSpacing);
            border.Frame = new CGRect(leadingViewWidth, this.Frame.Height - this.bottomSpacing + topSpacing - border.BorderWidth, this.Frame.Width - leadingViewWidth - trailingViewWidth, border.BorderWidth);
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
            //System.Console.WriteLine("Rotate");
            //Reset border frame and mask
            //if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.PortraitUpsideDown)
            // SetBorder();

            //Update icons position if any
            //UpdateIconsPosition();

            //SetNeedsDisplay();

            //UpdateFloatingLabel();
            //UpdateIconsPosition();
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
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding + leadingViewWidth + leftViewWidth + leftHelpingViewWidth, this.bottomPadding, this.rightPadding + trailingViewWidth + rightViewWidth + rightHelpingViewWidth));
        }
        //Placeholder padding
        public override CGRect PlaceholderRect(CGRect forBounds)
        {
            return InsetRect(base.TextRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding + leadingViewWidth + leftViewWidth + leftHelpingViewWidth, this.bottomPadding, this.rightPadding + trailingViewWidth + rightViewWidth + rightHelpingViewWidth));
        }
        //Edit rectangle padding
        public override CGRect EditingRect(CGRect forBounds)
        {
            return InsetRect(base.EditingRect(forBounds), new UIEdgeInsets(this.topPadding, this.leftPadding + leadingViewWidth + leftViewWidth + leftHelpingViewWidth, this.bottomPadding, this.rightPadding + trailingViewWidth + rightViewWidth + rightHelpingViewWidth));
        }

        //public override CGRect RightViewRect(CGRect forBounds)
        //{
        //    //return base.RightViewRect(forBounds);

        //    CGRect rightBounds = new CGRect(forBounds.Right - rightPadding - trailingViewWidth - this.RightView.Frame.Width, floatingHintYPostionNotFloating - 22 / 2, 22, this.RightView.Frame.Height);
        //    return rightBounds;
        //}

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
                    floatingHintXPostionFloating = this.leftPadding + leadingViewWidth + leftHelpingViewWidth + leftViewWidth + floatinghHintSizeNotFloating.Width / 2;
                    floatingHintXPostionNotFloating = this.leftPadding + leadingViewWidth + leftHelpingViewWidth + leftViewWidth + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = (this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2) - floatinghHintSizeFloating.Height / 2 - 2;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding + topSpacing - bottomSpacing) / 2 ;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + leadingViewWidth + floatinghHintSizeNotFloating.Width / 2 + leftRightSpacingLabels;
                    floatingHintXPostionNotFloating = this.leftPadding + leadingViewWidth + leftHelpingViewWidth + leftViewWidth + floatinghHintSizeNotFloating.Width / 2;

                    //Y
                    floatingHintYPostionFloating = 0 + topSpacing + (floatinghHintSizeNotFloating.Height - floatinghHintSizeFloating.Height) / 2;
                    floatingHintYPostionNotFloating = this.Frame.Height / 2 + (topPadding - bottomPadding) / 2;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                {
                    //X
                    floatingHintXPostionFloating = this.leftPadding + leadingViewWidth + floatinghHintSizeNotFloating.Width / 2;
                    floatingHintXPostionNotFloating = this.leftPadding + leadingViewWidth + leftHelpingViewWidth + leftViewWidth + floatinghHintSizeNotFloating.Width / 2;

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
                    errorLabel.Position = new CGPoint(this.leftPadding + leadingViewWidth + leftRightSpacingLabels + errorLabel.Size.Width / 2,
                                                       this.Frame.Height - errorLabel.Size.Height / 2 - 1);

                if (helperLabel != null)
                    helperLabel.Position = new CGPoint(this.leftPadding + leadingViewWidth + leftRightSpacingLabels + helperLabel.Size.Width / 2,
                                                       this.Frame.Height - helperLabel.Size.Height / 2 - 1);

                //if (counterLabel != null)
                //    counterLabel.Position = new CGPoint(this.Frame.Width - this.rightPadding - leftRightSpacingLabels - counterLabel.Size.Width / 2,
                //                                       this.Frame.Height - counterLabel.Size.Height / 2 - 1);


                //Init here because text property not yet set in create floatinghint method
                FloatingHintFramePlacement();


                //Update icons position if any
                UpdateIconsPosition();


                hasInitfinished = true;
            }


            //Counter
            if (this.counterMaxLength != -1)
                SetCounter();
        }

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

        //Update ui 
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            UpdateFloatingLabel();
            UpdateIconsPosition();
            if(counterLabel != null)
                UpdateCounterPosition();
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
            if (hint == null)
                hint = string.Empty;

            this.familyName = familyName;

            //CTFontDescriptorAttributes cTFontDescriptorAttributes = new CTFontDescriptorAttributes();
            //cTFontDescriptorAttributes.Traits = new CTFontTraits();
            //cTFontDescriptorAttributes.FamilyName = familyName;
            //cTFontDescriptorAttributes.Traits.SymbolicTraits = CTFontSymbolicTraits.Italic;
            //CTFontDescriptor cTFontDescriptor = new CTFontDescriptor(cTFontDescriptorAttributes);
            //CTFont ctFont = new CTFont(familyName, this.FontSize);
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