using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;
using System.ComponentModel;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Text;
using static Android.Views.View;
using Android.Views;
using Android.Util;

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        private bool isTextInputLayout;

        public RSEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error" && !isTextInputLayout)
                this.Control.Error = (this.Element as RSEntry).Error;

            //Draw border or not
            if ((this.Element as RSEntry).HasBorder)
                Extensions.ViewExtensions.DrawBorder(this.Control, Context, global::Android.Graphics.Color.Black);
        }

        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
        }

        protected override FormsEditText CreateNativeControl()
        {
            return new CustomEditText(Context);
        }
    }

    public class CustomEditText : FormsEditText
    {
        private int topSpacing;
        private int bottomSpacing;
        private int sidesSpacing;
        private int leftSpacingLabels;
        private int borderWidth;
        private int borderWidthFocused;
        private int labelsTextSize;
        private int floatingHintClipPadding;

        private bool errorEnabled = false;
        private bool isFocused;
        private string floatingHintText = "Floating hint";
        private int corectCorners;

        Rect floatingHintBounds;
        private TextPaint floatingHintPaint;
        private TextPaint errorPaint;
        Paint roundRectPaint;

        public CustomEditText(Context context) : base(context)
        {
            topSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics); ;
            bottomSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics); 
            sidesSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics); ;
            leftSpacingLabels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics); 

            borderWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Context.Resources.DisplayMetrics);
            borderWidthFocused = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, Context.Resources.DisplayMetrics);

            labelsTextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 12, Context.Resources.DisplayMetrics);

            floatingHintClipPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Context.Resources.DisplayMetrics);

            isFocused = this.IsFocused;

            corectCorners = borderWidthFocused - borderWidth;


            //this.SetPadding(this.PaddingLeft + sidesSpacing, this.PaddingTop + topSpacing, this.PaddingRight + sidesSpacing, this.PaddingBottom + bottomSpacing);

            //Floating Hint
            CreateFloatingHint();

            //Error Message
            CreateErrorMessage();

            //Rounded border
            CreateRoundedBorder();
        }


        public override void Draw(Canvas canvas)
        {
            base.OnDraw(canvas);

            //When EditText is focused or not change style accordingly
            if(isFocused != this.IsFocused)
            {
                if (this.IsFocused)
                {
                    roundRectPaint.StrokeWidth = borderWidthFocused;
                    roundRectPaint.Color = global::Android.Graphics.Color.Red;
                }
                else
                {
                    roundRectPaint.StrokeWidth = borderWidth;
                    roundRectPaint.Color = global::Android.Graphics.Color.DarkGray;
                }

                isFocused = this.IsFocused;
            }

            //Text rect bounds
            Rect textRect = new Rect();
            this.GetDrawingRect(textRect);

            //Update Floating Hint
            UpdateFloatingHint(canvas, textRect);

            //Update Error Message
            UpdateErrorMessage(canvas, textRect);

            //Update Rounded border
            UpdateRoundedBorder(canvas, textRect);
        }

        private bool IsFloating()
        {
            if (!string.IsNullOrEmpty(this.Text) || errorEnabled || this.IsFocused)
                return true;
            else
                return false;
        }


        //Create Methods
        private void CreateFloatingHint()
        {
            floatingHintPaint = new TextPaint();
            floatingHintBounds = new Rect();

            floatingHintPaint.TextAlign = global::Android.Graphics.Paint.Align.Left;
            floatingHintPaint.SetTypeface(Typeface.Create("Arial", TypefaceStyle.Normal));
            floatingHintPaint.Color = global::Android.Graphics.Color.Black;
            floatingHintPaint.TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 250, Context.Resources.DisplayMetrics);
            floatingHintPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            floatingHintPaint.AntiAlias = true;
        }

        private void CreateErrorMessage()
        {
            errorPaint = new TextPaint();

            errorPaint.Color = global::Android.Graphics.Color.Red;
            errorPaint.TextSize = labelsTextSize;
            errorPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            errorPaint.AntiAlias = true;
        }

        private void CreateRoundedBorder()
        {
            roundRectPaint = new Paint();

            roundRectPaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
            //roundRectPaint.Color = global::Android.Graphics.Color.DarkGray;
            //roundRectPaint.StrokeWidth = 2;
            roundRectPaint.AntiAlias = true;
        }


        //Update Methods
        private void UpdateFloatingHint(Canvas canvas, Rect textRect)
        {
            floatingHintBounds = new Rect();
            floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBounds);
            var height = floatingHintBounds.Height();
            var lol = floatingHintPaint.GetFontMetrics();
            
            var wefwe = textRect.CenterY();

            //canvas.DrawText(floatingHintText, textRect.Left + leftSpacingLabels, topSpacing + floatingHintBounds.Height() / 2, floatingHintPaint);
            canvas.DrawText(floatingHintText, textRect.Left + PaddingLeft, textRect.CenterY() + height / 2 - this.PaddingBottom - this.PaddingTop, floatingHintPaint);
        }

        private void UpdateErrorMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText("Error message", textRect.Left + leftSpacingLabels, canvas.Height - 10, errorPaint);
        }

        private void UpdateRoundedBorder(Canvas canvas, Rect textRect)
        {
            //Clip top border first and check if api < 26
            if(IsFloating())
            {
                if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                    canvas.ClipRect(textRect.Left + leftSpacingLabels - floatingHintClipPadding, textRect.Top, textRect.Left + floatingHintBounds.Width() + leftSpacingLabels + floatingHintClipPadding, textRect.Top + topSpacing + borderWidthFocused, global::Android.Graphics.Region.Op.Difference);
                else
                    canvas.ClipOutRect(textRect.Left + leftSpacingLabels - floatingHintClipPadding, textRect.Top, textRect.Left + floatingHintBounds.Width() + leftSpacingLabels + floatingHintClipPadding, textRect.Top + topSpacing + borderWidthFocused);
            }

            canvas.DrawRoundRect(new RectF(textRect.Left + corectCorners, textRect.Top + corectCorners + topSpacing, textRect.Right - corectCorners, textRect.Bottom - corectCorners - bottomSpacing), 10, 10, roundRectPaint);
        }


        //private void AnimateFloatingHint()
        //{
        //    currentAngle = 0;
        //    new Thread(new Runnable()
        //    {
        //    public void run()
        //    {
        //        while (currentAngle < maxAngle)
        //        {
        //            invalidate();
        //            try
        //            {
        //                Thread.sleep(50);
        //            }
        //            catch (InterruptedException e)
        //            {
        //                e.printStackTrace();
        //            }
        //        }
        //        currentAngle++;
        //    }
        //}).start();
    }
}