using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.RSControls.Droid.Extensions
{
    public static class ViewExtensions
    {
        public static Bitmap CreateBitmapFromView(global::Android.Views.View view, int width, int height)
        {
            if (width > 0 && height > 0)
            {
                view.Measure(View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly), View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
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
            //var vRenderer = Platform.CreateRendererWithContext(view, context);
            //var androidView = vRenderer.View;
            //vRenderer.Tracker.UpdateLayout();


            //var widthTotal = 0;
            //var spacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 6, context.Resources.DisplayMetrics);
            //var heightTotal = 0;

            //if (androidView is ViewGroup)
            //{
            //    for (int i = 0; i < (androidView as ViewGroup).ChildCount; i++)
            //    {
            //        var v = (androidView as ViewGroup).GetChildAt(i);

            //        v.MeasuredHeight.ToString();
            //        v.Measure(View.MeasureSpec.MakeMeasureSpec((int)size.Width, MeasureSpecMode.AtMost),
            //                  View.MeasureSpec.MakeMeasureSpec((int)size.Height, MeasureSpecMode.AtMost));

            //        //(androidView as ViewGroup).Measure(View.MeasureSpec.MakeMeasureSpec((int)size.Width, MeasureSpecMode.AtMost),
            //        //                                   View.MeasureSpec.MakeMeasureSpec((int)size.Height, MeasureSpecMode.AtMost));


            //        widthTotal += v.MeasuredWidth;
            //        heightTotal = v.MeasuredHeight;
            //    }
            //}
            //else
            //{
            //    androidView.Measure((int)size.Width, (int)size.Height);
            //    widthTotal = androidView.MeasuredWidth;
            //    heightTotal = androidView.MeasuredHeight;
            //}


            //var layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //androidView.LayoutParameters = layoutParams;
            //view.Layout(new Forms.Rectangle(0, 0, (double)270, (double)150));
            ////androidView.Layout(0, 0, widthTotal, heightTotal);
            //return androidView;

            var vRenderer = Platform.CreateRendererWithContext(view, context);
            var nativeView = vRenderer.View;
            vRenderer.Tracker.UpdateLayout();
            var layoutParams = new ViewGroup.LayoutParams((int)size.Width, (int)size.Height);
            nativeView.LayoutParameters = layoutParams;
            view.Layout(size);
            nativeView.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);


            return nativeView;
        }

        public static int IntToDip (int value, global::Android.Content.Context context)
        {
           return (int)global::Android.Util.TypedValue.ApplyDimension(global::Android.Util.ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
    }


    public class ViewCellContainer : global::Android.Widget.LinearLayout
    {
        public Xamarin.Forms.View _formsView;
        private IVisualElementRenderer _renderer;

        public ViewCellContainer(global::Android.Content.Context context, Xamarin.Forms.View formsView, IVisualElementRenderer renderer) : base(context)
        {
            _formsView = formsView;
            _renderer = renderer;
            this.AddView(_renderer.View);
        }

        //// this will layout the cell in xamarin forms and then get the height
        //// it means you can variable height cells / wrap to content etc
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            double pixels = MeasureSpec.GetSize(widthMeasureSpec);
            double num = ContextExtensions.FromPixels(this.Context, pixels);
            Forms.SizeRequest sizeRequest = _formsView.Measure(num, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);


            if (num < sizeRequest.Request.Width)
                _formsView.Layout(new Forms.Rectangle(0.0, 0.0, num, sizeRequest.Request.Height));
            else
                _formsView.Layout(new Forms.Rectangle(0.0, 0.0, sizeRequest.Request.Width, sizeRequest.Request.Height));

            //_formsView.Layout(new Forms.Rectangle(0.0, 0.0, sizeRequest.Request.Width, sizeRequest.Request.Height));



            double width = _formsView.Width;
            int measuredWidth = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, width), MeasureSpecMode.Exactly);
            double height = _formsView.Height;
            int measuredHeight = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, height), MeasureSpecMode.Exactly);
            _renderer.View.Measure(widthMeasureSpec, heightMeasureSpec);

            this.SetMeasuredDimension(measuredWidth, measuredHeight);
        }

        //protected override void OnLayout(bool changed, int l, int t, int r, int b)
        //{
        //    _renderer.UpdateLayout();
        //}
    }
}