﻿using System;
using System.Windows.Input;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPopupRenderer : UIView, IDialogPopup
    {
        private UIView mainView { get; set; }
        private UIView BackgroundView { get; set; }
        private RSDialogView DialogView { get; set; }
        private TestClass dialogStack { get; set; }
        private UIStackView contentStack { get; set; }
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
        private UIView convertView;
        private NSLayoutConstraint customViewWidthConstraint;
        private NSLayoutConstraint customViewHeightConstraint;
        private ContentPage customViewContentPage;
        private nfloat KeyboardPosition;

        //Constructor
        public RSPopupRenderer() : base()
        {
            buttonsCount = 0;
            BorderRadius = 12;
            ShadowEnabled = true;
            HasArrow = false;

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
            //this.LeadingAnchor.ConstraintEqualTo(mainView.LeadingAnchor).Active = true;
            //this.TrailingAnchor.ConstraintEqualTo(mainView.TrailingAnchor).Active = true;
            //this.TopAnchor.ConstraintEqualTo(mainView.TopAnchor).Active = true;
            //this.BottomAnchor.ConstraintEqualTo(mainView.BottomAnchor).Active = true;


            this.LeadingAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.LeadingAnchor).Active = true;
            this.TrailingAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
            this.TopAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.TopAnchor).Active = true;
            thisBottomConstraint = this.BottomAnchor.ConstraintEqualTo(mainView.SafeAreaLayoutGuide.BottomAnchor);
            thisBottomConstraint.Priority = 999;
            thisBottomConstraint.Active = true;


            //BackgroundView
            BackgroundView = new UIView();
            AddSubview(BackgroundView);

            //DialogVIew
            DialogView = new RSDialogView(BorderRadius, Color.White);
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
            contentStack = new UIStackView();
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
            dialogStack.Distribution = UIStackViewDistribution.Fill;
            dialogStack.Spacing = 5;
            dialogStack.LayoutMarginsRelativeArrangement = true;
            dialogStack.InsetsLayoutMarginsFromSafeArea = false;

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
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right && HasArrow)
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 20, 0, 10);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left && HasArrow)
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 20);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top && HasArrow)
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 10);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom && HasArrow)
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(20, 10, 0, 10);
            else
                dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
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
            customViewContentPage = new ContentPage();
            customViewContentPage.Content = CustomView;
            var renderer = Platform.CreateRenderer(customViewContentPage);
            Platform.SetRenderer(customViewContentPage, renderer);

            // Convert view
            convertView = new Extensions.FormsToNativeInPopup(customViewContentPage.Content, renderer.NativeView, dialogStack, mainView);
            contentStack.AddArrangedSubview(convertView);



            //// Give size to convertedView so it can be layed out correctly in the uistackview
            //// The prioity here is lower so if parent which is a uistackview is smaller in width, he can fullfill his constraints
            //// Widht
            //renderer.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;
            //customViewWidthConstraint = renderer.NativeView.WidthAnchor.ConstraintEqualTo(0);
            //customViewWidthConstraint.Priority = 999;
            //customViewWidthConstraint.Active = true;
            //// Height
            //customViewHeightConstraint = renderer.NativeView.HeightAnchor.ConstraintEqualTo(0);
            //customViewHeightConstraint.Priority = 999;
            //customViewHeightConstraint.Active = true;


            //// Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            //var offsetX = dialogStack.DirectionalLayoutMargins.Leading + dialogStack.DirectionalLayoutMargins.Bottom;
            //var offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;


            //// Set max size
            //var maxSize = new CGSize(mainView.Frame.Width - mainView.SafeAreaInsets.Left - mainView.SafeAreaInsets.Right,
            //                     mainView.Frame.Height - mainView.SafeAreaInsets.Top - mainView.SafeAreaInsets.Bottom);

            //// Get required size for forms view
            //var sizeRequest = customViewContentPage.Content.Measure(maxSize.Width - offsetX, maxSize.Height - offsetY, Forms.MeasureFlags.IncludeMargins);

            //// Layout forms view
            //customViewContentPage.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            //customViewContentPage.Content.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));

            //contentStack.AddArrangedSubview(renderer.NativeView);


            //set keyboard KeyboardObservers
            AddKeyboardObservers();
        }

        private void layoutCustomView()
        {
            // Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            var offsetX = dialogStack.DirectionalLayoutMargins.Leading + dialogStack.DirectionalLayoutMargins.Trailing;
            var offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;

            // Set max size
            var maxSize = new CGSize(mainView.Frame.Width - mainView.SafeAreaInsets.Left - mainView.SafeAreaInsets.Right,
                                 mainView.Frame.Height - mainView.SafeAreaInsets.Top - mainView.SafeAreaInsets.Bottom);

            // Get and Set required size for forms view
            var sizeRequest = customViewContentPage.Content.Measure(maxSize.Width - offsetX, maxSize.Height - offsetY, Forms.MeasureFlags.IncludeMargins);

            // Layout forms view
            customViewContentPage.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            customViewContentPage.Content.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));

            // Update FormsToNativeInPopup width and height constrains
            customViewWidthConstraint.Constant = (nfloat)sizeRequest.Request.Width;
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


        /*UIView getCurrentFirstResponder(UIView view)
        //{
        //    if (view == null)
        //        return null;

        //    if (view.IsFirstResponder)
        //        return view;

        //    UIView res = null;

        //    foreach (UIView item in view.Subviews)
        //    {
        //        var v = getCurrentFirstResponder(item);

        //        if (v != null && v.IsFirstResponder)
        //            res = v;
        //    }

        //    return res;
        //}
        //void AddListener(VisualElement element)
        //{
        //    if (!(element is Layout<View>) || (element as Layout<View>).Children.Count <= 0)
        //    {
        //        element.Focused += Element_Focused;
        //    }
        //    else
        //    {
        //        foreach (var item in (element as Layout<View>).Children)
        //        {
        //            AddListener(item);
        //        }
        //    }
        //}
        //void RemoveListener(VisualElement element)
        //{
        //    if (!(element is Layout<View>) || (element as Layout<View>).Children.Count <= 0)
        //    {
        //        element.Focused -= Element_Focused;
        //    }
        //    else
        //    {
        //        foreach (var item in (element as Layout<View>).Children)
        //        {
        //            RemoveListener(item);
        //        }
        //    }
        //}
        //private void translateDialogBack()
        //{
        //    UIView.Animate(0.33, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
        //    {
        //        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0, 0);
        //    }, null);
        //}
        //private void translateDialog()
        //{
        //    var focusedElement = getCurrentFirstResponder(contentStack);
        //    if (focusedElement == null)
        //        return;


        //    var minYPositionAllowed = this.Frame.Top + TopMargin;
        //    var position = focusedElement.ConvertRectFromView(focusedElement.Bounds, mainView);
        //    var posYTop = inverseNumber(position.Y);
        //    var posYBottom = Math.Abs(position.Y) + position.Height;


        //    if (posYBottom > mainView.Frame.Height)
        //        posYBottom -= posYBottom - mainView.Frame.Height;


        //    nfloat pos = this.DialogView.Transform.Ty;

        //    if(keyboardHeight > 0 && posYBottom > keyboardHeight)
        //        pos = keyboardHeight - (nfloat)posYBottom;
        //    else if (posYTop < minYPositionAllowed)
        //    {
        //        pos = minYPositionAllowed - (nfloat)posYTop;
        //    }


        //    UIView.Animate(0.33, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
        //    {
        //        this.DialogView.Transform = CGAffineTransform.MakeTranslation(0, pos);
        //    }, null);
        //}
        //private void Element_Focused(object sender, FocusEventArgs e)
        //{
        //    //translateDialog();
        //}
        //private nfloat inverseNumber(nfloat number)
        //{
        //    if (number > 0)
        //        number = -number;
        //    else
        //        number = (nfloat)Math.Abs(number);

        //    return number;
        //}
        //private nfloat keyboardHeight = 0;
        */

        // Keyboard show and hide Observers
        private void AddKeyboardObservers()
        {
            keyboardObserverOpen = UIKeyboard.Notifications.ObserveDidShow((handler, args) =>
            {
                var posDialogBottomY = Math.Abs(dialogStack.ConvertRectFromView(dialogStack.Bounds, this.mainView).Y) + dialogStack.Frame.Height;
                if (posDialogBottomY > args.FrameEnd.Y)
                {
                    KeyboardPosition = args.FrameEnd.Y;
                    thisBottomConstraint.Constant = -args.FrameEnd.Height + mainView.SafeAreaInsets.Bottom;
                    dialogStack.LayoutIfNeeded();
                }
            });

            keyboardObserverClose = UIKeyboard.Notifications.ObserveDidHide((handler, args) =>
            {
                KeyboardPosition = mainView.Frame.Bottom;
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
            topBorderButtonsStack.Hidden = false;

            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton.dialog = this;
                positiveButton.Command = command;
                positiveButton.Hidden = false;
                positiveButton.SetTitle(title, UIControlState.Normal);
                positiveButton.SetTitleColor(UIColor.SystemBlue, UIControlState.Normal);
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
                widthConstraint = dialogStack.WidthAnchor.ConstraintEqualTo(this.Width - (LeftMargin + RightMargin));
                //widthConstraint.Priority = 999f;
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
                heightConstraint = dialogStack.HeightAnchor.ConstraintEqualTo(this.Height);
                //heightConstraint.Priority = 999f;
                heightConstraint.Active = true;
            }
        }

        // Dialog initial Position / create constraints and update them later
        private void setDialogPosition()
        {
            if (RelativeView == null)
            {
                dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor).Active = true;
                dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
            }


            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                dialogPositionXConstraint = dialogStack.TrailingAnchor.ConstraintEqualTo(relativeViewAsNativeView.LeadingAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                dialogPositionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                dialogPositionXConstraint = dialogStack.LeadingAnchor.ConstraintEqualTo(relativeViewAsNativeView.TrailingAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterYAnchor);
                dialogPositionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(relativeViewAsNativeView.BottomAnchor);
                dialogPositionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.BottomAnchor.ConstraintEqualTo(relativeViewAsNativeView.TopAnchor);
                dialogPositionYConstraint.Active = true;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(relativeViewAsNativeView.CenterXAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.TopAnchor.ConstraintEqualTo(relativeViewAsNativeView.TopAnchor);
                dialogPositionYConstraint.Active = true;
            }
            else
            {
                dialogPositionXConstraint = dialogStack.CenterXAnchor.ConstraintEqualTo(this.CenterXAnchor);
                dialogPositionXConstraint.Active = true;
                dialogPositionYConstraint = dialogStack.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor);
                dialogPositionYConstraint.Active = true;
            }
        }

        /// <summary>
        /// Used for position calculations
        /// </summary>
        /// <param name="position"></param>
        private void setConstraintPosition(CGRect position)
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                CurrentDialogPosition.X = (nfloat)Math.Abs(position.X);
                CurrentDialogPosition.Y = (nfloat)Math.Abs(position.Y) + position.Height / 2;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                CurrentDialogPosition.X = (nfloat)Math.Abs(position.X) + position.Width;
                CurrentDialogPosition.Y = (nfloat)Math.Abs(position.Y) + position.Height / 2;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                CurrentDialogPosition.X = (nfloat)Math.Abs(position.X) + (position.Width / 2);
                CurrentDialogPosition.Y = (nfloat)Math.Abs(position.Y) + position.Height;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                CurrentDialogPosition.X = (nfloat)Math.Abs(position.X) + (position.Width / 2);
                CurrentDialogPosition.Y = (nfloat)Math.Abs(position.Y);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                CurrentDialogPosition.X = (nfloat)Math.Abs(position.X) + position.Width / 2;
                CurrentDialogPosition.Y = (nfloat)Math.Abs(position.Y);
            }
        }

        // Correct dialog position if needed
        private void updatePosition(CGRect position)
        {
            var minXPositionAllowed = this.Frame.Left + LeftMargin;
            var maxXPositionAllowed = this.Frame.Right - RightMargin;
            var minYPositionAllowed = this.Frame.Top + TopMargin;
            var maxYPositionAllowed = this.Frame.Bottom - BottomMargin;

            double projectedPositionLeft;
            double projectedPositionRight;
            double projectedPositionTop;
            double projectedPositionBottom;

            DialogView.ArrowSide = RSPopupPositionSideEnum;


            // Over centerAnchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                // If on right side move left
                if((Math.Abs(position.X) + position.Width / 2) >= mainView.Frame.Width / 2)
                {
                    projectedPositionLeft = CurrentDialogPosition.X + position.Width / 2 - dialogStack.Frame.Width;

                    if(projectedPositionLeft < minXPositionAllowed)
                    {
                        var constant = CurrentDialogPosition.X - minXPositionAllowed - dialogStack.Frame.Width / 2;
                        dialogPositionXConstraint.Constant = -(nfloat)constant;
                    }
                    else
                    {
                        dialogPositionXConstraint.Constant = (position.Width / 2 - dialogStack.Frame.Width / 2);
                    }
                }
                // If on left side move right
                else if ((Math.Abs(position.X) + position.Width / 2) < mainView.Frame.Width / 2)
                {
                    projectedPositionRight = CurrentDialogPosition.X - position.Width / 2 + dialogStack.Frame.Width;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        var constant = maxXPositionAllowed - projectedPositionRight + dialogStack.Frame.Width / 2 - position.Width / 2;
                        dialogPositionXConstraint.Constant = (nfloat)constant;
                    }
                    else
                    {
                        dialogPositionXConstraint.Constant = (- position.Width / 2 + dialogStack.Frame.Width / 2);
                    }
                }
            }

            // Left Trailing to Leading anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                projectedPositionLeft = CurrentDialogPosition.X - dialogStack.Frame.Width;
                projectedPositionRight = CurrentDialogPosition.X + position.Width + dialogStack.Frame.Width;

                // If left space not available check if right is ok if not just move it to the right
                if(projectedPositionLeft < minXPositionAllowed)
                {
                    // Check if right space enough, if ok than place it at right side
                    if(projectedPositionRight <= maxXPositionAllowed)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Right;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 20, 0, 10);
                            //dialogStack.LayoutIfNeeded();
                        }
 
                        var constant = position.Width + dialogStack.Frame.Width;
                        dialogPositionXConstraint.Constant = constant;
                    }
                    // Just move it to the right so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
                            //dialogStack.SetNeedsDisplay();
                            //dialogStack.LayoutIfNeeded();
                        }

                        var constant = minXPositionAllowed - projectedPositionLeft;
                        dialogPositionXConstraint.Constant = (nfloat)constant;
                    }
                }
                else
                    dialogPositionXConstraint.Constant = 0;
            }

            // Right Leading to Trailing anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                projectedPositionLeft = CurrentDialogPosition.X - position.Width - dialogStack.Frame.Width;
                projectedPositionRight = CurrentDialogPosition.X + dialogStack.Frame.Width;

                // If right space not available check if left is ok if not just move it to the left
                if (projectedPositionRight > maxXPositionAllowed)
                {
                    // Check if left space enough, if ok than place it at left side
                    if (projectedPositionLeft >= minXPositionAllowed)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Left;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 20);
                            //dialogStack.LayoutIfNeeded();
                        }


                        var constant = - position.Width - dialogStack.Frame.Width;
                        dialogPositionXConstraint.Constant = constant;
                    }
                    // Just move it to the left so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
                            //dialogStack.SetNeedsDisplay();
                            //dialogStack.LayoutIfNeeded();
                        }

                        var constant = maxXPositionAllowed - projectedPositionRight;
                        dialogPositionXConstraint.Constant = (nfloat)constant;
                    }
                }
                else
                    dialogPositionXConstraint.Constant = 0;
            }

            // Bottom and Top CenterAnchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                //// X position
                // If on right side move left if needed else keep position
                if ((Math.Abs(position.X) + position.Width / 2) >= mainView.Frame.Width / 2)
                {
                    projectedPositionRight = CurrentDialogPosition.X + dialogStack.Frame.Width / 2;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        var constant = projectedPositionRight - maxXPositionAllowed;
                        dialogPositionXConstraint.Constant = -(nfloat)constant;
                    }
                    else
                        dialogPositionXConstraint.Constant = 0;
                }
                // If on left side move right if needed else keep position
                else if ((Math.Abs(position.X) + position.Width / 2) < mainView.Frame.Width / 2)
                {
                    projectedPositionLeft = CurrentDialogPosition.X - dialogStack.Frame.Width / 2;

                    if (projectedPositionLeft < minXPositionAllowed)
                    {
                        var constant = minXPositionAllowed - projectedPositionLeft;
                        dialogPositionXConstraint.Constant = (nfloat)constant;
                    }
                    else
                        dialogPositionXConstraint.Constant = 0;
                }
            }


            // Y Position for Bottom
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                projectedPositionTop = CurrentDialogPosition.Y - position.Height - dialogStack.Frame.Height;
                projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height;

                // If bottom space not available check if top is ok if not just move it to the top
                if (projectedPositionBottom > maxYPositionAllowed)
                {
                    // Check if top space enough, if ok than place it at top side
                    if (projectedPositionTop >= minYPositionAllowed && (projectedPositionTop + dialogStack.Frame.Height) < KeyboardPosition)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Top;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 10);
                            //dialogStack.LayoutIfNeeded();
                        }

                        var constant = -position.Height - dialogStack.Frame.Height;
                        dialogPositionYConstraint.Constant = constant;
                    }
                    // Just move it to the top so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
                        }

                        var constant = maxYPositionAllowed - projectedPositionBottom;
                        dialogPositionYConstraint.Constant = (nfloat)constant;
                    }
                }
                else
                    dialogPositionYConstraint.Constant = 0;
            }

            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                projectedPositionTop = CurrentDialogPosition.Y - dialogStack.Frame.Height;
                projectedPositionBottom = CurrentDialogPosition.Y + position.Height + dialogStack.Frame.Height;

                // If top space not available check if bottom is ok if not just move it to the bottom
                if (projectedPositionTop < minYPositionAllowed)
                {
                    // Check if bottom space enough, if ok than place it at bottom side
                    if (projectedPositionBottom <= maxYPositionAllowed)
                    {
                        // Switch side
                        if(HasArrow)
                        {
                            DialogView.ArrowSide = RSPopupPositionSideEnum.Bottom;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(20, 10, 0, 10);
                            //dialogStack.SetNeedsDisplay();
                            //dialogStack.LayoutIfNeeded();
                        }

                        var constant = position.Height + dialogStack.Frame.Height;
                        dialogPositionYConstraint.Constant = constant;
                    }
                    // Just move it to the bottom so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
                            //dialogStack.SetNeedsDisplay();
                            //dialogStack.LayoutIfNeeded();
                        }

                        var constant = dialogStack.Frame.Height + maxYPositionAllowed - (projectedPositionBottom - position.Height);
                        dialogPositionYConstraint.Constant = (nfloat)constant;
                    }
                }
                else if(CurrentDialogPosition.Y > KeyboardPosition)
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                        dialogStack.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 0, 10);
                        //dialogStack.SetNeedsDisplay();
                        //dialogStack.LayoutIfNeeded();
                    }

                    var constant = KeyboardPosition - CurrentDialogPosition.Y;
                    dialogPositionYConstraint.Constant = (nfloat)constant;
                }
                else
                    dialogPositionYConstraint.Constant = 0;
            }


            // Y Position for Left Right
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left ||
            RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                // If on bottom side move to top if needed else keep position
                if ((Math.Abs(position.Y) + position.Height / 2) >= mainView.Frame.Height / 2)
                {
                    projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height / 2;

                    if (projectedPositionBottom > maxYPositionAllowed)
                    {
                        var constant = projectedPositionBottom - maxYPositionAllowed;
                        dialogPositionYConstraint.Constant = -(nfloat)constant;
                    }
                    else
                        dialogPositionYConstraint.Constant = 0;
                }
                // If on top side move to bottom if needed else keep position
                else if ((Math.Abs(position.Y) + position.Height / 2) < mainView.Frame.Height / 2)
                {
                    projectedPositionTop = CurrentDialogPosition.Y - dialogStack.Frame.Height / 2;

                    if (projectedPositionTop < minYPositionAllowed)
                    {
                        var constant = minYPositionAllowed - projectedPositionTop;
                        dialogPositionYConstraint.Constant = (nfloat)constant;
                    }
                    else if ((CurrentDialogPosition.Y + dialogStack.Frame.Height / 2) > KeyboardPosition)
                    {
                        var constant = KeyboardPosition - (CurrentDialogPosition.Y + dialogStack.Frame.Height / 2);
                        dialogPositionYConstraint.Constant = (nfloat)constant;
                    }
                    else
                        dialogPositionYConstraint.Constant = 0;
                }
            }

            // Y Position for Over
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                projectedPositionBottom = CurrentDialogPosition.Y + dialogStack.Frame.Height;

                if (projectedPositionBottom > maxYPositionAllowed)
                {
                    var constant = projectedPositionBottom - maxYPositionAllowed;
                    dialogPositionYConstraint.Constant = -(nfloat)constant;
                }
                else
                    dialogPositionYConstraint.Constant = 0;
            }


            // Set xConstrainConstant and yConstrainConstant, it's used to position the arrow at the correct place in draw method
            // We call Draw method manually because when dialog frame size doens't change it's not called
            // The condition here is so that it not calls draw unnecesarely
            if (DialogView.xConstrainConstant != dialogPositionXConstraint.Constant || DialogView.yConstrainConstant != dialogPositionYConstraint.Constant)
            {
                DialogView.xConstrainConstant = dialogPositionXConstraint.Constant;
                DialogView.yConstrainConstant = dialogPositionYConstraint.Constant;
                DialogView.Draw(DialogView.Bounds);
            }
        }

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

            KeyboardPosition = mainView.Frame.Bottom;


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


            if (RelativeView != null)
            {
                var position = relativeViewAsNativeView.ConvertRectFromView(relativeViewAsNativeView.Bounds, this.mainView);

                //if(CustomView != null)
                //    layoutCustomView();


                // layout pending layout updates
                dialogStack.LayoutIfNeeded();


                shouldShowArrow();
                setMargins();
                setConstraintPosition(position);
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

        //Dismiss event
        public delegate void DismissEventHandler(object source, EventArgs args);
        public event EventHandler DismissEvent;
        protected virtual void OnDismissed()
        {
            if (DismissEvent != null)
                DismissEvent(this, EventArgs.Empty);
        }

        // Dismiss Animation part
        public void Dismiss(bool animated)
        {
            if (CustomView != null)
            {
                RemoveKeyboardObservers();
            }

            if (animated)
            {
                IsAnimating = true;
                UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0f; }, () => { });
                UIView.Animate(0.33, 0,
                               UIViewAnimationOptions.CurveEaseInOut,
                               () => { this.DialogView.Transform = CGAffineTransform.MakeScale(1f, 1.1f); }, null);
                UIView.Animate(0.33,
                               () => { this.DialogView.Alpha = 0f; },
                               () => { this.RemoveFromSuperview(); IsAnimating = false; });

            }
            else
            {
                this.RemoveFromSuperview();
            }

            OnDismissed();
            this.Dispose();
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

                bezierPath.MoveTo(new CGPoint(x: arrowSpace, y: posY - arrowSpace));
                bezierPath.AddLineTo(new CGPoint(x: 0, y: posY));
                bezierPath.AddLineTo(new CGPoint(x: arrowSpace, y: posY + arrowSpace));
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Left && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width - arrowSpace, Frame.Height), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posY = (Frame.Height / 2) - yConstrainConstant;

                bezierPath.MoveTo(new CGPoint(x: Frame.Width - arrowSpace, y: posY - arrowSpace));
                bezierPath.AddLineTo(new CGPoint(x: Frame.Width, y: posY));
                bezierPath.AddLineTo(new CGPoint(x: Frame.Width - arrowSpace, y: posY + arrowSpace));
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Bottom && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, arrowSpace, Frame.Width, Frame.Height - arrowSpace), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posX = (Frame.Width / 2) - xConstrainConstant;
                bezierPath.MoveTo(new CGPoint(x: posX + arrowSpace, y: arrowSpace));
                bezierPath.AddLineTo(new CGPoint(x: posX, y: 0));
                bezierPath.AddLineTo(new CGPoint(x: posX - arrowSpace, y: arrowSpace));
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Top && rSPopupRenderer.HasArrow)
            {
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width, Frame.Height - arrowSpace), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));
                var posX = (Frame.Width / 2) - xConstrainConstant;

                bezierPath.MoveTo(new CGPoint(x: posX + arrowSpace, y: Frame.Height - arrowSpace));
                bezierPath.AddLineTo(new CGPoint(x: posX, y: Frame.Height));
                bezierPath.AddLineTo(new CGPoint(x: posX - arrowSpace, y: Frame.Height - arrowSpace));
            }
            else
                bezierPath = UIBezierPath.FromRoundedRect(new CGRect(0, 0, Frame.Width, Frame.Height), UIRectCorner.AllCorners, new CGSize(BorderRadius, BorderRadius));

            // Set the path
            shape.Path = bezierPath.CGPath;

            ////Draw the pointer bootom
            //bezierPath.MoveTo(new CGPoint(x: Frame.Width / 2 + 10, y: Frame.Height - (nfloat)10.0));
            //bezierPath.AddLineTo(new CGPoint(x: Frame.Width / 2, y: Frame.Height));
            //bezierPath.AddLineTo(new CGPoint(x: Frame.Width / 2 - (nfloat)10.0, y: Frame.Height - (nfloat)10.0));
            //shape.Path = bezierPath.CGPath;
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
    }




    public class TestClass : UIStackView
    {
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            this.Frame.ToString();
        }
    }
}

