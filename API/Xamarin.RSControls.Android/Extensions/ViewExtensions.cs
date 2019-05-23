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
            var shape = new ShapeDrawable(new RectShape());
            shape.Paint.Color = borderColor;
            shape.Paint.StrokeWidth = 2;
            shape.Paint.SetStyle(Paint.Style.Stroke);
            int upDown = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 6, context.Resources.DisplayMetrics);
            int leftRight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, context.Resources.DisplayMetrics);
            shape.SetPadding(leftRight, upDown, leftRight, upDown);
            shape.Paint.AntiAlias = true;
            shape.Paint.Flags = PaintFlags.AntiAlias;


            //Rounded shape
            GradientDrawable gd = new GradientDrawable();
            gd.SetCornerRadius(12);
            gd.SetShape(ShapeType.Rectangle);
            gd.SetColor(Color.White);
            gd.SetStroke(2, Color.Gray);


            nativeView.SetBackground(gd);
        }
    }
}