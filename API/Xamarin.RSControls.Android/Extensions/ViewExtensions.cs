using Android.AccessibilityServices;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.RSControls.Droid.Extensions
{
    public static class ViewExtensions
    {
        public static Bitmap CreateBitmapFromView(global::Android.Views.View view, int width, int height)
        {
            if (width > 0 && height > 0)
            {
                view.Measure(global::Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly), global::Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
            }

            view.Layout(0, 0, view.MeasuredWidth, view.MeasuredHeight);

            Bitmap bitmap = Bitmap.CreateBitmap(view.MeasuredWidth, view.MeasuredHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            Drawable background = view.Background;

            if (background != null)
            {
                background.Draw(canvas);
            }
            view.Draw(canvas);

            return bitmap;
        }

        public static Bitmap ConvertViewToBitmap(global::Android.Views.View v)
        {
            v.SetLayerType(LayerType.Hardware, null);

            v.Measure(global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
                global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            v.Layout(0, 0, v.MeasuredWidth, v.MeasuredHeight);

            Bitmap b = Bitmap.CreateBitmap(v.MeasuredWidth, v.MeasuredHeight, Bitmap.Config.Argb8888);
            return b;
        }

        public static global::Android.Views.View ConvertFormsToNative(Xamarin.Forms.View view, Xamarin.Forms.Rectangle size, Context context)
        {
            var vRenderer = Platform.CreateRendererWithContext(view, context);
            Platform.SetRenderer(view, vRenderer);

            //Get required size of view
            var sizeRequest = view.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);

            //Check if already manually set else give required size
            int width = 0;
            if (view.WidthRequest != -1)
                width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)view.WidthRequest, context.Resources.DisplayMetrics);
            else
                width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)sizeRequest.Request.Width, context.Resources.DisplayMetrics);

            int height = 0;
            if (view.HeightRequest != -1)
                height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)view.HeightRequest, context.Resources.DisplayMetrics);
            else
                height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)sizeRequest.Request.Height, context.Resources.DisplayMetrics);


            var nativeView = vRenderer.View;
            vRenderer.Tracker.UpdateLayout();
            var layoutParams = new ViewGroup.LayoutParams((int)width, (int)height);
            nativeView.LayoutParameters = layoutParams;
            view.Width.ToString();
            view.Layout(new Rectangle(0,0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            nativeView.Layout(0, 0, (int)width, (int)height);

            return nativeView;
        }

        public static global::Android.Views.View ConvertFormsToNative(Context context, Xamarin.Forms.View view, double x, double y, double width, double height, global::Android.Views.ViewGroup parent = null)
        {
            var renderer = Platform.CreateRendererWithContext(view, context);
            Platform.SetRenderer(view, renderer);
            renderer.Tracker.UpdateLayout();
            
            Xamarin.Forms.Rectangle size = new Xamarin.Forms.Rectangle(x, y, ContextExtensions.FromPixels(context, width), ContextExtensions.FromPixels(context, height));
            var sizeRequest = view.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);


            //Width
            if (width == 0)
                size.Width = sizeRequest.Request.Width;
            else
                if (size.Width < sizeRequest.Request.Width)
                    size.Width = sizeRequest.Request.Width;

            //Height
            if(height == 0)
                size.Height = sizeRequest.Request.Height;
            else
                if (size.Height < sizeRequest.Request.Height)
                    size.Height = sizeRequest.Request.Height;


            renderer.View.LayoutParameters = new ViewGroup.LayoutParams(
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)size.Width, context.Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)size.Height, context.Resources.DisplayMetrics));

            renderer.Element.Layout(size);
            renderer.View.Layout(0, 0,
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)size.Width, context.Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)size.Height, context.Resources.DisplayMetrics));


            var nativeView = renderer.View;

            return nativeView;
        }


        public static int IntToDip (int value, global::Android.Content.Context context)
        {
           return (int)global::Android.Util.TypedValue.ApplyDimension(global::Android.Util.ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
    }


    public class ViewCellContainer : ViewGroup
    {
        public Xamarin.Forms.View _formsView;
        private Forms.StackLayout stack;
        private IVisualElementRenderer _renderer;
        private double minCellHeight;

        public ViewCellContainer(global::Android.Content.Context context, Xamarin.Forms.View formsView, IVisualElementRenderer renderer, double minCellHeight, ViewGroup contentView) : base(context)
        {
            stack = new StackLayout();
            stack.Children.Add(formsView);
            stack.BackgroundColor = Forms.Color.Blue;
            _formsView = formsView;
            _renderer = Platform.CreateRendererWithContext(stack, Context);
            Platform.SetRenderer(stack, _renderer);
             this.AddView(_renderer.View);
            this.minCellHeight = ContextExtensions.FromPixels(this.Context, minCellHeight);
            this.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            _renderer.View.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            contentView.AddView(this);
        }

        // this will layout the cell in xamarin forms and then get the height
        // it means you can variable height cells / wrap to content etc
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);


            double pixelsWidth = MeasureSpec.GetSize(widthMeasureSpec);
            double numWidth = ContextExtensions.FromPixels(this.Context, pixelsWidth);
            Forms.SizeRequest sizeRequest = _formsView.Measure(numWidth, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);


            double layoutWidth = 0;
            //if (numWidth != 0 && (this.Parent.Parent as global::Android.Views.View).LayoutParameters.Width == -1)
            //    layoutWidth = numWidth;
            //else
                layoutWidth = sizeRequest.Request.Width;


            //if (this.minCellHeight < sizeRequest.Request.Height)
            //    _formsView.Layout(new Forms.Rectangle(_formsView.X, _formsView.Y, layoutWidth, sizeRequest.Request.Height ));
            //else
            //    _formsView.Layout(new Forms.Rectangle(_formsView.X, _formsView.Y, layoutWidth, this.minCellHeight));

            stack.Layout(new Forms.Rectangle(0.0, 0.0, numWidth, sizeRequest.Request.Height));

            double width = stack.Width;
            double parentWidth = ContextExtensions.FromPixels(this.Context, (this.Parent.Parent as global::Android.Views.View).MeasuredWidth);

            int measuredWidth = 0;
            measuredWidth = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, width), MeasureSpecMode.Exactly);

            //if (width >= parentWidth)
            //{
            //    measuredWidth = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, width), MeasureSpecMode.Exactly);
            //    //stack.Layout(new Forms.Rectangle(0.0, 0.0, width, sizeRequest.Request.Height));
            //}
            //else
            //{
            //    measuredWidth = MeasureSpec.MakeMeasureSpec((this.Parent.Parent as global::Android.Views.View).MeasuredWidth, MeasureSpecMode.Exactly);
            //    //stack.Layout(new Forms.Rectangle(0.0, 0.0, parentWidth, sizeRequest.Request.Height));
            //}



            double height = _formsView.Height;
            int measuredHeight = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, height), MeasureSpecMode.Exactly);
            //_renderer.View.Measure(widthMeasureSpec, heightMeasureSpec);


            this.SetMeasuredDimension(measuredWidth, measuredHeight);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            _renderer.UpdateLayout();
        }
    }
}