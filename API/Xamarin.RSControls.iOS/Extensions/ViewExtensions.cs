using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.RSControls.iOS.Extensions
{
    public static class ViewExtensions
    {
        public static UIView ConvertFormsToNative(Xamarin.Forms.View view, CGRect size)
        {
            var renderer = Platform.CreateRenderer(view);
            Platform.SetRenderer(view, renderer);

            //Get required size of view
            var sizeRequest = view.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);

            //Check if already manually set else give required size
            int width = 0;
            if (view.WidthRequest != -1)
                width = (int)view.WidthRequest;
            else
                width = (int)sizeRequest.Request.Width;

            int height = 0;
            if (view.HeightRequest != -1)
                height = (int)view.HeightRequest;
            else
                height = (int)(float)sizeRequest.Request.Height;


            size = new CGRect(0, 0, width, height);

            
            renderer.NativeView.Frame = size;
            //renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;
            renderer.Element.Layout(size.ToRectangle());
            var nativeView = renderer.NativeView;

            nativeView.SetNeedsLayout();
            return nativeView;
        }

        public static UIView ConvertFormsToNative(Xamarin.Forms.View view, double x, double y, double width, double height)
        {
            var renderer = Platform.CreateRenderer(view);
            Platform.SetRenderer(view, renderer);

            CGRect size = new CGRect(x, y, width, height);

            if (width == 0 || height == 0)
            {
                var sizeRequest = view.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);

                if (width == 0)
                    size.Width = (nfloat)sizeRequest.Request.Width;

                if (height == 0)
                    size.Height = (nfloat)sizeRequest.Request.Height;
            }

            renderer.NativeView.Frame = size;
            //renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            //renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;
            renderer.Element.Layout(size.ToRectangle());
            var nativeView = renderer.NativeView;

            //nativeView.SetNeedsLayout();
            return nativeView;
        }

        public static UIView EdgeTo(UIView parent, UIView view, bool leading = true, bool trailing = true, bool top = true, bool bottom = true)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            view.LeadingAnchor.ConstraintEqualTo(parent.LeadingAnchor).Active = leading;
            view.TrailingAnchor.ConstraintEqualTo(parent.TrailingAnchor).Active = trailing;
            view.TopAnchor.ConstraintEqualTo(parent.TopAnchor).Active = top;
            view.BottomAnchor.ConstraintEqualTo(parent.BottomAnchor).Active = bottom;

            return view;
        }

    }

    public class CustomNativeView : UIView
    {
        private UIView natView;
        public CustomNativeView( UIView nativeView)
        {
            natView = nativeView;
            this.AddSubview(nativeView);
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                return new CGSize(natView.Frame.Width, natView.Frame.Height);
            }
        }


        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
        }
    }

    public class CustomContainer : UIViewController
    {
        public Xamarin.Forms.View _formsView;
        private IVisualElementRenderer _renderer;
        public UIView _parent;


        public CustomContainer(Xamarin.Forms.View formsView, IVisualElementRenderer renderer, UIView parent)
        {
            _formsView = formsView;
            _renderer = renderer;
            _parent = parent;
            this.View.AddSubview(_renderer.NativeView);
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var width = _parent != null ? _parent.Frame.Width : 0;
            var height = _parent != null ? _parent.Frame.Height : 0;
        }
    }

    public class FormsToiosCustomDialogView : UIView
    {
        public Xamarin.Forms.View _formsView;
        private IVisualElementRenderer _renderer;
        public UIView _parent;
        private CGSize contentSize;
        private NSObject deviceRotationObserver;

        public FormsToiosCustomDialogView(Xamarin.Forms.View formsView, IVisualElementRenderer renderer, UIView uIView)
        {
            _formsView = formsView;
            _renderer = renderer;
            this.AddSubview(_renderer.NativeView);
            this.ClipsToBounds = true;
            _parent = uIView;

            //Set event for device orientation change so we can reset border frame and mask
            deviceRotationObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);

            _formsView.MeasureInvalidated += _formsView_MeasureInvalidated;


            MeasureAllowedSize();
        }

        private void MeasureAllowedSize()
        {
            double width = 0;
            double height = 0;


            Forms.SizeRequest sizeRequest = _formsView.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);


            //Width
            if (sizeRequest.Request.Width < 270)
                width = 270;
            else if (sizeRequest.Request.Width > UIScreen.MainScreen.Bounds.Width * 0.9f)
                width = UIScreen.MainScreen.Bounds.Width * 0.9f;
            else
                width = sizeRequest.Request.Width;


            //Height
            if (sizeRequest.Request.Height > _parent.Frame.Height)
                height = _parent.Frame.Height;
            else
                height = sizeRequest.Request.Height;



            contentSize = new CGSize(width, height);
        }

        private void _formsView_MeasureInvalidated(object sender, EventArgs e)
        {
            this.InvalidateIntrinsicContentSize();
        }

        public override void InvalidateIntrinsicContentSize()
        {
            base.InvalidateIntrinsicContentSize();


            _renderer.NativeView.Frame = new CGRect(0, 0, contentSize.Width, contentSize.Height);
            _renderer.Element.Layout(new Forms.Rectangle(0, 0, contentSize.Width, contentSize.Height));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            this.InvalidateIntrinsicContentSize();
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                MeasureAllowedSize();

                return new CGSize(contentSize.Width, contentSize.Height);
            }
        }

        //Device orientation change event
        private void DeviceRotated(Foundation.NSNotification notification)
        {
            MeasureAllowedSize();

            this.InvalidateIntrinsicContentSize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //Remove observer
            NSNotificationCenter.DefaultCenter.RemoveObserver(deviceRotationObserver);

            if(_formsView != null)
                _formsView.MeasureInvalidated -= _formsView_MeasureInvalidated;
        }
    }






    public class FormsToNativeInPopup : UIView
    {
        Forms.View formsView;
        UIStackView dialogStack;
        UIView rSPopupRenderer;
        Forms.SizeRequest sizeRequest;
        nfloat offsetX;
        nfloat offsetY;
        NSLayoutConstraint widthConstraint;
        NSLayoutConstraint heightConstraint;
        UIView nativeView;

        public FormsToNativeInPopup(Forms.View formsView, UIView nativeView, UIStackView dialogStack, UIView rSPopupRenderer)
        {
            this.nativeView = nativeView;
            this.formsView = formsView;
            this.dialogStack = dialogStack;
            this.rSPopupRenderer = rSPopupRenderer;
            var lol = nativeView.Superview;
            this.AddSubview(nativeView);
            

            // Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            offsetX = rSPopupRenderer.DirectionalLayoutMargins.Leading + rSPopupRenderer.DirectionalLayoutMargins.Bottom;
            offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;

            // Get required size for forms view
            sizeRequest = formsView.Measure(rSPopupRenderer.Frame.Width - offsetX, rSPopupRenderer.Frame.Height - offsetY, Forms.MeasureFlags.IncludeMargins);


            // Set nativeView constraints
            nativeView.TranslatesAutoresizingMaskIntoConstraints = false;
            nativeView.WidthAnchor.ConstraintEqualTo(this.WidthAnchor).Active = true;
            nativeView.HeightAnchor.ConstraintEqualTo(this.HeightAnchor).Active = true;
            nativeView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor).Active = true;
            nativeView.TopAnchor.ConstraintEqualTo(this.TopAnchor).Active = true;


            // Give size to convertedView so it can be layed out correctly in the uistackview
            // The prioity here is lower so if parent which is a uistackview is smaller in width, he can fullfill his constraints
            // Widht
            this.TranslatesAutoresizingMaskIntoConstraints = false;
            widthConstraint = this.WidthAnchor.ConstraintEqualTo(0);
            widthConstraint.Priority = 999;
            widthConstraint.Active = true;
            // Height
            heightConstraint = this.HeightAnchor.ConstraintEqualTo(0);
            heightConstraint.Priority = 999;
            heightConstraint.Active = true;


            // Layout forms view
            (formsView.Parent as Forms.ContentPage).Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            formsView.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Take into account dialogStack's DirectionalLayoutMargins when calculating custom view sizeRequest
            offsetX = dialogStack.DirectionalLayoutMargins.Leading + dialogStack.DirectionalLayoutMargins.Trailing;
            offsetY = dialogStack.DirectionalLayoutMargins.Top + dialogStack.DirectionalLayoutMargins.Bottom;

            // Get and Set required size for forms view
            sizeRequest = formsView.Measure(rSPopupRenderer.Frame.Width - offsetX, rSPopupRenderer.Frame.Height - offsetY, Forms.MeasureFlags.IncludeMargins);

            // Layout forms view
            (formsView.Parent as Forms.ContentPage).Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            formsView.Layout(new Forms.Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));

            // Update FormsToNativeInPopup width and height constrains
            widthConstraint.Constant = (nfloat)sizeRequest.Request.Width;
            heightConstraint.Constant = (nfloat)sizeRequest.Request.Height;
        }

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            var v = this.nativeView.Subviews[0];
            var lll = v.Focused;
            v.BecomeFirstResponder();
            var lol = nativeView.PointInside(point, uievent);
            return base.PointInside(point, uievent);
        }
    }
}
