using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Droid.Controls;

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
            var vRenderer = Platform.CreateRendererWithContext(view, context);
            var androidView = vRenderer.View;

            vRenderer.Tracker.UpdateLayout();
            //vRenderer.View.contr

            var widthTotal = 0;
            var spacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 6, context.Resources.DisplayMetrics); 
            var heightTotal = 0;

            if (androidView is ViewGroup && (androidView as ViewGroup).ChildCount >= 1)
            {
                for (int i = 0; i < (androidView as ViewGroup).ChildCount; i++)
                {
                    (androidView as ViewGroup).GetChildAt(i).Measure((int)size.Width, (int)size.Height);
                    (androidView as ViewGroup).Measure((androidView as ViewGroup).GetChildAt(i).MeasuredWidth, (androidView as ViewGroup).GetChildAt(i).MeasuredHeight);

                    widthTotal += (androidView as ViewGroup).GetChildAt(i).MeasuredWidth + spacing;
                    heightTotal = (androidView as ViewGroup).GetChildAt(i).MeasuredHeight;
                }
            }
            else
            {
                androidView.Measure((int)size.Width, (int)size.Height);
                widthTotal = androidView.MeasuredWidth;
                heightTotal = androidView.MeasuredHeight;
            }


            var layoutParams = new ViewGroup.LayoutParams(widthTotal, heightTotal);
            androidView.LayoutParameters = layoutParams;
            view.Layout(new Forms.Rectangle(0, 0, (double)widthTotal, (double)heightTotal));
            androidView.Layout(0, 0, widthTotal, heightTotal);
            return androidView;
        }


        public static int IntToDip (int value, global::Android.Content.Context context)
        {
           return (int)global::Android.Util.TypedValue.ApplyDimension(global::Android.Util.ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
    }


    //public class NativeView : global::Android.Views.View
    //{
    //    public Xamarin.Forms.View formsView;
    //    private IVisualElementRenderer renderer;
    //    Xamarin.Forms.Rectangle size;

    //    public NativeView(Context context, Forms.View formsView, Xamarin.Forms.Rectangle size) : base(context)
    //    {
    //        this.size = size;
    //        this.formsView = formsView;
    //        renderer = Platform.CreateRendererWithContext(formsView, context);
    //        renderer.Tracker.UpdateLayout();
    //        var layoutParams = new ViewGroup.LayoutParams((int)size.Width, (int)size.Height);
    //        renderer.View.LayoutParameters = layoutParams;
    //        formsView.Layout(size);
    //        renderer.View.Layout(0, 0, (int)formsView.WidthRequest, (int)formsView.HeightRequest);

    //    }

    //    // this will layout the cell in xamarin forms and then get the height
    //    // it means you can variable height cells / wrap to content etc
    //    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    //    {
    //        double pixels = MeasureSpec.GetSize(widthMeasureSpec);
    //        double num = ContextExtensions.FromPixels(this.Context, pixels);
    //        Xamarin.Forms.SizeRequest sizeRequest = formsView.Measure(num, double.PositiveInfinity);
    //        formsView.Layout(new Xamarin.Forms.Rectangle(0.0, 0.0, num, sizeRequest.Request.Height));
    //        double width = formsView.Width;
    //        int measuredWidth = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, width), MeasureSpecMode.Exactly);
    //        double height = formsView.Height;
    //        int measuredHeight = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, height), MeasureSpecMode.Exactly);
    //        renderer.View.Measure(widthMeasureSpec, heightMeasureSpec);
    //        this.SetMeasuredDimension(measuredWidth, measuredHeight);
    //    }

    //    protected override void OnLayout(bool changed, int l, int t, int r, int b)
    //    {
    //        renderer.UpdateLayout();
    //    }
    //}
}