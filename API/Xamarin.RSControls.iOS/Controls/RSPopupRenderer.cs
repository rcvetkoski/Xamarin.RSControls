using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPopupRenderer : UIView, IDialogPopup
    {
        private UIView mainView { get; set; }
        public UIView BackgroundView { get; set; }
        public UIView DialogView { get; set; }
        private UIStackView dialogStack { get; set; }
        private UIStackView contentStack { get; set; }
        private UIScrollView contentScroll { get; set; }
        private UIStackView buttonsStack { get; set; }
        public UIButton positiveButton;
        public UIButton destructiveButton;
        public UIButton neutralButton;
        private UITextView titleText;
        private UITextView messageLabel;
        private bool isAnimating;
        public string Title { get; set; }
        public string Message { get; set; }
        public Forms.View RelativeView { get; set; }
        public Forms.View CustomView { get; set; }
        public Forms.Color BorderFillColor { get; set; }
        public float DimAmount { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float BorderRadius { get; set; }
        public bool ShadowEnabled { get; set; }
        private int buttonsCount;
        

        //Constructor
        public RSPopupRenderer() : base()
        {
            buttonsCount = 0;
            BorderRadius = 12;
            ShadowEnabled = true;
        }

        //Init Views
        private void InitViews()
        {
            //MainView
            mainView = UIApplication.SharedApplication.Delegate?.GetWindow()?.RootViewController?.View;
            mainView.AddSubview(this);
            this.TranslatesAutoresizingMaskIntoConstraints = false;
            this.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor).Active = true;
            this.TrailingAnchor.ConstraintEqualTo(mainView.TrailingAnchor).Active = true;
            this.TopAnchor.ConstraintEqualTo(mainView.TopAnchor).Active = true;
            this.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor).Active = true;


            //BackgroundView
            BackgroundView = new UIView();
            AddSubview(BackgroundView);

            //DialogVIew
            DialogView = new UIView();
            AddSubview(DialogView);

            //DialogStack
            dialogStack = new UIStackView();
            DialogView.AddSubview(dialogStack);

            //Title
            titleText = new UITextView();
            dialogStack.AddArrangedSubview(titleText);

            //ContentStack
            contentStack = new UIStackView();

            //Content scroll
            contentScroll = new UIScrollView();
            contentScroll.AddSubview(contentStack);
            dialogStack.AddArrangedSubview(contentScroll);

            //Message
            messageLabel = new UITextView();
            contentStack.AddArrangedSubview(messageLabel);

            //Buttons stack
            buttonsStack = new UIStackView();
            dialogStack.AddArrangedSubview(buttonsStack);
        }

        //Setup backgroundView
        private void SetupBackgroundView()
        {
            BackgroundView.TranslatesAutoresizingMaskIntoConstraints = false;
            BackgroundView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor).Active = true;
            BackgroundView.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor).Active = true;
            BackgroundView.TopAnchor.ConstraintEqualTo(this.TopAnchor).Active = true;
            BackgroundView.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;

            BackgroundView.BackgroundColor = UIColor.Black.ColorWithAlpha(DimAmount);
        }

        //Setup DialogView
        private void SetupDialogView()
        {
            //Setup graphics
            DialogView.Layer.CornerRadius = BorderRadius;
            DialogView.Layer.BorderWidth = 1;
            DialogView.Layer.BorderColor = UIColor.White.CGColor;
            DialogView.Layer.ShadowColor = UIColor.Gray.CGColor;
            DialogView.Layer.ShadowOpacity = 0.5f;
            DialogView.Layer.ShadowRadius = 10;
            DialogView.Layer.BackgroundColor = BorderFillColor.ToCGColor();
            DialogView.Layer.ShadowOffset = new CGSize(0f, 5f);



            //Constraints
            DialogView.TranslatesAutoresizingMaskIntoConstraints = false;

            ////Set size
            DialogView.WidthAnchor.ConstraintEqualTo(dialogStack.WidthAnchor).Active = true;
            DialogView.HeightAnchor.ConstraintEqualTo(dialogStack.HeightAnchor).Active = true;
            ////DialogView.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 0.9f).Active = true;
            ////DialogView.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 0.8f).Active = true;
            ////DialogView.WidthAnchor.ConstraintGreaterThanOrEqualTo(220f).Active = true;
            ////DialogView.HeightAnchor.ConstraintGreaterThanOrEqualTo(150f).Active = true;

            ////Position dialog
            DialogView.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
            DialogView.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
        }

        //Setup stackDialog
        private void SetupDialogStack()
        {
            dialogStack.Axis = UILayoutConstraintAxis.Vertical;
            dialogStack.Distribution = UIStackViewDistribution.Fill;

            //Constraints
            dialogStack.TranslatesAutoresizingMaskIntoConstraints = false;
            //dialogStack.TopAnchor.ConstraintEqualTo(DialogView.TopAnchor).Active = true;
            //dialogStack.BottomAnchor.ConstraintEqualTo(DialogView.BottomAnchor).Active = true;
            //dialogStack.LeadingAnchor.ConstraintEqualTo(DialogView.LeadingAnchor).Active = true;
            //dialogStack.TrailingAnchor.ConstraintEqualTo(DialogView.TrailingAnchor).Active = true;

            dialogStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(250f).Active = true;
            dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 0.9f).Active = true;

            dialogStack.CenterXAnchor.ConstraintEqualTo(DialogView.CenterXAnchor).Active = true;
            dialogStack.CenterYAnchor.ConstraintEqualTo(DialogView.CenterYAnchor).Active = true;
        }

        //Set content scrollview
        private void SetupContentScroll()
        {
            //content stack
            contentStack.Axis = UILayoutConstraintAxis.Vertical;
            contentStack.Distribution = UIStackViewDistribution.Fill;

            contentStack.TranslatesAutoresizingMaskIntoConstraints = false;
            contentStack.LeadingAnchor.ConstraintEqualTo(contentScroll.LeadingAnchor).Active = true;
            contentStack.TrailingAnchor.ConstraintEqualTo(contentScroll.TrailingAnchor).Active = true;
            contentStack.TopAnchor.ConstraintEqualTo(contentScroll.TopAnchor).Active = true;
            contentStack.BottomAnchor.ConstraintEqualTo(contentScroll.BottomAnchor).Active = true;
            contentStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor,0.9f).Active = true;


            //Scroll
            contentScroll.BackgroundColor = UIColor.Clear;
            contentScroll.TranslatesAutoresizingMaskIntoConstraints = false;
            contentScroll.WidthAnchor.ConstraintEqualTo(contentStack.WidthAnchor).Active = true;
            contentScroll.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 0.7f).Active = true;
            var heightEqualConstraint = contentScroll.HeightAnchor.ConstraintEqualTo(contentStack.HeightAnchor);
            heightEqualConstraint.Priority = 250; //low priority
            heightEqualConstraint.Active = true;
            contentScroll.HeightAnchor.ConstraintGreaterThanOrEqualTo(60f).Active = true;

        }

        //Create buttons stack
        private void CreateButtonsStack()
        {
            buttonsStack.Axis = UILayoutConstraintAxis.Horizontal;
            buttonsStack.Distribution = UIStackViewDistribution.FillEqually;
            var topBorder = new UIView( ){ BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false };
            buttonsStack.AddSubview(topBorder);
            topBorder.LeadingAnchor.ConstraintEqualTo(buttonsStack.LeadingAnchor).Active = true;
            topBorder.TrailingAnchor.ConstraintEqualTo(buttonsStack.TrailingAnchor).Active = true;
            topBorder.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;


            positiveButton = new UIButton(UIButtonType.System) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 
            neutralButton = new UIButton(UIButtonType.System) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 
            destructiveButton = new UIButton(UIButtonType.System) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 


            buttonsStack.AddArrangedSubview(destructiveButton);
            buttonsStack.AddArrangedSubview(neutralButton);
            buttonsStack.AddArrangedSubview(positiveButton);

            buttonsCount = 3;
        }

        //Set title
        public void SetTitle(string title, float fontSize)
        {
            titleText.BackgroundColor = UIColor.Clear;
            titleText.Text = title;
            titleText.TextColor = UIColor.DarkGray;
            titleText.Font = UIFont.BoldSystemFontOfSize(fontSize);
            titleText.TextAlignment = UITextAlignment.Center;
            titleText.ScrollEnabled = false;
            titleText.TextContainerInset = new UIEdgeInsets(15, 0, 5, 0);
        }

        //Set message
        public void SetMessage(string message, float fontSize)
        {
            messageLabel.BackgroundColor = UIColor.Clear;
            messageLabel.TextColor = UIColor.DarkGray;
            messageLabel.Text = message;
            messageLabel.Font = UIFont.SystemFontOfSize(fontSize);
            messageLabel.TextAlignment = UITextAlignment.Center;
            //messageLabel.TextContainer.HeightTracksTextView = true;
            messageLabel.ScrollEnabled = false;
            messageLabel.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
            messageLabel.TextContainerInset = new UIEdgeInsets(5, 5, 5, 5);
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType)
        {
            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton.Hidden = false;
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                AddBorder(positiveButton);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton.Hidden = false;
                neutralButton.SetTitle(title, UIControlState.Normal);
                neutralButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                AddBorder(neutralButton);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton.Hidden = false;
                destructiveButton.SetTitle(title, UIControlState.Normal);
                destructiveButton.SetTitleColor(UIColor.SystemRedColor, UIControlState.Normal);
                AddBorder(destructiveButton);
            }
            else
            {

            }
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

        //ShowPopup
        public void ShowPopup()
        {
            InitViews();
            SetupBackgroundView();
            SetupDialogView();
            SetupDialogStack();
            SetupContentScroll();
            SetTitle(Title, 18);
            CreateButtonsStack();
            SetMessage(Message, 12);


            UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
            {
                Dismiss(true);
            });

            BackgroundView.AddGestureRecognizer(didTappedOnBackgroundView);

            bool animated = true;
            this.BackgroundView.Alpha = 0;
            this.DialogView.Alpha = 0;
            this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f);

            //var mainView = UIApplication.SharedApplication.Delegate?.GetWindow()?.RootViewController?.View;
            //mainView.AddSubview(this);


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


        private void AddBorder(UIButton button)
        {
            //var topBorder = new UIView() { BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false };
            //button.AddSubview(topBorder);
            //topBorder.LeadingAnchor.ConstraintEqualTo(button.LeadingAnchor).Active = true;
            //topBorder.TrailingAnchor.ConstraintEqualTo(button.TrailingAnchor).Active = true;
            //topBorder.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;

            if (buttonsCount > 1)
            {
                UIView border = new UIView() { BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false };
                button.AddSubview(border);
                border.HeightAnchor.ConstraintEqualTo(button.HeightAnchor).Active = true;
                border.WidthAnchor.ConstraintEqualTo(0.5f).Active = true;

                if(buttonsStack.ArrangedSubviews[buttonsCount - 1] != button)
                    border.LeadingAnchor.ConstraintEqualTo(button.TrailingAnchor).Active = true;
            }
        }

        public enum UIButtonBorderSide
        {
            Top, Bottom, Left, Right
        }
    }





    //public class RSPopupRenderer : UIView, IDialogPopup
    //{
    //    public UIView BackgroundView { get; set; }
    //    public UIView DialogView { get; set; }
    //    private UIStackView stackDialog { get; set; }
    //    private UIStackView contentStack { get; set; }
    //    private UIStackView buttonsStack { get; set; }
    //    public UIButton positiveButton;
    //    public UIButton destructiveButton;
    //    public UIButton neutralButton;
    //    private UILabel titleLabel;
    //    private UILabel messageLabel;
    //    private bool isAnimating;
    //    public string Title { get; set; }
    //    public string Message { get; set; }
    //    public Forms.View RelativeView { get; set; }
    //    public Forms.View CustomView { get; set; }
    //    public Forms.Color BorderFillColor { get; set; }
    //    public float DimAmount { get; set; }
    //    public float PositionX { get; set; }
    //    public float PositionY { get; set; }
    //    public float BorderRadius { get; set; }
    //    public bool ShadowEnabled { get; set; }


    //    public RSPopupRenderer() : base()
    //    {
    //        BorderRadius = 12;
    //        ShadowEnabled = true;
    //    }

    //    //Init Views
    //    private void InitViews()
    //    {
    //        //BackgroundView
    //        BackgroundView = new UIView();
    //        AddSubview(BackgroundView);

    //        //DialogVIew
    //        DialogView = new UIView();
    //        AddSubview(DialogView);

    //        //Stack dialog
    //        stackDialog = new UIStackView();
    //        DialogView.AddSubview(stackDialog);

    //        //Title
    //        titleLabel = new UILabel();
    //        stackDialog.AddArrangedSubview(titleLabel);

    //        //Content Stack
    //        contentStack = new UIStackView();
    //        stackDialog.AddArrangedSubview(contentStack);

    //        //Buttons stack
    //        buttonsStack = new UIStackView();
    //        stackDialog.AddArrangedSubview(buttonsStack);
    //    }

    //    //Setup backgroundView
    //    private void SetupBackgroundView()
    //    {
    //        this.Frame = UIScreen.MainScreen.Bounds;
    //        BackgroundView.Frame = this.Frame;
    //        BackgroundView.BackgroundColor = UIColor.Black.ColorWithAlpha(DimAmount);
    //    }

    //    //Setup DialogView
    //    private void SetupDialogView()
    //    {
    //        //Setup graphics
    //        DialogView.Layer.CornerRadius = BorderRadius;
    //        DialogView.Layer.BorderWidth = 1;
    //        DialogView.Layer.BorderColor = UIColor.White.CGColor;
    //        DialogView.Layer.ShadowColor = UIColor.Gray.CGColor;
    //        DialogView.Layer.ShadowOpacity = 0.5f;
    //        DialogView.Layer.ShadowRadius = BorderRadius;
    //        DialogView.Layer.BackgroundColor = BorderFillColor.ToCGColor();
    //        DialogView.Layer.ShadowOffset = new CGSize(1f, 1f);


    //        //Constraints
    //        DialogView.TranslatesAutoresizingMaskIntoConstraints = false;
    //        //DialogView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, (nfloat)PositionX).Active = true;
    //        //DialogView.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor, -(nfloat)PositionX).Active = true;
    //        //DialogView.TopAnchor.ConstraintEqualTo(this.TopAnchor, (nfloat)PositionY).Active = true;

    //        DialogView.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
    //        DialogView.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
    //        DialogView.WidthAnchor.ConstraintEqualTo(stackDialog.WidthAnchor).Active = true;
    //        DialogView.HeightAnchor.ConstraintEqualTo(stackDialog.HeightAnchor).Active = true;
    //        DialogView.WidthAnchor.ConstraintGreaterThanOrEqualTo(230f).Active = true; ;
    //        DialogView.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 0.9f).Active = true;
    //        DialogView.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 0.8f).Active = true;
    //    }

    //    //Setup stackDialog
    //    private void SetupStackDialogView()
    //    {
    //        stackDialog.Axis = UILayoutConstraintAxis.Vertical;
    //        stackDialog.Distribution = UIStackViewDistribution.FillProportionally;
    //        stackDialog.TranslatesAutoresizingMaskIntoConstraints = false;

    //        //Constraints
    //        stackDialog.BottomAnchor.ConstraintEqualTo(DialogView.BottomAnchor).Active = true;
    //        stackDialog.TopAnchor.ConstraintEqualTo(DialogView.TopAnchor).Active = true;
    //        stackDialog.LeadingAnchor.ConstraintEqualTo(DialogView.LeadingAnchor).Active = true;
    //        stackDialog.TrailingAnchor.ConstraintEqualTo(DialogView.TrailingAnchor).Active = true;
    //    }

    //    //Setup contentStack
    //    private void SetupContentStack()
    //    {
    //        //contentStack.BackgroundColor = UIColor.Green;
    //        contentStack.Axis = UILayoutConstraintAxis.Vertical;
    //        contentStack.Distribution = UIStackViewDistribution.FillProportionally;
    //        contentStack.TranslatesAutoresizingMaskIntoConstraints = false;
    //        contentStack.HeightAnchor.ConstraintGreaterThanOrEqualTo(70f).Active = true;
    //    }

    //    //Create buttons stack
    //    private void CreateButtonsStack()
    //    {
    //        buttonsStack.Axis = UILayoutConstraintAxis.Horizontal;
    //        buttonsStack.Distribution = UIStackViewDistribution.FillEqually;
    //        positiveButton = new UIButton(UIButtonType.System) { Hidden = true };
    //        neutralButton = new UIButton(UIButtonType.System) { Hidden = true };
    //        destructiveButton = new UIButton(UIButtonType.System) { Hidden = true };


    //        buttonsStack.AddArrangedSubview(destructiveButton);
    //        buttonsStack.AddArrangedSubview(neutralButton);
    //        buttonsStack.AddArrangedSubview(positiveButton);
    //    }


    //    public void SetTitle(string title, float fontSize)
    //    {
    //        titleLabel.Text = title;
    //        titleLabel.TextColor = UIColor.DarkGray;
    //        titleLabel.Font = UIFont.BoldSystemFontOfSize(fontSize);
    //        titleLabel.TextAlignment = UITextAlignment.Center;
    //    }

    //    public void SetMessage(string message, float fontSize)
    //    {
    //        messageLabel = new UILabel();
    //        messageLabel.TextColor = UIColor.DarkGray;
    //        messageLabel.Text = message;
    //        messageLabel.Lines = 0;
    //        messageLabel.LineBreakMode = UILineBreakMode.WordWrap;
    //        messageLabel.Font = UIFont.SystemFontOfSize(fontSize);
    //        messageLabel.TextAlignment = UITextAlignment.Center;
    //        contentStack.AddArrangedSubview(messageLabel);
    //    }

    //    //Buttons
    //    public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType)
    //    {
    //        if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
    //        {
    //            positiveButton.Hidden = false;
    //            positiveButton.SetTitle(title, UIControlState.Normal);
    //            positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
    //        {
    //            neutralButton.Hidden = false;
    //            neutralButton.SetTitle(title, UIControlState.Normal);
    //            neutralButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
    //        {
    //            destructiveButton.Hidden = false;
    //            destructiveButton.SetTitle(title, UIControlState.Normal);
    //            destructiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
    //        }
    //        else
    //        {

    //        }


    //    }


    //    // Animation part
    //    public void Dismiss(bool animated)
    //    {
    //        if (animated)
    //        {
    //            isAnimating = true;
    //            UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0f; }, () => { });
    //            UIView.Animate(0.33, 0,
    //                           UIViewAnimationOptions.CurveEaseInOut,
    //                           () => { this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f); }, null);
    //            UIView.Animate(0.33,
    //                           () => { this.DialogView.Alpha = 0f; },
    //                           () => { this.RemoveFromSuperview(); isAnimating = false; });

    //        }
    //        else
    //        {
    //            this.RemoveFromSuperview();
    //        }
    //    }

    //    public void ShowPopup()
    //    {
    //        this.Frame = UIScreen.MainScreen.Bounds;
    //        InitViews();
    //        SetupBackgroundView();
    //        SetupStackDialogView();
    //        SetupDialogView();
    //        SetTitle(Title, 18);
    //        SetupContentStack();
    //        CreateButtonsStack();
    //        SetMessage(Message, 12);


    //        UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
    //        {
    //            Dismiss(true);
    //        });

    //        BackgroundView.AddGestureRecognizer(didTappedOnBackgroundView);

    //        bool animated = true;
    //        this.BackgroundView.Alpha = 0;
    //        this.DialogView.Alpha = 0;
    //        this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f);

    //        var mainView = UIApplication.SharedApplication.Delegate?.GetWindow()?.RootViewController?.View;
    //        mainView.AddSubview(this);


    //        if (animated)
    //        {
    //            UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0.66f; });

    //            UIView.Animate(0.33, () => { this.DialogView.Alpha = 1f; });


    //            UIView.Animate(0.33, 0,
    //                           UIViewAnimationOptions.CurveEaseInOut,
    //                           () => { this.DialogView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f); }, null);
    //        }
    //        else
    //        {
    //            this.BackgroundView.Alpha = 0.66f;
    //            this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height - this.DialogView.Frame.Height / 2);
    //        }
    //    }

    //    public override void LayoutSubviews()
    //    {
    //        base.LayoutSubviews();

    //        if (!isAnimating)
    //        {
    //            this.Frame = UIScreen.MainScreen.Bounds;
    //            BackgroundView.Frame = this.Frame;

    //            //nfloat width = 270;
    //            //nfloat height = 150;
    //            //nfloat centerX = this.Frame.Width / 2 - width / 2;
    //            //nfloat centerY = this.Frame.Height / 2 - height / 2;
    //            //DialogView.Frame = new CGRect(centerX, centerY, width, height);

    //            //var caTitleSize = caTitle.PreferredFrameSize();
    //            //caTitle.Frame = new CGRect(DialogView.Frame.Width / 2 - caTitleSize.Width / 2, 15, caTitleSize.Width, caTitleSize.Height);

    //            //var caMessageSize = caMessage.PreferredFrameSize();
    //            //caMessage.Frame = new CGRect(DialogView.Frame.Width / 2 - caMessageSize.Width / 2, 15 + caTitle.Frame.Height, caMessageSize.Width, caMessageSize.Height);
    //            //DialogView.Layer.AddSublayer(caMessage);
    //        }
    //    }
    //}

}
