using System;
using System.Windows.Input;
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
        public RSButtonNative positiveButton;
        public RSButtonNative destructiveButton;
        public RSButtonNative neutralButton;
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
            dialogStack.AddArrangedSubview(contentStack);

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
            dialogStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(250f).Active = true;
            dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 0.9f).Active = true;
            dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 0.7f).Active = true;
            dialogStack.HeightAnchor.ConstraintGreaterThanOrEqualTo(150f).Active = true;

            dialogStack.CenterXAnchor.ConstraintEqualTo(DialogView.CenterXAnchor).Active = true;
            dialogStack.CenterYAnchor.ConstraintEqualTo(DialogView.CenterYAnchor).Active = true;
        }

        //Set content scrollview
        private void SetupContentStack()
        {
            //content stack
            contentStack.Axis = UILayoutConstraintAxis.Vertical;
            contentStack.Distribution = UIStackViewDistribution.Fill;
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


            positiveButton = new RSButtonNative(UIButtonType.System, RSPopupButtonTypeEnum.Positive) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 
            neutralButton = new RSButtonNative(UIButtonType.System, RSPopupButtonTypeEnum.Neutral) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 
            destructiveButton = new RSButtonNative(UIButtonType.System, RSPopupButtonTypeEnum.Destructive) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) }; 


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
            titleText.UserInteractionEnabled = false;
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
            messageLabel.UserInteractionEnabled = false;
            messageLabel.ScrollEnabled = false;
            messageLabel.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
            messageLabel.TextContainerInset = new UIEdgeInsets(5, 5, 5, 5);
        }

        //Set and add custom view 
        private void SetCustomView()
        {


            var nativeView = Extensions.ViewExtensions.ConvertFormsToNative(CustomView, new CGRect(0, 0, 200, 200));
            this.contentStack.AddArrangedSubview(nativeView);
            contentStack.InvalidateIntrinsicContentSize();
            contentStack.LayoutIfNeeded();
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter = null)
        {
            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton.dialog = this;
                positiveButton.Command = command;
                positiveButton.Hidden = false;
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                AddBorder(positiveButton);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton.dialog = this;
                neutralButton.Command = command;
                neutralButton.Hidden = false;
                neutralButton.SetTitle(title, UIControlState.Normal);
                neutralButton.SetTitleColor(UIColor.SystemBlueColor, UIControlState.Normal);
                AddBorder(neutralButton);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton.dialog = this;
                destructiveButton.Command = command;
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
            SetupContentStack();
            SetTitle(Title, 18);
            CreateButtonsStack();
            SetMessage(Message, 12);
            if(CustomView != null)
                SetCustomView();


            UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
            {
                Dismiss(true);
                Dispose();
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    //Custom button used to assign command
    public class RSButtonNative : UIButton
    {
        public RSPopupRenderer dialog { get; set; }
        public object CommandParameter { get; set; }
        private Command command;
        public Command Command
        {
            get
            {
                return command;
            }
            set
            {
                command = value;

                if (command != null)
                {
                    command.CanExecuteChanged += Command_CanExecuteChanged;
                    Command.ChangeCanExecute();
                }
            }
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            if ((sender as Command).CanExecute(CommandParameter))
                this.Enabled = true;
            else
                this.Enabled = false;
        }

        public RSPopupButtonTypeEnum rSPopupButtonType { get; set; }

        public RSButtonNative(UIButtonType type, RSPopupButtonTypeEnum rSPopupButtonType) : base(type)
        {
            this.rSPopupButtonType = rSPopupButtonType;
            this.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            this.AddTarget(ButtonEventHandler, UIControlEvent.TouchUpInside);
        }

        private void ButtonEventHandler(object sender, EventArgs e)
        {
            var button = sender as RSButtonNative;

            if (button.Command != null)
                button.Command.Execute(null);


            if (button.rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
                dialog.Dismiss(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
