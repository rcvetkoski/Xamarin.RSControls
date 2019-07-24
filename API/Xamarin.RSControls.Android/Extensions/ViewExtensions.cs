using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.RSControls.Droid.Extensions
{
    public static class ViewExtensions
    {
        public static void DrawBorder(View nativeView, Context context, Color borderColor)
        {
            //Rekt Shape
            //var shape = new ShapeDrawable(new RectShape());
            //shape.Paint.Color = borderColor;
            //shape.Paint.StrokeWidth = 2;
            //shape.Paint.SetStyle(Paint.Style.Stroke);
            //int upDown = IntToDip(8, context);
            //int leftRight = IntToDip(8, context);
            //shape.SetPadding(leftRight, upDown, leftRight, upDown);
            //shape.Paint.AntiAlias = true;
            //shape.Paint.Flags = PaintFlags.AntiAlias;


            ////Rounded shape
            //GradientDrawable gd = new GradientDrawable();
            //gd.SetCornerRadius(12);
            //gd.SetShape(ShapeType.Rectangle);
            //gd.SetColor(Color.Transparent);
            //gd.SetStroke(2, Color.Gray);


            //nativeView.SetBackground(shape);

            nativeView.SetBackgroundResource(Resource.Color.RSRectangleBorderShapeColorStateList);
        }



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

        public static global::Android.Views.View ConvertFormsToNative(Xamarin.Forms.View view, Xamarin.Forms.Rectangle size, Context context)
        {
            var vRenderer = Platform.CreateRendererWithContext(view, context);
            var androidView = vRenderer.View;
            vRenderer.Tracker.UpdateLayout();
            var layoutParams = new ViewGroup.LayoutParams((int)size.Width, (int)size.Height);
            androidView.LayoutParameters = layoutParams;
            view.Layout(size);
            androidView.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);
            return androidView;
        }


        public static int IntToDip (int value, global::Android.Content.Context context)
        {
           return (int)global::Android.Util.TypedValue.ApplyDimension(global::Android.Util.ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);
        }
    }
}