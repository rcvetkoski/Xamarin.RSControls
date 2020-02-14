﻿using System;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPopupRenderer : UIView, IDialogPopup
    {
        public UIView BackgroundView { get; set; }
        public UIView DialogView { get; set; }
        private UIStackView stackDialog { get; set; }
        private UIStackView contentStack { get; set; }
        private UIStackView buttonsStack { get; set; }
        public UIButton positiveButton;
        public UIButton negativeButton;
        public UIButton neutralButton;
        private UILabel titleLabel;
        private UILabel messageLabel;
        private bool isAnimating;
        public float PositionX { get; set; }
        public float PositionY { get; set; }


        public RSPopupRenderer() : base()
        {

        }

        //Init Views
        private void InitViews()
        {
            //BackgroundView
            BackgroundView = new UIView();
            AddSubview(BackgroundView);

            //DialogVIew
            DialogView = new UIView();
            AddSubview(DialogView);

            //Stack dialog
            stackDialog = new UIStackView();
            DialogView.AddSubview(stackDialog);

            //Title
            titleLabel = new UILabel();
            stackDialog.AddArrangedSubview(titleLabel);

            //Content Stack
            contentStack = new UIStackView();
            stackDialog.AddArrangedSubview(contentStack);

            //Buttons stack
            buttonsStack = new UIStackView();
            stackDialog.AddArrangedSubview(buttonsStack);
        }

        //Setup backgroundView
        private void SetupBackgroundView()
        {
            this.Frame = UIScreen.MainScreen.Bounds;
            BackgroundView.Frame = this.Frame;
            //BackgroundView.BackgroundColor = UIColor.Black.ColorWithAlpha(0.4f);
        }

        //Setup DialogView
        private void SetupDialogView()
        {
            //Setup graphics
            DialogView.Layer.CornerRadius = 12;
            DialogView.Layer.BorderWidth = 1;
            DialogView.Layer.BorderColor = UIColor.White.CGColor;
            DialogView.Layer.ShadowColor = UIColor.Gray.CGColor;
            DialogView.Layer.ShadowOpacity = 0.5f;
            DialogView.Layer.ShadowRadius = 7;
            DialogView.Layer.BackgroundColor = UIColor.White.CGColor;
            DialogView.Layer.ShadowOffset = new CGSize(1f, 1f);


            //Constraints
            DialogView.TranslatesAutoresizingMaskIntoConstraints = false;
            DialogView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, (nfloat)PositionX).Active = true;
            DialogView.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -(nfloat)PositionX).Active = true;
            DialogView.TopAnchor.ConstraintEqualTo(this.TopAnchor, (nfloat)PositionY).Active = true;

            //DialogView.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
            //DialogView.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
            DialogView.WidthAnchor.ConstraintEqualTo(stackDialog.WidthAnchor).Active = true;
            DialogView.HeightAnchor.ConstraintEqualTo(stackDialog.HeightAnchor).Active = true;
            DialogView.WidthAnchor.ConstraintGreaterThanOrEqualTo(250f).Active = true; ;
            DialogView.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor).Active = true;
            DialogView.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 0.8f).Active = true;
        }

        //Setup stackDialog
        private void SetupStackDialogView()
        {
            stackDialog.Axis = UILayoutConstraintAxis.Vertical;
            stackDialog.Distribution = UIStackViewDistribution.FillProportionally;
            stackDialog.TranslatesAutoresizingMaskIntoConstraints = false;

            //Constraints
            stackDialog.BottomAnchor.ConstraintEqualTo(DialogView.BottomAnchor).Active = true;
            stackDialog.TopAnchor.ConstraintEqualTo(DialogView.TopAnchor).Active = true;
            stackDialog.LeadingAnchor.ConstraintEqualTo(DialogView.LeadingAnchor).Active = true;
            stackDialog.TrailingAnchor.ConstraintEqualTo(DialogView.TrailingAnchor).Active = true;
        }

        //Setup contentStack
        private void SetupContentStack()
        {
            //contentStack.BackgroundColor = UIColor.Green;
            contentStack.Axis = UILayoutConstraintAxis.Vertical;
            contentStack.Distribution = UIStackViewDistribution.FillProportionally;
            contentStack.TranslatesAutoresizingMaskIntoConstraints = false;
            contentStack.HeightAnchor.ConstraintGreaterThanOrEqualTo(70f).Active = true;
        }

        //Create buttons stack
        private void CreateButtonsStack()
        {
            buttonsStack.Axis = UILayoutConstraintAxis.Horizontal;
            buttonsStack.Distribution = UIStackViewDistribution.FillEqually;

            //Add buttons
            AddButton("Cancel", RSPopupButtonTypeEnum.Neutral, null);
            //AddButton("Done", RSPopupButtonEnum.Positive, new EventHandler<Action>(trol));
            AddButton("Done", RSPopupButtonTypeEnum.Positive, null);
        }

        public void AddButton(string title, RSPopupButtonTypeEnum rSPopupButtonStyle, EventHandler<Action> action)
        {
            if (rSPopupButtonStyle == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton = new UIButton(UIButtonType.System);
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.AddTarget((sender, e) => { Dismiss(true); }, UIControlEvent.TouchUpInside);
                positiveButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
                positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                buttonsStack.AddArrangedSubview(positiveButton);
            }
            else if (rSPopupButtonStyle == RSPopupButtonTypeEnum.Destructive)
            {
                positiveButton = new UIButton(UIButtonType.System);
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
                positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                buttonsStack.AddArrangedSubview(positiveButton);
            }
            else if (rSPopupButtonStyle == RSPopupButtonTypeEnum.Neutral)
            {
                positiveButton = new UIButton(UIButtonType.System);
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.Font = UIFont.BoldSystemFontOfSize(positiveButton.Font.PointSize);
                positiveButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
                positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                buttonsStack.AddArrangedSubview(positiveButton);
            }
        }

        public void SetTitle(string title, float fontSize)
        {
            titleLabel.Text = title;
            //titleLabel.BackgroundColor = UIColor.Gray;
            titleLabel.Font = UIFont.BoldSystemFontOfSize(fontSize);
            titleLabel.TextAlignment = UITextAlignment.Center;
        }

        public void SetMessage(string message, float fontSize)
        {
            messageLabel = new UILabel();
            messageLabel.Text = message;
            messageLabel.Font = UIFont.SystemFontOfSize(fontSize);
            messageLabel.TextAlignment = UITextAlignment.Center;
            contentStack.AddArrangedSubview(messageLabel);
        }


        // Animation part
        public void Dismiss(bool animated)
        {
            if (animated)
            {
                isAnimating = true;
                UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0f; }, () => { });
                UIView.Animate(0.33, 0,
                               UIViewAnimationOptions.CurveEaseInOut,
                               () => { this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f); }, null);
                UIView.Animate(0.33,
                               () => { this.DialogView.Alpha = 0f; },
                               () => { this.RemoveFromSuperview(); isAnimating = false; });

            }
            else
            {
                this.RemoveFromSuperview();
            }
        }

        public void ShowPopup()
        {
            this.Frame = UIScreen.MainScreen.Bounds;
            InitViews();
            SetupBackgroundView();
            SetupStackDialogView();
            SetupDialogView();
            SetTitle("Title", 18);
            SetupContentStack();
            CreateButtonsStack();
            SetMessage("Message", 12);


            UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
            {
                Dismiss(true);
            });

            BackgroundView.AddGestureRecognizer(didTappedOnBackgroundView);

            bool animated = true;
            this.BackgroundView.Alpha = 0;
            this.DialogView.Alpha = 0;
            this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f);

            var mainView = UIApplication.SharedApplication.Delegate?.GetWindow()?.RootViewController?.View;
            mainView.AddSubview(this);


            if (animated)
            {
                UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0.66f; });

                UIView.Animate(0.33, () => { this.DialogView.Alpha = 1f; });


                UIView.Animate(0.33, 0,
                               UIViewAnimationOptions.CurveEaseInOut,
                               () => { this.DialogView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f); }, null);
            }
            else
            {
                this.BackgroundView.Alpha = 0.66f;
                this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height - this.DialogView.Frame.Height / 2);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (!isAnimating)
            {
                this.Frame = UIScreen.MainScreen.Bounds;
                BackgroundView.Frame = this.Frame;

                //nfloat width = 270;
                //nfloat height = 150;
                //nfloat centerX = this.Frame.Width / 2 - width / 2;
                //nfloat centerY = this.Frame.Height / 2 - height / 2;
                //DialogView.Frame = new CGRect(centerX, centerY, width, height);

                //var caTitleSize = caTitle.PreferredFrameSize();
                //caTitle.Frame = new CGRect(DialogView.Frame.Width / 2 - caTitleSize.Width / 2, 15, caTitleSize.Width, caTitleSize.Height);

                //var caMessageSize = caMessage.PreferredFrameSize();
                //caMessage.Frame = new CGRect(DialogView.Frame.Width / 2 - caMessageSize.Width / 2, 15 + caTitle.Frame.Height, caMessageSize.Width, caMessageSize.Height);
                //DialogView.Layer.AddSublayer(caMessage);
            }
        }
    }
}
