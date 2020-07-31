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
        private UIView topBorderButtonsStack;
        public RSButtonNative positiveButton;
        public RSButtonNative destructiveButton;
        public RSButtonNative neutralButton;
        private UITextView titleText;
        private UITextView messageLabel;
        private RSPopupArrow arrow;
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
        int IDialogPopup.PositionX { get; set; }
        int IDialogPopup.PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool UserSetPosition { get; set; }
        public bool UserSetSize { get; set; }
        public RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
        public bool IsModal { get; set; }
        public bool UserSetMargin { get; set; }
        public bool HasCloseButton { get; set; }
        public int RightMargin { get; set; }
        public int LeftMargin { get; set; }
        public int TopMargin { get; set; }
        public int BottomMargin { get; set; }
        public float RSPopupOffsetX { get; set; }
        public float RSPopupOffsetY { get; set; }
        private UIView relativeViewAsNativeView;
        private int buttonsCount;

        private UIDeviceOrientation orientation;
        private NSLayoutConstraint widthConstraint;
        private NSLayoutConstraint maxWidthConstraint;
        private NSLayoutConstraint heightConstraint; 
        private NSLayoutConstraint maxHeightConstraint;

        private NSLayoutConstraint positionXConstraint;
        private NSLayoutConstraint positionYConstraint;


        //Constructor
        public RSPopupRenderer() : base()
        {
            buttonsCount = 0;
            BorderRadius = 12;
            ShadowEnabled = true;

            InitViews();
            CreateButtonsStack();
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

            //Arrow
            arrow = new RSPopupArrow();
            AddSubview(arrow);

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


            orientation = UIDevice.CurrentDevice.Orientation;
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

        //Setup arrow
        private void SetupArrow()
        {
            arrow.TranslatesAutoresizingMaskIntoConstraints = false;
            arrow.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
            arrow.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor, -80).Active = true;
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
            DialogView.CenterXAnchor.ConstraintEqualTo(dialogStack.CenterXAnchor).Active = true;
            DialogView.CenterYAnchor.ConstraintEqualTo(dialogStack.CenterYAnchor).Active = true;
        }

        //Setup stackDialog
        private void SetupDialogStack()
        {
            dialogStack.Axis = UILayoutConstraintAxis.Vertical;
            //dialogStack.Distribution = UIStackViewDistribution.Fill;

            //Constraints
            dialogStack.TranslatesAutoresizingMaskIntoConstraints = false;
            //dialogStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(250f).Active = true;
            //dialogStack.HeightAnchor.ConstraintGreaterThanOrEqualTo(150f).Active = true;


            CGRect position = new CGRect();
            if(RelativeView != null)
            {
                var relativeViewAsNativeRenderer = Xamarin.Forms.Platform.iOS.Platform.GetRenderer(RelativeView);
                relativeViewAsNativeView = relativeViewAsNativeRenderer.NativeView;
                position = relativeViewAsNativeView.ConvertRectFromView(relativeViewAsNativeView.Bounds, this.mainView);
            }


            setSize(position);

            ////Height
            //if (this.Height == -1)//Match Parent
            //{

            //    heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.HeightAnchor, 1f, -(TopMargin + BottomMargin));
            //    heightConstraint.Priority = 999f;
            //    heightConstraint.Active = true;
            //}
            //else if (this.Height == -2)//Wrap Content
            //{
            //    var heightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, -(TopMargin + BottomMargin));
            //    heightConstraint.Priority = 999f;
            //    heightConstraint.Active = true;
            //}
            //else if (this.Height != -1 && this.Height != -2)//Raw value user input
            //{
            //    var heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.Height);
            //    heightConstraint.Priority = 999f;
            //    heightConstraint.Active = true;
            //}

            //var maxHeightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, -(TopMargin + BottomMargin));
            //maxHeightConstraint.Priority = 1000f;
            //maxHeightConstraint.Active = true;




            //Position
            if (RelativeView != null)
            {
                setPosition();
            }
            else
            {
                dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
                dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
            }

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
            topBorderButtonsStack = new UIView() { BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false, Hidden = true };
            buttonsStack.AddSubview(topBorderButtonsStack);
            topBorderButtonsStack.LeadingAnchor.ConstraintEqualTo(buttonsStack.LeadingAnchor).Active = true;
            topBorderButtonsStack.TrailingAnchor.ConstraintEqualTo(buttonsStack.TrailingAnchor).Active = true;
            topBorderButtonsStack.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;


            positiveButton = new RSButtonNative(RSPopupButtonTypeEnum.Positive, UIColor.SystemBlueColor) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) };
            neutralButton = new RSButtonNative(RSPopupButtonTypeEnum.Neutral, UIColor.SystemBlueColor) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) };
            destructiveButton = new RSButtonNative(RSPopupButtonTypeEnum.Destructive, UIColor.SystemRedColor) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(12, 10, 12, 10) };


            buttonsStack.AddArrangedSubview(destructiveButton);
            buttonsStack.AddArrangedSubview(neutralButton);
            buttonsStack.AddArrangedSubview(positiveButton);

            buttonsCount = 3;
        }

        //Set title
        public void SetTitle(string title, float fontSize)
        {
            if (string.IsNullOrEmpty(title))
                titleText.Hidden = true;

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
            if (string.IsNullOrEmpty(message))
                messageLabel.Hidden = true;

            messageLabel.BackgroundColor = UIColor.Clear;
            messageLabel.TextColor = UIColor.DarkGray;
            messageLabel.Text = message;
            messageLabel.Font = UIFont.SystemFontOfSize(fontSize);
            messageLabel.TextAlignment = UITextAlignment.Center;
            messageLabel.UserInteractionEnabled = false;
            messageLabel.ScrollEnabled = false;
            messageLabel.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
            messageLabel.TextContainerInset = new UIEdgeInsets(15, 10, 15, 10);
        }

        //Set and add custom view 
        private void SetCustomView()
        {
            var renderer = Platform.CreateRenderer(CustomView);
            Platform.SetRenderer(CustomView, renderer);
            var convertView = new Extensions.FormsToiosCustomDialogView(CustomView, renderer, this.DialogView);
            this.contentStack.AddArrangedSubview(convertView);
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter = null)
        {
            topBorderButtonsStack.Hidden = false;

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
                AddBorder(destructiveButton);
            }
            else
            {

            }
        }


        //Update constraints
        private void updateConstriants(CGRect position)
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                var constant = (int)Math.Abs(position.X) - LeftMargin;
                this.Frame.Width.ToString();
                maxWidthConstraint.Constant = constant;
                dialogStack.UpdateConstraints();
                messageLabel.UpdateConstraints();
            }
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                var constant = ((int)Math.Abs(position.X) + relativeViewAsNativeView.Frame.Width + RightMargin);
                this.Frame.Width.ToString();
                maxWidthConstraint.Constant = - constant;
                dialogStack.UpdateConstraints();
                messageLabel.UpdateConstraints();
            }

            
            //messageLabel.UpdateConstraints();
        }

        //Size
        private void setSize(CGRect position)
        {
            //Width
            if (this.Width == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    var constant = (LeftMargin + (int)Math.Abs(position.X));
                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.WidthAnchor, 0f, constant);
                    maxWidthConstraint.Priority = 999f;
                    maxWidthConstraint.Active = true;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    var constant = ((int)Math.Abs(position.X) + relativeViewAsNativeView.Frame.Width + RightMargin);
                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.WidthAnchor, 1f, -constant);
                    maxWidthConstraint.Priority = 999f;
                    maxWidthConstraint.Active = true;
                }
                else
                {
                    widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.WidthAnchor, 1f, - (LeftMargin + RightMargin));
                    widthConstraint.Priority = 1000f;
                    widthConstraint.Active = true;
                }
            }
            else if (this.Width == -2)//Wrap Content
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    var constant = ((int)Math.Abs(position.X) - LeftMargin);
                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(constant);
                    maxWidthConstraint.Priority = 999f;
                    maxWidthConstraint.Active = true;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    var constant = ((int)Math.Abs(position.X) + relativeViewAsNativeView.Frame.Width + RightMargin);
                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, -constant);
                    maxWidthConstraint.Priority = 999f;
                    maxWidthConstraint.Active = true;
                }
                else
                {
                    widthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, - (LeftMargin + RightMargin));
                    widthConstraint.Priority = 999f;
                    widthConstraint.Active = true;


                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, - (LeftMargin + RightMargin));
                    maxWidthConstraint.Priority = 1000f;
                    maxWidthConstraint.Active = true;
                }
            }
            else if (this.Width != -1 && this.Width != -2)//Raw value user input
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    var constant = (int)Math.Abs(position.X) + LeftMargin;
                    widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.Width);
                    widthConstraint.Priority = 999f;
                    widthConstraint.Active = true;


                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, -constant);
                    maxWidthConstraint.Priority = 1000f;
                    maxWidthConstraint.Active = true;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    var constant = (int)Math.Abs(position.X) + relativeViewAsNativeView.Frame.Width + RightMargin;
                    widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.Width);
                    widthConstraint.Priority = 999f;
                    widthConstraint.Active = true;


                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, -constant);
                    maxWidthConstraint.Priority = 1000f;
                    maxWidthConstraint.Active = true;
                }
                else
                {
                    widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.Width - (LeftMargin + RightMargin));
                    widthConstraint.Priority = 999f;
                    widthConstraint.Active = true;

                    maxWidthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, -(LeftMargin + RightMargin));
                    maxWidthConstraint.Priority = 1000f;
                    maxWidthConstraint.Active = true;
                }
            }



            //Height
            if (this.Height == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    maxHeightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.HeightAnchor, 1f, -(TopMargin + BottomMargin));
                    maxHeightConstraint.Priority = 999f;
                    maxHeightConstraint.Active = true;
                }
                else
                {
                    heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                    heightConstraint.Priority = 1000f;
                    heightConstraint.Active = true;
                }
            }
            else if (this.Height == -2)//Wrap Content
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    maxHeightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                    maxHeightConstraint.Priority = 999f;
                    maxHeightConstraint.Active = true;
                }
                else
                {
                    heightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                    heightConstraint.Priority = 1000f;
                    heightConstraint.Active = true;
                }
            }
            else if (this.Height != -1 && this.Height != -2)//Raw value user input
            {
                heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.Height);
                heightConstraint.Priority = 999f;
                heightConstraint.Active = true;

                maxHeightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                maxHeightConstraint.Priority = 1000f;
                maxHeightConstraint.Active = true;
            }
        }

        //Position
        private void setPosition()
        {
            if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                positionXConstraint = dialogStack.TrailingAnchor.ConstraintEqualTo(relativeViewAsNativeView.LeadingAnchor);
                positionXConstraint.Active = true;
                positionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                positionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                positionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(relativeViewAsNativeView.TrailingAnchor);
                positionXConstraint.Active = true;
                positionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                positionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                positionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                positionXConstraint.Active = true;
                positionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(relativeViewAsNativeView.BottomAnchor);
                positionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                positionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                positionXConstraint.Active = true;
                positionYConstraint = dialogStack.BottomAnchor.ConstraintEqualTo(relativeViewAsNativeView.TopAnchor);
                positionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                positionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(relativeViewAsNativeView.LeadingAnchor);
                positionXConstraint.Active = true;
                positionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(relativeViewAsNativeView.TopAnchor);
                positionYConstraint.Active = true;
            }
        }

        //Update position
        private void updatePosition(CGRect position)
        {
            if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                //X Position
                if ((relativeViewAsNativeView.Frame.GetMidX() + DialogView.Frame.Width / 2) > (this.Frame.Width - RightMargin - LeftMargin))
                {
                    var constant = (relativeViewAsNativeView.Frame.GetMidX() + DialogView.Frame.Width / 2) - (this.Frame.Width - RightMargin - LeftMargin);
                    positionXConstraint.Constant = -constant;
                }
                else if((relativeViewAsNativeView.Frame.GetMidX() - DialogView.Frame.Width / 2) < LeftMargin)
                {
                    var constant = (relativeViewAsNativeView.Frame.GetMidX() - DialogView.Frame.Width / 2);
                    positionXConstraint.Constant = (nfloat)Math.Abs(constant);
                }
                else
                    positionXConstraint.Constant = 0;


                //Y Position TODO
                if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                {
                    var bottomPosition = Math.Abs(position.Y) - (position.Height - BottomMargin - TopMargin) + DialogView.Frame.Height;
                    var topPosition = Math.Abs(position.Y);


                    if (bottomPosition > (this.Frame.Height - TopMargin - BottomMargin) && bottomPosition > TopMargin && topPosition < (this.Frame.Height - BottomMargin - TopMargin))
                    {
                        //TODO change arrow
                        var constant = position.Height + DialogView.Frame.Height;
                        positionYConstraint.Constant = -constant;
                    }
                    else if(bottomPosition > this.Frame.Height - BottomMargin)
                    {
                        var constant = (Math.Abs(position.Y - position.Height) + DialogView.Frame.Height) - (this.Frame.Height - BottomMargin);
                        positionYConstraint.Constant = -(nfloat)constant;
                    }
                    else
                        positionYConstraint.Constant = 0;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    var topPosition = (Math.Abs(position.Y) - DialogView.Frame.Height);
                    var bottomPosition = (Math.Abs(position.Y) + DialogView.Frame.Height);

                    if (topPosition < TopMargin && topPosition < (this.Frame.Height + TopMargin) && bottomPosition < (this.Frame.Height - BottomMargin))
                    {
                        //TODO change arrow
                        var constant = position.Height + DialogView.Frame.Height;
                        positionYConstraint.Constant = constant;
                    }
                    else if(topPosition < TopMargin)
                    {
                        var constant = (Math.Abs(position.Y) - this.Frame.Height) + BottomMargin;
                        positionYConstraint.Constant = -(nfloat)constant;
                    }
                    else if (bottomPosition > (this.Frame.Height - BottomMargin))
                    {
                        var constant = (Math.Abs(position.Y) - this.Frame.Height) + BottomMargin;
                        positionYConstraint.Constant = -(nfloat)constant;
                    }
                    else
                    {
                        positionYConstraint.Constant = 0;
                    }
                }
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                //X Position
                if ((relativeViewAsNativeView.Frame.X + DialogView.Frame.Width) > (this.Frame.Width - RightMargin - LeftMargin))
                {
                    var constant = (relativeViewAsNativeView.Frame.X + DialogView.Frame.Width) - (this.Frame.Width - RightMargin - LeftMargin);
                    positionXConstraint.Constant = -constant;
                }
                else
                    positionXConstraint.Constant = 0;

                //Y Position
                if((Math.Abs(position.Y) + DialogView.Frame.Height) > this.Frame.Height - TopMargin - BottomMargin)
                {
                    var constant = (Math.Abs(position.Y) + DialogView.Frame.Height) - (this.Frame.Height - BottomMargin);
                    positionYConstraint.Constant = -(nfloat)constant;
                }
                else
                    positionYConstraint.Constant = 0;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                if ((Math.Abs(position.Y - position.Height / 2) + DialogView.Frame.Height / 2) > (this.Frame.Height - TopMargin - BottomMargin))
                {
                    var constant = (Math.Abs(position.Y - position.Height / 2) + DialogView.Frame.Height / 2) - (this.Frame.Height - BottomMargin);
                    positionYConstraint.Constant = -(nfloat)constant;
                }
                else if ((Math.Abs(position.Y - position.Height / 2) - DialogView.Frame.Height / 2) < TopMargin)
                {
                    var constant = position.Y - position.Height / 2 + DialogView.Frame.Height / 2 + TopMargin;
                    positionYConstraint.Constant = (nfloat)constant;
                }
                else
                    positionYConstraint.Constant = 0;
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


            this.Dispose();
        }

        //ShowPopup
        public void ShowPopup()
        {
            SetupBackgroundView();

            if (RelativeView != null)
                SetupArrow();

            SetupDialogView();
            SetupDialogStack();
            SetupContentStack();
            SetTitle(Title, 18);
            SetMessage(Message, 12);
            if (CustomView != null)
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

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();


            if(RelativeView != null)
            {
                orientation = UIDevice.CurrentDevice.Orientation;
                var position = relativeViewAsNativeView.ConvertRectFromView(relativeViewAsNativeView.Bounds, this.mainView);
                updateConstriants(position);
                updatePosition(position);
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

                if (buttonsStack.ArrangedSubviews[buttonsCount - 1] != button)
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

            if (disposing)
            {
                foreach (var item in this.contentStack.Subviews)
                {
                    item.Dispose();
                }
            }

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

        public RSButtonNative(RSPopupButtonTypeEnum rSPopupButtonType, UIColor titleColor) : base()
        {
            this.rSPopupButtonType = rSPopupButtonType;
            this.SetTitleColor(titleColor, UIControlState.Normal);
            this.SetTitleColor(titleColor.ColorWithAlpha(0.5f), UIControlState.Highlighted);
            this.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            this.AddTarget(ButtonEventHandler, UIControlEvent.TouchUpInside);
        }

        //public RSButtonNative(UIButtonType type, RSPopupButtonTypeEnum rSPopupButtonType) : base(type)
        //{
        //    this.rSPopupButtonType = rSPopupButtonType;
        //    this.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
        //    this.AddTarget(ButtonEventHandler, UIControlEvent.TouchUpInside);
        //}

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




    //Arrow
    public class RSPopupArrow : UIView
    {
        public RSPopupArrow() : base()
        {
            this.BackgroundColor = UIColor.Clear;
            this.Layer.ShadowColor = UIColor.Black.CGColor;
            this.Layer.ZPosition = 100f;
            this.Layer.ShadowRadius = 1f;
            this.Layer.ShadowOpacity = 0.1f;
            this.Layer.ShadowOffset = new CGSize(0, -1.1f);
            //this.Transform = CGAffineTransform.MakeRotation(90 * MathF.PI / 180);
    }


        public override CGSize IntrinsicContentSize
        {
            get
            {
                return new CGSize(20, 15);
            }
        }

        public override void Draw(CGRect rect)
        {
            //base.Draw(rect);

            var context = UIGraphics.GetCurrentContext();
            UIColor.White.SetFill();
            UIColor.White.SetStroke();

            var path = new CGPath();

            path.AddLines(new CGPoint[]{
            new CGPoint (0, this.IntrinsicContentSize.Height),
            new CGPoint (IntrinsicContentSize.Width / 2, 0),
            new CGPoint (IntrinsicContentSize.Width, IntrinsicContentSize.Height)});

            path.CloseSubpath();

            context.AddPath(path);
            context.DrawPath(CGPathDrawingMode.FillStroke);



        }
    }
}

