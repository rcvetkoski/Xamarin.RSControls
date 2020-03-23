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

            renderer.NativeView.Frame = size;

            renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

            renderer.Element.Layout(size.ToRectangle());

            var nativeView = renderer.NativeView;

            nativeView.SetNeedsLayout();

            //return nativeView;


            return nativeView.Subviews[0];
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


            _formsView.Layout(new Forms.Rectangle(0, 0, 170, 150));

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
}
