using System;
using System.Runtime.Remoting.Contexts;
using System.Windows.Input;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using SkiaSharp;
using SpriteKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPopupRenderer : UIView, IDialogPopup
    {
        public RSPopup rSPopup { get; set; }

        private UIView mainView { get; set; }
        private UIView BackgroundView { get; set; }
        private RSDialogView DialogView { get; set; }
        private TestClass dialogStack { get; set; }
        private RScontentStack contentStack { get; set; }
        private RSUIScrollView contentScrollView { get; set; }
        private UIStackView buttonsStack { get; set; }
        private UIView topBorderButtonsStack;
        public RSButtonNative positiveButton;
        public RSButtonNative destructiveButton;
        public RSButtonNative neutralButton;
        private UILabel titleText;
        private UILabel messageLabel;
        public bool IsAnimating;
        public bool HasArrow { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Forms.View RelativeView { get; set; }
        public Forms.VisualElement CustomView { get; set; }
        private IVisualElementRenderer renderer;
        public Forms.Color BorderFillColor { get; set; }
        public float DimAmount { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float BorderRadius { get; set; }
        public bool ShadowEnabled { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool UserSetPosition { get; set; }
        public RSPopupAnimationEnum RSPopupAnimationEnum { get; set; }
        public RSPopupPositionEnum RSPopupPositionEnum { get; set; }
        public RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
        public RSPopupStyleEnum RSPopupStyleEnum { get; set; }
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
        private NSLayoutConstraint widthConstraint;
        private NSLayoutConstraint heightConstraint; 
        private NSLayoutConstraint dialogPositionXConstraint;
        private NSLayoutConstraint dialogPositionYConstraint;
        private NSLayoutConstraint thisBottomConstraint;
        private NSObject keyboardObserverOpen;
        private NSObject keyboardObserverClose;
        public CGRect CurrentDialogPosition = new CGRect();
        private NSLayoutConstraint customViewWidthConstraint;
        private NSLayoutConstraint customViewHeightConstraint;
        private ContentPage customViewContentPage;
        private nfloat KeyboardPosition;
        private NSLayoutConstraint dialogViewBottomConstraint;
        private NSLayoutConstraint dialogViewTopConstraint;
        private NSLayoutConstraint dialogViewLeadingConstraint;
        private NSLayoutConstraint dialogViewTrailingConstraint;
        private nfloat arrowSize;


        //Constructor
        public RSPopupRenderer() : base()
        {
            buttonsCount = 0;
            BorderRadius = 12;
            ShadowEnabled = true;
            HasArrow = false;
            arrowSize = 10;

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
            thisBottomConstraint = this.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor);
            thisBottomConstraint.Priority = 999;
            thisBottomConstraint.Active = true;

            //this.LeadingAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.LeadingAnchor).Active = true;
            //this.TrailingAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
            //this.TopAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.TopAnchor).Active = true;
            //thisBottomConstraint = this.BottomAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.BottomAnchor);
            //thisBottomConstraint.Priority = 999;
            //thisBottomConstraint.Active = true;


            this.BackgroundColor = UIColor.Gray.ColorWithAlpha(0.5f);

            //BackgroundView
            BackgroundView = new UIView();
            AddSubview(BackgroundView);

            //DialogVIew
            DialogView = new RSDialogView(BorderRadius, this.BorderFillColor);
            AddSubview(DialogView);

            //DialogStack
            dialogStack = new TestClass();
            DialogView.AddSubview(dialogStack);

            //Title
            titleText = new UILabel();
            dialogStack.AddArrangedSubview(titleText);

            // Content scrollView
            contentScrollView = new RSUIScrollView();
            dialogStack.AddArrangedSubview(contentScrollView);

            // ContentStack
            contentStack = new RScontentStack();
            contentScrollView.AddSubview(contentStack);

            //Message
            messageLabel = new UILabel();
            contentStack.AddArrangedSubview(messageLabel);

            //Buttons stack
            buttonsStack = new UIStackView();
            dialogStack.AddArrangedSubview(buttonsStack);
        }

        //Setup backgroundView
        private void SetupBackgroundView()
        {
            BackgroundView.TranslatesAutoresizingMaskIntoConstraints = false;
            BackgroundView.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor).Active = true;
            BackgroundView.TrailingAnchor.ConstraintEqualTo(mainView.TrailingAnchor).Active = true;
            BackgroundView.TopAnchor.ConstraintEqualTo(mainView.TopAnchor).Active = true;
            BackgroundView.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor).Active = true;

            BackgroundView.BackgroundColor = UIColor.Black.ColorWithAlpha(DimAmount);
        }

        //Setup DialogView
        private void SetupDialogView()
        {
            DialogView.SetBorderFillColor(this.BorderFillColor);

            // Set parameters in DialogView subclass used for arrow position
            DialogView.SetRSPopupRenderer(this);
            DialogView.ArrowSide = RSPopupPositionSideEnum;

            //Constraints
            DialogView.TranslatesAutoresizingMaskIntoConstraints = false;

            ////Set size
            //DialogView.WidthAnchor.ConstraintEqualTo(dialogStack.WidthAnchor).Active = true;
            //DialogView.HeightAnchor.ConstraintEqualTo(dialogStack.HeightAnchor).Active = true;


            dialogViewLeadingConstraint = DialogView.LeadingAnchor.ConstraintEqualTo(dialogStack.LeadingAnchor);
            dialogViewLeadingConstraint.Active = true;
            dialogViewTrailingConstraint = DialogView.TrailingAnchor.ConstraintEqualTo(dialogStack.TrailingAnchor);
            dialogViewTrailingConstraint.Active = true;
            dialogViewTopConstraint = DialogView.TopAnchor.ConstraintEqualTo(dialogStack.TopAnchor);
            dialogViewTopConstraint.Active = true;
            dialogViewBottomConstraint = DialogView.BottomAnchor.ConstraintEqualTo(dialogStack.BottomAnchor);
            dialogViewBottomConstraint.Active = true;



            //////Position dialog
            //DialogView.CenterXAnchor.ConstraintEqualTo(dialogStack.CenterXAnchor).Active = true;
            //DialogView.CenterYAnchor.ConstraintEqualTo(dialogStack.CenterYAnchor).Active = true;


            UIPanGestureRecognizer uISwipeGestureRecognizer = new UIPanGestureRecognizer((UIPanGestureRecognizer gesture) =>
            {
                var pos = gesture.TranslationInView(mainView);

                if (pos.Y > 0)
                    DialogView.Transform = CGAffineTransform.MakeTranslation(DialogView.Transform.Tx, pos.Y);

                if (gesture.State == UIGestureRecognizerState.Ended)
                {
                    if(pos.Y > DialogView.Frame.Height * 0.3)
                        this.Dismiss(true);
                    else
                        UIView.Animate(0.10, 0, UIViewAnimationOptions.CurveEaseInOut, () =>{ DialogView.Transform = CGAffineTransform.MakeTranslation(DialogView.Transform.Tx, 0); }, null);
                }
            });
            DialogView.AddGestureRecognizer(uISwipeGestureRecognizer);

        }

        //Setup stackDialog
        private void SetupDialogStack()
        {
            dialogStack.Axis = UILayoutConstraintAxis.Vertical;
            dialogStack.Distribution = UIStackViewDistribution.Fill;
            dialogStack.Spacing = 10;
            dialogStack.LayoutMarginsRelativeArrangement = true;
            dialogStack.InsetsLayoutMarginsFromSafeArea = false;
            (dialogStack as TestClass).rSPopupRenderer = this;

            // Get Relative view position including eventual margins
            CGRect position = new CGRect();
            if (RelativeView != null)
            {
                var relativeViewAsNativeRenderer = Platform.GetRenderer(RelativeView);
                relativeViewAsNativeView = relativeViewAsNativeRenderer.NativeView;
                position = relativeViewAsNativeView.ConvertRectFromView(relativeViewAsNativeView.Bounds, this.mainView);
                this.HasArrow = true;
            }

            //Constraints
            dialogStack.TranslatesAutoresizingMaskIntoConstraints = false;

            // Set size
            setSize(position);

            // Set margins
            setMargins();

            // Set position
            setDialogPosition();
        }

        // Set dialogstack margins
        private void setMargins()
        {
            //if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right && HasArrow)
            //    dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 20, 0, 10);
            //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left && HasArrow)
            //    dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 20);
            //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top && HasArrow)
            //    dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 10);
            //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom && HasArrow)
            //    dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(20, 10, 0, 10);
            //else
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(15, 10, 0, 10);
        }

        // Set ContentStack
        private void setContentStack()
        {
            contentStack.Distribution = UIStackViewDistribution.Fill;
            contentStack.Axis = UILayoutConstraintAxis.Vertical;
            contentStack.Spacing = 5;
            contentStack.TranslatesAutoresizingMaskIntoConstraints = false;
            contentStack.CenterXAnchor.ConstraintEqualTo(contentScrollView.CenterXAnchor).Active = true;
            Extensions.ViewExtensions.EdgeTo(contentScrollView, contentStack, true, true, true, true);
        }

        // Set Content scrollView
        private void setContentScrollView()
        {
            contentScrollView.Bounces = false;
            contentScrollView.ShowsVerticalScrollIndicator = false;
            contentScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
            contentScrollView.SetContentCompressionResistancePriority(1, UILayoutConstraintAxis.Vertical);
        }

        //Create buttons 
        private void CreateButtonsStack()
        {
            buttonsStack.Axis = UILayoutConstraintAxis.Horizontal;
            buttonsStack.Distribution = UIStackViewDistribution.FillEqually;

            // Separator
            topBorderButtonsStack = new UIView() { BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false, Hidden = true };
            buttonsStack.AddSubview(topBorderButtonsStack);
            topBorderButtonsStack.LeadingAnchor.ConstraintEqualTo(buttonsStack.LeadingAnchor, -10).Active = true;
            topBorderButtonsStack.TrailingAnchor.ConstraintEqualTo(buttonsStack.TrailingAnchor, + 10).Active = true;
            topBorderButtonsStack.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;

            // Buttons
            positiveButton = new RSButtonNative(RSPopupButtonTypeEnum.Positive, UIColor.SystemBlue) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(8, 10, 8, 10) };
            neutralButton = new RSButtonNative(RSPopupButtonTypeEnum.Neutral, UIColor.SystemBlue) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(8, 10, 8, 10) };
            destructiveButton = new RSButtonNative(RSPopupButtonTypeEnum.Destructive, UIColor.SystemBlue) { Hidden = true, ContentEdgeInsets = new UIEdgeInsets(8, 10, 8, 10) };

            positiveButton.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);
            neutralButton.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);
            destructiveButton.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);


            // Set minimum button height
            positiveButton.TranslatesAutoresizingMaskIntoConstraints = false;
            neutralButton.TranslatesAutoresizingMaskIntoConstraints = false;
            destructiveButton.TranslatesAutoresizingMaskIntoConstraints = false;
            positiveButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(42).Active = true;
            neutralButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(42).Active = true;
            destructiveButton.HeightAnchor.ConstraintGreaterThanOrEqualTo(42).Active = true;


            buttonsStack.AddArrangedSubview(destructiveButton);
            buttonsStack.AddArrangedSubview(neutralButton);
            buttonsStack.AddArrangedSubview(positiveButton);
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
            titleText.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Vertical);
            //titleText.ScrollEnabled = false;
            //titleText.TextContainerInset = new UIEdgeInsets(10, 5, 0, 5);
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
            messageLabel.LineBreakMode = UILineBreakMode.WordWrap;
            messageLabel.Lines = 0;
            messageLabel.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);

            //messageLabel.ScrollEnabled = false;
            //messageLabel.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
        }

        //Set and add custom view 
        private void SetCustomView()
        {
            // Set page to custom view


            if (CustomView is Page)
            {
                CustomView.BackgroundColor = Color.Transparent;
                renderer = Platform.CreateRenderer(CustomView);
                Platform.SetRenderer(CustomView, renderer);
            }
            else
            {
                customViewContentPage = new ContentPage();
                customViewContentPage.BackgroundColor = Color.Transparent;
                customViewContentPage.Content = CustomView as Forms.View;
                renderer = Platform.CreateRenderer(customViewContentPage);
                Platform.SetRenderer(customViewContentPage, renderer);
            }


            // Give size to convertedView so it can be layed out correctly in the uistackview
            // The prioity here is lower so if parent which is a uistackview is smaller in width, he can fullfill his constraints
            // Widht
            renderer.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;
            customViewWidthConstraint = renderer.NativeView.WidthAnchor.ConstraintEqualTo(0);
            customViewWidthConstraint.Priority = 750;
            customViewWidthConstraint.Active = true;

            // Height
            customViewHeightConstraint = renderer.NativeView.HeightAnchor.ConstraintEqualTo(0);
            customViewHeightConstraint.Priority = 999;
            customViewHeightConstraint.Active = true;


            // Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            var offsetX = dialogStack.DirectionalLayoutMargins.Leading + dialogStack.DirectionalLayoutMargins.Trailing;
            var offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;


            // Set max size
            var maxSize = new CGSize(mainView.Frame.Width - mainView.SafeAreaInsets.Left - mainView.SafeAreaInsets.Right -offsetX - (nfloat)LeftMargin - (nfloat)RightMargin,
                                 mainView.Frame.Height - mainView.SafeAreaInsets.Top - mainView.SafeAreaInsets.Bottom - offsetY - (nfloat)TopMargin - (nfloat)BottomMargin);

            // Get required size for forms view
            var sizeRequest = (renderer.Element as ContentPage).Content.Measure(maxSize.Width, maxSize.Height, Forms.MeasureFlags.IncludeMargins);

            // Layout forms view -- need to give some size here or else it won't render -- the correct size is done later in layoutCustomView()
            renderer.Element.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));

            contentStack.AddArrangedSubview(renderer.NativeView);
            contentStack.CustomViewContentPage = renderer.Element as ContentPage;

            // set keyboard KeyboardObservers
            AddKeyboardObservers();

            // Listen to CustomView changes and update
            renderer.Element.MeasureInvalidated += CustomViewContentPage_MeasureInvalidated;
        }

        private void CustomViewContentPage_MeasureInvalidated(object sender, EventArgs e)
        {
            this.LayoutSubviews();
            //layoutCustomView();
        }

        private void layoutCustomView()
        {
            // Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            var offsetX = dialogStack.DirectionalLayoutMargins.Leading + dialogStack.DirectionalLayoutMargins.Trailing;
            var offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;

            // Set max size
            var maxSize = new CGSize(mainView.Frame.Width - mainView.SafeAreaInsets.Left - mainView.SafeAreaInsets.Right -offsetX - (nfloat)LeftMargin - (nfloat)RightMargin,
                                 mainView.Frame.Height - mainView.SafeAreaInsets.Top - mainView.SafeAreaInsets.Bottom - offsetY - (nfloat)TopMargin - (nfloat)BottomMargin);

            // Get and Set required size for forms view
            var sizeRequest = (renderer.Element as ContentPage).Content.Measure(maxSize.Width, maxSize.Height, Forms.MeasureFlags.IncludeMargins);
            //var sizeRequest = (renderer.Element as ContentPage).Content.Measure(mainView.Frame.Width + 30, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);


            //if (sizeRequest.Request.Width > maxSize.Width)
            //    sizeRequest.Request = new Size(maxSize.Width, sizeRequest.Request.Height);

            //if (sizeRequest.Request.Height > maxSize.Height)
            //    sizeRequest.Request = new Size(sizeRequest.Request.Width, sizeRequest.Request.Height);


            // Calculate final width
            if (this.Width == -1)
            {
                customViewWidthConstraint.Constant = maxSize.Width;
                renderer.Element.Layout(new Rectangle(0, 0, maxSize.Width, sizeRequest.Request.Height));
            }
            else if (this.Width == -2)
            {
                customViewWidthConstraint.Constant = (nfloat)sizeRequest.Request.Width;
                renderer.Element.Layout(new Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            }
            else
            {
                nfloat maxWidth = mainView.Frame.Width - LeftMargin - RightMargin - offsetX;
                nfloat projectedWidth = (nfloat)this.Width - offsetX;

                if (projectedWidth > maxWidth)
                    projectedWidth = maxWidth;

                customViewWidthConstraint.Constant = projectedWidth;
                renderer.Element.Layout(new Rectangle(0, 0, projectedWidth, sizeRequest.Request.Height));
            }

            customViewHeightConstraint.Constant = (nfloat)sizeRequest.Request.Height;
        }

        // Set arrow true or false
        private void shouldShowArrow()
        {
            // Width
            if (this.Width == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                    HasArrow = false;
                else
                    HasArrow = true;
            }
            // Height
            else if (this.Height == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                    HasArrow = false;
                else
                    HasArrow = true;
            }
            else
                HasArrow = true;
        }

        // Keyboard show and hide Observers
        private void AddKeyboardObservers()
        {
            keyboardObserverOpen = UIKeyboard.Notifications.ObserveDidShow((handler, args) =>
            {
                thisBottomConstraint.Constant = -args.FrameEnd.Height + mainView.SafeAreaInsets.Bottom;
                dialogStack.LayoutIfNeeded();

                //var posDialogBottomY = Math.Abs(dialogStack.ConvertRectFromView(dialogStack.Bounds, this.mainView).Y) + dialogStack.Frame.Height;
                //if (posDialogBottomY > args.FrameEnd.Y)
                //{
                //    KeyboardPosition = args.FrameEnd.Height;
                //    thisBottomConstraint.Constant = -args.FrameEnd.Height + mainView.SafeAreaInsets.Bottom;
                //    dialogStack.LayoutIfNeeded();
                //}
            });

            keyboardObserverClose = UIKeyboard.Notifications.ObserveDidHide((handler, args) =>
            {
                //KeyboardPosition = 0;

                thisBottomConstraint.Constant = 0;
                dialogStack.LayoutIfNeeded();
            });
        }
        private void RemoveKeyboardObservers()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardObserverOpen);
            NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardObserverClose);
        }

        //Set native view 
        public void SetNativeView(UIView nativeView)
        {
            dialogStack.InsertArrangedSubview(nativeView, (nuint)dialogStack.ArrangedSubviews.Length - 1);
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter = null)
        {
            buttonsCount++;
            topBorderButtonsStack.Hidden = false;

            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton.dialog = this;
                positiveButton.Command = command;
                positiveButton.Hidden = false;
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.SetTitleColor(UIColor.SystemBlue, UIControlState.Normal);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton.dialog = this;
                neutralButton.Command = command;
                neutralButton.Hidden = false;
                neutralButton.SetTitle(title, UIControlState.Normal);
                neutralButton.SetTitleColor(UIColor.SystemBlue, UIControlState.Normal);
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton.dialog = this;
                destructiveButton.Command = command;
                destructiveButton.Hidden = false;
                destructiveButton.SetTitle(title, UIControlState.Normal);
            }
        }

        private void AddButtonDivider()
        {
            if (buttonsCount <= 1)
                return;

            for(int i = 0; i < buttonsStack.ArrangedSubviews.Length - 1; i++)
            {
                if(i == (buttonsStack.ArrangedSubviews.Length - 2))
                {
                    if ((buttonsStack.ArrangedSubviews[i + 1] as UIButton).Hidden)
                        return;
                }

                AddBorder(buttonsStack.ArrangedSubviews[i] as UIButton);
            }
        }

        // Size
        private void setSize(CGRect position)
        {
            // Width
            if (this.Width == -1)//Match Parent
            {
                widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.WidthAnchor, 1f, - (LeftMargin + RightMargin));
                //widthConstraint.Priority = 1000f;
                widthConstraint.Active = true;
            }
            else if (this.Width == -2)//Wrap Content
            {
                widthConstraint = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, - (LeftMargin + RightMargin));
                //widthConstraint.Priority = 999f;
                widthConstraint.Active = true;
            }
            else if (this.Width != -1 && this.Width != -2)//Raw value user input
            {
                var w = dialogStack.WidthAnchor.ConstraintLessThanOrEqualTo(this.WidthAnchor, 1f, -(LeftMargin + RightMargin));
                w.Active = true;

                widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.Width);
                widthConstraint.Priority = 999f;
                widthConstraint.Active = true;
            }


            // Height
            if (this.Height == -1)//Match Parent
            {
                heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                //heightConstraint.Priority = 1000f;
                heightConstraint.Active = true;
            }
            else if (this.Height == -2)//Wrap Content
            {
                heightConstraint = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, - (TopMargin + BottomMargin));
                //heightConstraint.Priority = 1000f;
                heightConstraint.Active = true;
            }
            else if (this.Height != -1 && this.Height != -2)//Raw value user input
            {
                var h = dialogStack.HeightAnchor.ConstraintLessThanOrEqualTo(this.HeightAnchor, 1f, -(TopMargin + BottomMargin));
                h.Active = true;

                heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.Height);
                heightConstraint.Priority = 999f;
                heightConstraint.Active = true;
            }
        }

        /// <summary>
        /// Converts coordinates to correct ones
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private nfloat convertCoordinates(nfloat position)
        {
            if (position >= 0)
                return -position;
            else
                return (nfloat)Math.Abs(position);
        }

        // Dialog initial Position / create constraints and update them later
        private void setDialogPosition()
        {
            if (RelativeView != null)
            {
                //if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                //{
                //    dialogPositionXConstraint = dialogStack.TrailingAnchor.ConstraintEqualTo(relativeViewAsNativeView.LeadingAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}
                //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                //{
                //    dialogPositionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(relativeViewAsNativeView.TrailingAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}
                //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                //{
                //    dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(relativeViewAsNativeView.BottomAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}
                //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                //{
                //    dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.BottomAnchor.ConstraintEqualTo(relativeViewAsNativeView.TopAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}
                //else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
                //{
                //    dialogPositionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(mainView.TopAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}
                //else
                //{
                //    dialogPositionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor);
                //    dialogPositionXConstraint.Active = true;
                //    dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(mainView.TopAnchor);
                //    dialogPositionYConstraint.Active = true;
                //}


                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Center)
                {
                    dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor);
                    dialogPositionXConstraint.Active = true;
                    dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor);
                    dialogPositionYConstraint.Active = true;
                }
                else
                {
                    dialogPositionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor);
                    dialogPositionXConstraint.Active = true;
                    dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(mainView.TopAnchor);
                    dialogPositionYConstraint.Active = true;
                }
            }
            else
            {
                if (RSPopupPositionEnum == RSPopupPositionEnum.Center)
                {
                    dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
                    dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor);
                    dialogPositionYConstraint.Active = true;
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Bottom)
                {
                    dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
                    dialogStack.BottomAnchor.ConstraintEqualTo(this.BottomAnchor).Active = true;
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Top)
                {
                    dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
                    dialogStack.TopAnchor.ConstraintEqualTo(this.TopAnchor).Active = true;
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Left)
                {
                    dialogStack.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor).Active = true;
                    dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Right)
                {
                    dialogStack.TrailingAnchor.ConstraintEqualTo(this.TrailingAnchor).Active = true;
                    dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
                }
            }
        }

        /// <summary>
        /// Used for position calculations
        /// </summary>
        /// <param name="position"></param>
        public void setConstraintPosition(CGRect position)
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                CurrentDialogPosition.X = convertCoordinates(position.X) - dialogStack.Frame.Width - arrowSize;
                CurrentDialogPosition.Y = convertCoordinates(position.Y) + position.Height / 2 - dialogStack.Frame.Height / 2;
                dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
                dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;
                dialogViewTrailingConstraint.Constant = arrowSize;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                CurrentDialogPosition.X = convertCoordinates(position.X) + position.Width + arrowSize;
                CurrentDialogPosition.Y = convertCoordinates(position.Y) + position.Height / 2 - dialogStack.Frame.Height / 2;
                dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
                dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;
                dialogViewLeadingConstraint.Constant = -arrowSize;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                CurrentDialogPosition.X = convertCoordinates(position.X) + (position.Width / 2) - dialogStack.Frame.Width / 2;
                CurrentDialogPosition.Y = convertCoordinates(position.Y) + arrowSize + position.Height;
                dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
                dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;
                dialogViewTopConstraint.Constant = -arrowSize;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                CurrentDialogPosition.X = convertCoordinates(position.X) + (position.Width / 2) - dialogStack.Frame.Width / 2;
                CurrentDialogPosition.Y = convertCoordinates(position.Y) - arrowSize - dialogStack.Frame.Height;
                dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
                dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;
                dialogViewBottomConstraint.Constant = arrowSize;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                CurrentDialogPosition.X = convertCoordinates(position.X);
                CurrentDialogPosition.Y = convertCoordinates(position.Y);
                dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
                dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;
            }
        }

        // Correct dialog position if needed
        public void updatePosition(CGRect position)
        {
            var minXPositionAllowed = this.Frame.Left + LeftMargin;
            var maxXPositionAllowed = this.Frame.Right - RightMargin;
            var minYPositionAllowed = this.Frame.Top + TopMargin;
            var maxYPositionAllowed = this.Frame.Bottom - BottomMargin;

            nfloat projectedPositionLeft;
            nfloat projectedPositionRight;
            nfloat projectedPositionTop;
            nfloat projectedPositionBottom;

            nfloat constantX = 0;
            nfloat constantY = 0;

            DialogView.ArrowSide = RSPopupPositionSideEnum;


            // Over left top corner
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                // If on right side move left
                if((convertCoordinates(position.X) + position.Width / 2) >= mainView.Frame.Width / 2)
                {
                    // first set to correct x position
                    projectedPositionLeft = CurrentDialogPosition.X + position.Width - dialogStack.Frame.Width;
                    CurrentDialogPosition.X = (nfloat)projectedPositionLeft;

                    // Fix margin
                    if ((CurrentDialogPosition.X + dialogStack.Frame.Width) > this.Frame.Right - RightMargin)
                    {
                        CurrentDialogPosition.X -= (CurrentDialogPosition.X + dialogStack.Frame.Width) - (this.Frame.Right - RightMargin);
                    }

                    if(projectedPositionLeft < minXPositionAllowed)
                    {
                        constantX = minXPositionAllowed - projectedPositionLeft;
                        CurrentDialogPosition.X += constantX;
                    }
                }
                // If on left side move right
                else if ((convertCoordinates(position.X) + position.Width / 2) < mainView.Frame.Width / 2)
                {
                    if (CurrentDialogPosition.X < (this.Frame.Left + LeftMargin))
                    {
                        CurrentDialogPosition.X = (this.Frame.Left + LeftMargin);
                    }

                    projectedPositionRight = CurrentDialogPosition.X + dialogStack.Frame.Width;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        constantX = maxXPositionAllowed - projectedPositionRight;
                        CurrentDialogPosition.X += constantX;
                    }
                }
            }

            // Left Trailing to Leading anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                projectedPositionLeft = convertCoordinates(position.X) - dialogStack.Frame.Width - arrowSize;
                projectedPositionRight = convertCoordinates(position.X) + position.Width + dialogStack.Frame.Width + arrowSize;

                // If left space not available check if right is ok if not just move it to the right
                if(projectedPositionLeft < minXPositionAllowed)
                {
                    // Check if right space enough, if ok than place it at right side
                    if(projectedPositionRight <= maxXPositionAllowed && (projectedPositionRight - dialogStack.Frame.Width - arrowSize) >= minXPositionAllowed)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Right;
                        }
 
                        constantX = convertCoordinates(position.X) + position.Width + arrowSize;
                        CurrentDialogPosition.X = constantX;
                        dialogViewLeadingConstraint.Constant = -arrowSize;
                        dialogViewTrailingConstraint.Constant = 0;
                    }
                    // Just move it so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                        }

                        constantX = minXPositionAllowed - projectedPositionLeft;
                        CurrentDialogPosition.X += constantX;
                        dialogViewTrailingConstraint.Constant = 0;
                    }
                }
                else if((projectedPositionLeft + dialogStack.Frame.Width + arrowSize) > maxXPositionAllowed)
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                    }

                    constantX = maxXPositionAllowed - dialogStack.Frame.Width;
                    CurrentDialogPosition.X = constantX;
                    dialogViewTrailingConstraint.Constant = 0;
                }
            }

            // Right Leading to Trailing anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                projectedPositionLeft = convertCoordinates(position.X) - dialogStack.Frame.Width - arrowSize;
                projectedPositionRight = CurrentDialogPosition.X + dialogStack.Frame.Width + arrowSize;

                // If right space not available check if left is ok if not just move it to the left
                if (projectedPositionRight > maxXPositionAllowed)
                {
                    // Check if left space enough, if ok than place it at left side
                    if (projectedPositionLeft >= minXPositionAllowed && ((projectedPositionLeft + dialogStack.Frame.Width + arrowSize) <= maxXPositionAllowed))
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Left;
                        }

                        constantX = convertCoordinates(position.X) - dialogStack.Frame.Width - arrowSize;
                        CurrentDialogPosition.X = constantX;
                        dialogViewTrailingConstraint.Constant = arrowSize;
                        dialogViewLeadingConstraint.Constant = 0;
                    }
                    // Just move it so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                        }

                        constantX = maxXPositionAllowed - projectedPositionRight + arrowSize;
                        CurrentDialogPosition.X += constantX;
                        dialogViewLeadingConstraint.Constant = 0;
                    }
                }
                else if((projectedPositionRight - dialogStack.Frame.Width - arrowSize) < minXPositionAllowed)
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                    }

                    constantX = minXPositionAllowed - (projectedPositionRight - dialogStack.Frame.Width);
                    CurrentDialogPosition.X += constantX;
                    dialogViewLeadingConstraint.Constant = 0;
                }
            }

            // Bottom and Top CenterAnchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                //// X position
                // If on right side move left if needed else keep position
                if ((convertCoordinates(position.X) + position.Width / 2) >= mainView.Frame.Width / 2)
                {
                    projectedPositionRight = convertCoordinates(position.X) + position.Width / 2 + dialogStack.Frame.Width / 2;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        constantX = maxXPositionAllowed - projectedPositionRight;
                        CurrentDialogPosition.X = CurrentDialogPosition.X + constantX;
                    }
                }
                // If on left side move right if needed else keep position
                else if ((convertCoordinates(position.X) + position.Width / 2) < mainView.Frame.Width / 2)
                {
                    projectedPositionLeft = convertCoordinates(position.X) + position.Width / 2 - dialogStack.Frame.Width / 2;

                    if (projectedPositionLeft < minXPositionAllowed)
                    {
                        constantX = minXPositionAllowed - projectedPositionLeft;
                        CurrentDialogPosition.X = CurrentDialogPosition.X + constantX;
                    }
                }


                // Hide arrow if not withit dialogstack bounds
                if ((System.Math.Abs(constantX) + arrowSize) >= (dialogStack.Frame.Width / 2))
                {
                    (DialogView as RSDialogView).HideArrow = true;
                }
            }



            // Y Position for Bottom
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                projectedPositionTop = CurrentDialogPosition.Y - position.Height - dialogStack.Frame.Height - arrowSize;
                projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height + arrowSize;

                // If bottom space not available check if top is ok if not just move it to the top
                if (projectedPositionBottom > maxYPositionAllowed)
                {
                    // Check if top space enough, if ok than place it at top side
                    if (projectedPositionTop >= minYPositionAllowed && (projectedPositionTop + dialogStack.Frame.Height + arrowSize) <= (this.Frame.Bottom))
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Top;
                        }


                        constantY = convertCoordinates(position.Y) - dialogStack.Frame.Height - arrowSize;
                        CurrentDialogPosition.Y = constantY;
                        dialogViewBottomConstraint.Constant = arrowSize;
                        dialogViewTopConstraint.Constant = 0;
                    }
                    // Just move it to the top so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                        }

                        //constantY = maxYPositionAllowed - projectedPositionBottom + arrowSize;
                        constantY = projectedPositionBottom - maxYPositionAllowed - arrowSize;
                        CurrentDialogPosition.Y -= constantY;
                        dialogViewTopConstraint.Constant = 0;
                    }
                }
                else if((projectedPositionBottom - dialogStack.Frame.Height - arrowSize) < minYPositionAllowed)
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                    }

                    constantY = minYPositionAllowed - (projectedPositionBottom - dialogStack.Frame.Height - arrowSize);
                    CurrentDialogPosition.Y += constantY;
                    dialogViewTopConstraint.Constant = 0;
                }
            }

            // Y Position for Top
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                projectedPositionTop = convertCoordinates(position.Y) - dialogStack.Frame.Height - arrowSize;
                projectedPositionBottom = convertCoordinates(position.Y) + position.Height + dialogStack.Frame.Height + arrowSize;

                // If top space not available check if bottom is ok if not just move it to the bottom
                if (projectedPositionTop < minYPositionAllowed)
                {
                    // Check if bottom space enough, if ok than place it at bottom side
                    if (projectedPositionBottom <= maxYPositionAllowed && (projectedPositionBottom - dialogStack.Frame.Height - arrowSize) >= minYPositionAllowed)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Bottom;
                        }

                        constantY = convertCoordinates(position.Y) + position.Height + arrowSize;
                        CurrentDialogPosition.Y = constantY;
                        dialogViewTopConstraint.Constant = -arrowSize;
                        dialogViewBottomConstraint.Constant = 0;
                    }
                    // Just move it to the bottom so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                        }

                        //constantY = dialogStack.Frame.Height + maxYPositionAllowed - (projectedPositionBottom - position.Height) + arrowSize;
                        constantY = minYPositionAllowed - projectedPositionTop;
                        CurrentDialogPosition.Y += constantY;
                        dialogViewBottomConstraint.Constant = 0;
                    }
                }
                else if((CurrentDialogPosition.Y + dialogStack.Frame.Height) > (this.Frame.Bottom ))
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                    }

                    constantY = (CurrentDialogPosition.Y + dialogStack.Frame.Height) - (this.Frame.Bottom);
                    CurrentDialogPosition.Y = CurrentDialogPosition.Y - constantY;
                    dialogViewBottomConstraint.Constant = 0;
                }
            }

            // Y Position for Left Right
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left ||
            RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                // If on bottom side move to top if needed else keep position
                if ((Math.Abs(position.Y) + position.Height / 2) >= mainView.Frame.Height / 2)
                {
                    projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height;

                    if (projectedPositionBottom > maxYPositionAllowed)
                    {
                        constantY = maxYPositionAllowed - projectedPositionBottom;
                        CurrentDialogPosition.Y += constantY;
                    }
                }
                // If on top side move to bottom if needed else keep position
                else if ((Math.Abs(position.Y) + position.Height / 2) < mainView.Frame.Height / 2)
                {
                    projectedPositionTop = CurrentDialogPosition.Y;

                    if (projectedPositionTop < minYPositionAllowed)
                    {
                        constantY = minYPositionAllowed - projectedPositionTop;
                        CurrentDialogPosition.Y += constantY;
                    }
                    else if ((CurrentDialogPosition.Y + dialogStack.Frame.Height / 2) > (this.Frame.Bottom))
                    {
                        constantY = (this.Frame.Bottom) - (CurrentDialogPosition.Y + dialogStack.Frame.Height / 2);
                        CurrentDialogPosition.Y += constantY;
                    }
                }

                // Hide arrow if not withit dialogstack bounds
                if ((System.Math.Abs(constantY) + arrowSize) >= (dialogStack.Frame.Height / 2))
                {
                    (DialogView as RSDialogView).HideArrow = true;
                }
            }

            // Y Position for Over
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                if (CurrentDialogPosition.Y < TopMargin)
                    CurrentDialogPosition.Y = TopMargin;

                projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height;

                if (projectedPositionBottom > maxYPositionAllowed)
                {
                    var constant = projectedPositionBottom - maxYPositionAllowed;
                    CurrentDialogPosition.Y -= (nfloat)constant;
                }
            }


            dialogPositionXConstraint.Constant = CurrentDialogPosition.X;
            dialogPositionYConstraint.Constant = CurrentDialogPosition.Y;


            // Set xConstrainConstant and yConstrainConstant, it's used to position the arrow at the correct place in draw method
            // We call Draw method manually because when dialog frame size doens't change it's not called
            // The condition here is so that it not calls draw unnecesarely
            if (DialogView.xConstrainConstant != dialogPositionXConstraint.Constant || DialogView.yConstrainConstant != dialogPositionYConstraint.Constant)
            {
                DialogView.xConstrainConstant = constantX;
                DialogView.yConstrainConstant = constantY;
                DialogView.Draw(DialogView.Bounds);
            }
        }

        // Create and open popup
        public void ShowPopup()
        {
            SetupBackgroundView();
            SetupDialogView();
            SetupDialogStack();
            SetTitle(Title, 18);
            SetMessage(Message, 12);
            setContentStack();
            setContentScrollView();
            if (CustomView != null)
                SetCustomView();

            AddButtonDivider();

            KeyboardPosition = 0;

            // Add bottom margin to last element if there are no buttons so it equals the other sides
            if(topBorderButtonsStack.Hidden)
                dialogStack.SetCustomSpacing(15, dialogStack.ArrangedSubviews[1]);


            UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
            {
                Dismiss(true);
                Dispose();
            });
            BackgroundView.AddGestureRecognizer(didTappedOnBackgroundView);

            bool animated = true;


            // Animation
            if (animated)
            {

                //RSPopupAnimation(DialogView, RSPopupAnimationEnum.Expanding, true);
            }
            else
            {
                this.BackgroundView.Alpha = 1f;
                this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height - this.DialogView.Frame.Height / 2);
            }
        }

        // Call animation block here, this method is called once all layout has finished and is ready to draw
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            // Animate 
            RSPopupAnimation(DialogView, RSPopupAnimationEnum, true);
        }
        public void RSPopupAnimation(UIView view, RSPopupAnimationEnum animationType, bool isShowing)
        {
            nfloat duration = 0.33f;

            if(animationType == RSPopupAnimationEnum.CurveEaseInOut)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f);

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
                    }, null);
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeScale(1f, 1.1f);

                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }

            if (animationType == RSPopupAnimationEnum.Expanding)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeScale(0.2f, 0.2f);

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
                    }, null
                    );
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeScale(0.2f, 0.2f);

                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }

            if (animationType == RSPopupAnimationEnum.BottomToTop)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, dialogStack.Frame.Height + (mainView.Frame.Height - DialogView.Frame.Bottom));

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, 0);
                    }, null
                    );
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, dialogStack.Frame.Height + (mainView.Frame.Height - DialogView.Frame.Bottom) + DialogView.Transform.Ty);
                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }

            if (animationType == RSPopupAnimationEnum.TopToBottom)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, -dialogStack.Frame.Height - DialogView.Frame.Top);

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, 0);
                    }, null
                    );
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, -dialogStack.Frame.Height - DialogView.Frame.Top);
                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }

            if (animationType == RSPopupAnimationEnum.LeftToRight)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeTranslation(-dialogStack.Frame.Width - DialogView.Frame.Left, 0);

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, 0f);
                    }, null
                    );
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeTranslation(-dialogStack.Frame.Width - DialogView.Frame.Left, 0f);
                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }

            if (animationType == RSPopupAnimationEnum.RightToLeft)
            {
                if (isShowing)
                {
                    this.BackgroundView.Alpha = 0;
                    this.DialogView.Transform = CGAffineTransform.MakeTranslation(dialogStack.Frame.Width + (mainView.Frame.Width - DialogView.Frame.Right), 0);

                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
                    {
                        this.BackgroundView.Alpha = 1f;
                        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0f, 0f);
                    }, null
                    );
                }
                else
                {
                    UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                                   () =>
                                   {
                                       this.BackgroundView.Alpha = 0f;
                                       this.DialogView.Transform = CGAffineTransform.MakeTranslation(dialogStack.Frame.Width + (mainView.Frame.Width - DialogView.Frame.Right), 0f);
                                   },
                                   () => { this.RemoveFromSuperview(); IsAnimating = false; });
                }
            }
        }

        // Used to position popup at the right place
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // layout pending layout updates
            //dialogStack.LayoutIfNeeded();

            UpdateDialogPosition();

            (dialogStack as TestClass).dh = dialogStack.Frame.Height;
            (dialogStack as TestClass).dw = dialogStack.Frame.Width;
        }

        public void UpdateDialogPosition()
        {
            if (CustomView != null)
            {
                layoutCustomView();
                if ((renderer.Element as ContentPage).Content is Layout)
                    ((renderer.Element as ContentPage).Content as Layout).ForceLayout();
            }

            if (RelativeView != null)
            {
                var position = relativeViewAsNativeView.ConvertRectFromView(relativeViewAsNativeView.Bounds, this.mainView);

                dialogViewBottomConstraint.Constant = 0;
                dialogViewTopConstraint.Constant = 0;
                dialogViewLeadingConstraint.Constant = 0;
                dialogViewTrailingConstraint.Constant = 0;
                (DialogView as RSDialogView).HideArrow = false;

                shouldShowArrow();
                setConstraintPosition(position);
                updatePosition(position);
            }
        }

        // Button graphics
        private void AddBorder(UIButton button)
        {
            UIView border = new UIView() { BackgroundColor = UIColor.LightGray, TranslatesAutoresizingMaskIntoConstraints = false };
            button.AddSubview(border);
            border.HeightAnchor.ConstraintEqualTo(button.HeightAnchor).Active = true;
            border.WidthAnchor.ConstraintEqualTo(0.5f).Active = true;
            border.LeadingAnchor.ConstraintEqualTo(button.TrailingAnchor).Active = true;
        }
        public enum UIButtonBorderSide
        {
            Top, Bottom, Left, Right
        }

        public void Close()
        {
           Dismiss(true);
        }

        //Dismiss event
        public delegate void DismissEventHandler(object source, EventArgs args);
        public event EventHandler DismissEvent;
        protected virtual void OnDismissed()
        {
            if (DismissEvent != null)
                DismissEvent(this, EventArgs.Empty);

            // Invoke dismiss event for xamarin rspopup object
            if (rSPopup != null)
                rSPopup.OnDismissed();
        }

        // Dismiss Animation part
        public void Dismiss(bool animated)
        {
            if (CustomView != null)
            {
                RemoveKeyboardObservers();
                renderer.Element.MeasureInvalidated -= CustomViewContentPage_MeasureInvalidated;
                //renderer.NativeView.RemoveFromSuperview();
                //CustomView.Parent = null;
            }

            if (animated)
            {
                IsAnimating = true;
                RSPopupAnimation(DialogView, RSPopupAnimationEnum, false);
            }
            else
            {
                this.RemoveFromSuperview();
            }

            OnDismissed();
            this.Dispose();
        }

        // Dispose
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


    public class RSDialogView : UIView
    {
        nfloat BorderRadius;
        CAShapeLayer shape;
        RSPopupRenderer rSPopupRenderer;
        public bool HideArrow { get; set; }
        nfloat arrowSpace;
        public RSPopupPositionSideEnum ArrowSide;
        public nfloat xConstrainConstant;
        public nfloat yConstrainConstant;


        public void SetRSPopupRenderer(RSPopupRenderer rSPopupRenderer)
        {
            this.rSPopupRenderer = rSPopupRenderer;
        }

        public RSDialogView(nfloat BorderRadius, Forms.Color BorderFillColor) : base()
        {
            this.BorderRadius = BorderRadius;
            this.ContentMode = UIViewContentMode.Redraw; // force redraw when frame changes
            arrowSpace = 10;

            // Set layer shape
            shape = new CAShapeLayer();
            shape.CornerRadius = BorderRadius;
            shape.BorderWidth = 1;
            shape.ZPosition = -1;
            shape.BorderColor = BorderFillColor.ToCGColor();
            shape.ShadowColor = UIColor.Gray.CGColor;
            shape.ShadowOpacity = 0.5f;
            shape.ShadowRadius = 10;
            shape.BackgroundColor = BorderFillColor.ToCGColor();
            shape.ShadowOffset = new CGSize(0f, 5f);
            shape.FillColor = BorderFillColor.ToCGColor();
            Layer.AddSublayer(shape);
        }

        public void SetBorderFillColor(Color color)
        {
            shape.BorderColor = color.ToCGColor();
            shape.BackgroundColor = color.ToCGColor();
            shape.FillColor = color.ToCGColor();
        }

        public override void Draw(CGRect rect)
        {
            // We don't need to redraw when animating
            if (rSPopupRenderer.IsAnimating)
                return;


            base.Draw(rect);

            UIBezierPath bezierPath;

            if (ArrowSide == RSPopupPositionSideEnum.Right && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(arrowSpace, 0, Frame.Width - arrowSpace, Frame.Height), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posY = (Frame.Height / 2) - yConstrainConstant;

                if (!HideArrow)
                {
                    bezierPath.MoveTo(new CGPoint(x: 0, y: posY));
                    bezierPath.AddLineTo(new CGPoint(x: arrowSpace, y: posY - arrowSpace));
                    bezierPath.AddLineTo(new CGPoint(x: arrowSpace, y: posY + arrowSpace));
                }
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Left && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width - arrowSpace, Frame.Height), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posY = (Frame.Height / 2) - yConstrainConstant;

                if (!HideArrow)
                {
                    bezierPath.MoveTo(new CGPoint(x: Frame.Width - arrowSpace, y: posY - arrowSpace));
                    bezierPath.AddLineTo(new CGPoint(x: Frame.Width, y: posY));
                    bezierPath.AddLineTo(new CGPoint(x: Frame.Width - arrowSpace, y: posY + arrowSpace));
                }
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Bottom && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, arrowSpace, Frame.Width, Frame.Height - arrowSpace), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posX = (Frame.Width / 2) - xConstrainConstant;

                if (!HideArrow)
                {
                    bezierPath.MoveTo(new CGPoint(x: posX - arrowSpace, y: arrowSpace));
                    bezierPath.AddLineTo(new CGPoint(x: posX, y: 0));
                    bezierPath.AddLineTo(new CGPoint(x: posX + arrowSpace, y: arrowSpace));
                }
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Top && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width, Frame.Height - arrowSpace), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posX = (Frame.Width / 2) - xConstrainConstant;

                if (!HideArrow)
                {
                    bezierPath.MoveTo(new CGPoint(x: posX - arrowSpace, y: Frame.Height - arrowSpace));
                    bezierPath.AddLineTo(new CGPoint(x: posX, y: Frame.Height));
                    bezierPath.AddLineTo(new CGPoint(x: posX + arrowSpace, y: Frame.Height - arrowSpace));
                }
            }
            else
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width, Frame.Height), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));

            // Close path
            bezierPath.ClosePath();

            // Set the path
            shape.Path = bezierPath.CGPath;
        }
    }


    /// <summary>
    /// Used only to set IntrinsicContentSize so it can be properly layed out in the uistackview
    /// </summary>
    public class RSUIScrollView : UIScrollView
    {
        public override CGSize IntrinsicContentSize
        {
            get
            {
                return new CGSize(this.Subviews[0].Frame.Width, this.Subviews[0].Frame.Height);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            InvalidateIntrinsicContentSize();
        }

        //public override CGPoint ContentOffset
        //{
        //    get => base.ContentOffset;
        //    set
        //    {
        //        base.ContentOffset = value;
        //        //Console.WriteLine(value.Y);
        //        if (PanGestureRecognizer != null)
        //        {
        //            if (ContentOffset.Y <= 0)
        //                Console.WriteLine(this.PanGestureRecognizer.LocationInView(this).Y);
        //        }
        //    }
        //}
    }


    // Used to ForceLayout on Xamarin custom view if present
    public class RScontentStack : UIStackView
    {
        public ContentPage CustomViewContentPage { get; set; }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if(CustomViewContentPage != null)
            {
                if ((CustomViewContentPage as ContentPage).Content is Layout)
                    ((CustomViewContentPage as ContentPage).Content as Layout).ForceLayout();
            }
        }
    }


    public class TestClass : UIStackView
    {
        public nfloat dh;
        public nfloat dw;
        public RSPopupRenderer rSPopupRenderer;


        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            //Console.WriteLine("base    " + dh);
            //Console.WriteLine("subclass " + Frame);


            if (dh != Frame.Height || dw != Frame.Width)
                rSPopupRenderer.UpdateDialogPosition();
        }
    }
}

