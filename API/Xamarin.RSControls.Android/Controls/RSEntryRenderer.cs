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
using Android.Animation;
using static Android.Animation.ValueAnimator;
using System;
using Android.Views.Animations;
using System.Threading;
using Android.Graphics.Drawables;
using Android.Text.Method;
using Android.Support.Graphics.Drawable;
using Android.Support.V7.Widget;
using Xamarin.RSControls.Interfaces;

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

            if (this.Element is IRSControl && (this.Element as IRSControl).RSEntryStyle != Enums.RSEntryStyleSelectionEnum.Default)
                this.Element.Placeholder = "";

            //if(!this.Element.IsPassword)
            //    SetIcon(Control);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error" && this.Control is CustomEditText && !isTextInputLayout)
                (this.Control as CustomEditText).ErrorMessage = (this.Element as RSEntry).Error;

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
            if ((this.Element as IRSControl).RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Default)
                return base.CreateNativeControl();
            else
                return new CustomEditText(Context, this.Element as IRSControl);
        }

        private void SetIcon(global::Android.Widget.EditText nativeEditText)
        {
            string rightPath = string.Empty;
            string leftPath = string.Empty;
            Drawable rightDrawable = null;
            Drawable leftDrawable = null;


            rightPath = "Samples/Data/SVG/calendar.svg";


            int pixel = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)20, Context.Resources.DisplayMetrics);
            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = rightPath, HeightRequest = pixel, WidthRequest = pixel, Color = global::Android.Graphics.Color.DarkGray.ToColor() };
            var convertedRightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new Rectangle(), Context);
            rightDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedRightView, pixel, pixel));


            //Set Drawable to control
            nativeEditText.SetCompoundDrawablesRelativeWithIntrinsicBounds(null, null, rightDrawable, null);
        }
    }

    public class CustomEditText : FormsEditText, IOnTouchListener
    {
        IRSControl rSControl;
        private int topSpacing;
        private int bottomSpacing;
        private int leftSpacing;
        private int rightSpacing;
        private int leftRightSpacingLabels;
        private int borderWidth;
        private int borderWidthFocused;
        private int labelsTextSize;
        private int floatingHintClipPadding;
        private int corectCorners;
        private bool errorEnabled = false;
        private bool isFocused;
        private string floatingHintText;
        private string errorMessage;
        private string helperMessage;
        private string counterMessage;
        private TextPaint floatingHintPaint;
        private TextPaint errorPaint;
        private TextPaint helperPaint;
        private TextPaint counterPaint;
        Rect floatingHintBounds;
        Rect conterMessageBounds;
        Paint borderPaint;
        Paint filledPaint;
        private float floatingHintYPostion;
        private bool isFloatingHintAnimating = false;
        private float floatingHintYPostionWhenFloating;
        private float floatingHintYPostionFilledBorder;
        private bool hasInitfloatingHintYPosition = false;
        private float errorYPosition;
        private float helperYPosition;
        private float counterYPosition;

        //Password icon drawable
        private AnimatedStateListDrawable showPassword;

        //Animators
        ValueAnimator errorHelperMessageAnimator;
        ValueAnimator floatingHintAnimator;


        //Click effect on drawable
        private Paint ripplePaint;
        private float radius = 0;
        private bool ripple = false;

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                if (value == null)
                    errorMessage = string.Empty;
                else
                    errorMessage = value;

                //Force Animation because draw method executed after this setter
                if (isFocused != this.IsFocused && !isFloatingHintAnimating)
                    AnimateFloatingHint();

                if (!string.IsNullOrEmpty(value))
                {
                    errorEnabled = true;
                    AnimateErrorHelperMessage(0, 255, errorYPosition + bottomSpacing, this.Height - bottomSpacing - errorPaint.Ascent());
                }
                else
                {
                    errorEnabled = false;
                    AnimateErrorHelperMessage(255, 0, errorYPosition + bottomSpacing, this.Height - bottomSpacing - errorPaint.Ascent() + (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Context.Resources.DisplayMetrics));
                }
            }
        }


        public CustomEditText(Context context, IRSControl rSControl) : base(context)
        {
            this.rSControl = rSControl;

            //Init values
            isFocused = this.IsFocused;
            labelsTextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 12, Context.Resources.DisplayMetrics);
            borderWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Context.Resources.DisplayMetrics);
            borderWidthFocused = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, Context.Resources.DisplayMetrics);
            floatingHintClipPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Context.Resources.DisplayMetrics);
            corectCorners = borderWidthFocused - borderWidth;
            leftRightSpacingLabels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 12, Context.Resources.DisplayMetrics);
            errorMessage = string.Empty;


            //Set Padding
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                SetPaddingValues(5, 10, 5, 10);
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                SetPaddingValues(0, 6, 0, 4);
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                SetPaddingValues(5, 15, 5, 7);

            //Floating Hint
            CreateFloatingHint();

            //Error Message
            CreateErrorMessage();

            //Helper Message
            CreateHelperMessage();

            //Create Counter Message
            CreateCounterMessage();

            //Rounded border
            CreateRoundedBorder();

            //Filled container
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                CreateFilledBorder();

            //Password
            if (this.rSControl.IsPassword)
                CreatePasswordIcon();

            if (this.rSControl.CounterMaxLength != -1)
                this.AfterTextChanged += CustomEditText_AfterTextChanged;


            //Ripple effect on click drawable
            CreateRippleEffect();

            //Create errorMessage animator TODO check if error seted
            CreateErrorHelperMessageAnimator();
            CreateFloatingHintAnimator();

            //On touch listener
            this.SetOnTouchListener(this);
        }


        private void SetPaddingValues(int leftPadd, int topPadd, int rightPadd, int bottomPad)
        {
            topSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics);

            if (this.rSControl.HasError || !string.IsNullOrEmpty(this.rSControl.Helper) || this.rSControl.CounterMaxLength != -1)
                bottomSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics);
            else
                bottomSpacing = 0;

            leftSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, leftPadd, Context.Resources.DisplayMetrics);
            rightSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, rightPadd, Context.Resources.DisplayMetrics);


            //Fix for some devices paddings are wrong device related bug ? this is a hack, if it breaks on other devices just use default paddings
            //var underTextLineHeight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Context.Resources.DisplayMetrics);
            var topPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, topPadd, Context.Resources.DisplayMetrics);
            var bottomPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, bottomPad, Context.Resources.DisplayMetrics);


            //Set Padding
            this.SetPadding(this.PaddingLeft + leftSpacing, topPadding + topSpacing, this.PaddingRight + rightSpacing, bottomPadding + bottomSpacing);
        }
        private void SetColors()
        {
            if (this.IsFocused)
            {
                borderPaint.StrokeWidth = borderWidthFocused;
                if (errorEnabled)
                {
                    borderPaint.Color = global::Android.Graphics.Color.Red;
                    floatingHintPaint.Color = global::Android.Graphics.Color.Red;
                }
                else
                {
                    borderPaint.Color = global::Android.Graphics.Color.Blue;
                    floatingHintPaint.Color = global::Android.Graphics.Color.Blue;
                }
            }
            else
            {
                borderPaint.StrokeWidth = borderWidth;
                if (errorEnabled)
                {
                    borderPaint.Color = global::Android.Graphics.Color.Red;
                    floatingHintPaint.Color = global::Android.Graphics.Color.Red;
                }
                else
                {
                    borderPaint.Color = global::Android.Graphics.Color.DarkGray;
                    floatingHintPaint.Color = global::Android.Graphics.Color.Gray;
                }
            }
        }
        private bool CanAnimate(bool focused)
        {
            if (focused)
            {
                if (IsFloating() && string.IsNullOrEmpty(this.Text) && !errorEnabled)
                    return true;
                else if (!IsFloating())
                    return true;
                else
                    return false;
            }
            else
            {
                if (!IsFloating())
                    return true;
                else
                    return false;
            }
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

            floatingHintText = this.rSControl.Placeholder;
            global::Android.Graphics.Color color = new global::Android.Graphics.Color(this.CurrentHintTextColor);
            floatingHintPaint.Color = color;
            floatingHintPaint.TextSize = labelsTextSize;
            floatingHintPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            floatingHintPaint.AntiAlias = true;

            //Init y when floating
            floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBounds);

            if(this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                floatingHintYPostionWhenFloating = topSpacing + floatingHintBounds.Height() + 5;
            else
                floatingHintYPostionWhenFloating = topSpacing + floatingHintBounds.Height() / 2;

            //Update textSize
            floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
        }
        private void CreateErrorMessage()
        {
            errorPaint = new TextPaint();

            errorPaint.Color = global::Android.Graphics.Color.Red;
            errorPaint.Alpha = 0; //Set alpha after color set or not working. Range 0-255
            errorPaint.TextSize = labelsTextSize;
            errorPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            errorPaint.AntiAlias = true;
        }
        private void CreateHelperMessage()
        {
            helperPaint = new TextPaint();
            helperMessage = this.rSControl.Helper;

            //global::Android.Graphics.Color color = new global::Android.Graphics.Color(this.CurrentHintTextColor);
            helperPaint.Color = global::Android.Graphics.Color.Gray;
            helperPaint.TextSize = labelsTextSize;
            helperPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            helperPaint.AntiAlias = true;
        }
        private void CreateCounterMessage()
        {
            counterPaint = new TextPaint();
            conterMessageBounds = new Rect();
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);

            counterPaint.Color = global::Android.Graphics.Color.DarkGray;
            counterPaint.TextSize = labelsTextSize;
            counterPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            counterPaint.AntiAlias = true;

            //Set bounds to get width
            counterPaint.GetTextBounds(counterMessage, 0, counterMessage.Length, conterMessageBounds);
        }
        private void CreateRoundedBorder()
        {
            borderPaint = new Paint();

            borderPaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
            borderPaint.Color = global::Android.Graphics.Color.DarkGray;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.AntiAlias = true;
            //borderPaint.SetShadowLayer(borderWidth, 0, borderWidth, global::Android.Graphics.Color.Gray);
        }
        private void CreateFilledBorder()
        {
            filledPaint = new Paint();

            filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            var color = new global::Android.Graphics.Color(global::Android.Support.V4.Content.ContextCompat.GetColor(Context, Droid.Resource.Color.mtrl_textinput_filled_box_default_background_color));
            filledPaint.Color = global::Android.Graphics.Color.White;
            //filledPaint.StrokeWidth = borderWidth;
            filledPaint.AntiAlias = true;
        }
        private void CreatePasswordIcon()
        {
            showPassword = (AnimatedStateListDrawable)Context.GetDrawable(RSControls.Droid.Resource.Drawable.custom_design_password_eye);
            showPassword.SetTint(global::Android.Graphics.Color.DarkGray);
            this.SetCompoundDrawablesWithIntrinsicBounds(null, null, showPassword, null);
        }
        private void CreateRippleEffect()
        {
            ripplePaint = new Paint();
            ripplePaint.AntiAlias = true;
            ripplePaint.Color = global::Android.Graphics.Color.LightGray;
        }
        private void DrawFingerPrint(Canvas canvas, Rect textRect)
        {
            var divideRadiusValue = showPassword.IntrinsicWidth / 12;

            canvas.DrawCircle(textRect.Right - this.PaddingRight - (showPassword.IntrinsicWidth / 2),
                              textRect.Top + showPassword.IntrinsicHeight / 2 + this.PaddingTop,
                              radius,
                              ripplePaint);

            if (radius <= this.PaddingRight * 2)
            {
                radius += divideRadiusValue;
                Invalidate();
            }
            else
            {
                if (ripplePaint.Alpha > 0)
                {
                    ripplePaint.Alpha -= 15;
                    Invalidate();
                }
                else
                {
                    ripplePaint.Alpha = 0;
                    radius = 0f;
                    ripple = false;
                    Invalidate();
                }
            }
        } //Ripple effect on drawable click


        //Draw
        public override void Draw(Canvas canvas)
        {
            //Init floatingHint Y value
            if (!hasInitfloatingHintYPosition)
            {
                if(this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                {
                    floatingHintYPostionFilledBorder = (this.Height - bottomSpacing) / 2 - ((floatingHintPaint.GetFontMetrics().Bottom + floatingHintPaint.GetFontMetrics().Top) / 2);
                    floatingHintYPostion = floatingHintYPostionFilledBorder;
                }
                else
                {
                    floatingHintYPostionFilledBorder = this.Baseline;
                    floatingHintYPostion = floatingHintYPostionFilledBorder;
                }

                hasInitfloatingHintYPosition = true;

                if (errorPaint != null)
                    errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent();

                if (helperPaint != null)
                    helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent();

                if (counterPaint != null)
                    counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent();
            }

            //When EditText is focused Animate
            if (isFocused != this.IsFocused && !isFloatingHintAnimating)
            {
                if (this.IsFocused)
                {
                    if (CanAnimate(this.IsFocused))
                        AnimateFloatingHint();
                }
                else
                {
                    if (CanAnimate(this.IsFocused))
                        AnimateFloatingHint();
                }

                isFocused = this.IsFocused;
            }

            //Update Colors
            SetColors();

            //Text rect bounds
            Rect textRect = new Rect();
            this.GetDrawingRect(textRect);

            //Update Rounded border
            UpdateBorder(canvas, textRect);

            //Update Floating Hint
            UpdateFloatingHint(canvas, textRect);

            //Update Error Message
            if (errorEnabled)
                UpdateErrorMessage(canvas, textRect);

            //Update Helper Message
            if (!string.IsNullOrEmpty(this.rSControl.Helper))
                UpdateHelperMessage(canvas, textRect);

            //Update Counter Message
            if (this.rSControl.CounterMaxLength != -1)
                UpdateCounterMessage(canvas, textRect);



            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                var drawable = Context.GetDrawable(RSControls.Droid.Resource.Drawable.avd_hide_password);
                drawable.SetTint(global::Android.Graphics.Color.DarkGray);
                var y1 = (this.Height - bottomSpacing) / 2 - drawable.IntrinsicHeight / 2;
                var y2 = (this.Height - bottomSpacing) / 2 + drawable.IntrinsicHeight / 2;
                drawable.SetBounds(textRect.Right - this.PaddingRight - drawable.IntrinsicWidth, y1, textRect.Right - this.PaddingRight, y2);
                drawable.Draw(canvas);
            }


            if (ripple)
                DrawFingerPrint(canvas, textRect);


            base.OnDraw(canvas);
        }

        
        //Update Methods
        private void UpdateFloatingHint(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(floatingHintText, textRect.Left + leftRightSpacingLabels, floatingHintYPostion, floatingHintPaint);
        }
        private void UpdateErrorMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(errorMessage, textRect.Left + leftRightSpacingLabels, errorYPosition, errorPaint);
        }
        private void UpdateHelperMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(helperMessage, textRect.Left + leftRightSpacingLabels, helperYPosition, helperPaint);
        }
        private void UpdateCounterMessage(Canvas canvas, Rect textRect)
        {
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);
            canvas.DrawText(counterMessage, textRect.Right - (conterMessageBounds.Width()) - leftRightSpacingLabels, counterYPosition, counterPaint);
        }
        private void UpdateBorder(Canvas canvas, Rect textRect)
        {
            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                //Clip top border first and check if api < 26
                canvas.Save();

                if (IsFloating())
                {
                    if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                    {
                        canvas.ClipRect(textRect.Left + leftRightSpacingLabels - floatingHintClipPadding,
                                        textRect.Top,
                                        textRect.Left + floatingHintBounds.Width() + leftRightSpacingLabels + floatingHintClipPadding,
                                        textRect.Top + topSpacing + floatingHintBounds.Height(),
                                        global::Android.Graphics.Region.Op.Difference);
                    }
                    else
                    {
                        canvas.ClipOutRect(textRect.Left + leftRightSpacingLabels - floatingHintClipPadding,
                                           textRect.Top,
                                           textRect.Left + floatingHintBounds.Width() + leftRightSpacingLabels + floatingHintClipPadding,
                                           textRect.Top + topSpacing + floatingHintBounds.Height());
                    }
                }

                canvas.DrawRoundRect(new RectF(textRect.Left + corectCorners,
                                               textRect.Top + corectCorners + topSpacing,
                                               textRect.Right - corectCorners,
                                               textRect.Bottom - corectCorners - bottomSpacing),
                                               this.rSControl.BorderRadius, this.rSControl.BorderRadius,
                                               borderPaint);

                canvas.Restore();

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                canvas.DrawLine(textRect.Left + this.PaddingLeft - leftSpacing,
                                textRect.Bottom - bottomSpacing - borderWidth,
                                textRect.Right - this.PaddingRight + rightSpacing,
                                textRect.Bottom - bottomSpacing,
                                borderPaint);

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                canvas.DrawLine(textRect.Left - leftSpacing + corectCorners,
                                textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth,
                                textRect.Right + rightSpacing - corectCorners,
                                textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth,
                                borderPaint);

                canvas.Save();

                if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                {
                    canvas.ClipRect(textRect.Left,
                                    textRect.Bottom - bottomSpacing - borderWidthFocused,
                                    textRect.Right,
                                    textRect.Bottom,
                                    global::Android.Graphics.Region.Op.Difference);
                }
                else
                {
                    canvas.ClipOutRect(textRect.Left, textRect.Bottom - bottomSpacing - borderWidthFocused, textRect.Right, textRect.Bottom);
                }


                canvas.DrawRoundRect(new RectF(textRect.Left,
                                               textRect.Top,
                                               textRect.Right,
                                               this.Bottom + this.Height / 2),
                                               this.rSControl.BorderRadius, this.rSControl.BorderRadius,
                                               filledPaint);

                canvas.Restore();
            }
        }


        //Floating hint animation
        private void CreateFloatingHintAnimator()
        {
            floatingHintAnimator = new ValueAnimator();
            floatingHintAnimator.Update += FloatingHintAnimator_Update;
            floatingHintAnimator.AnimationEnd += FloatingHintAnimator_AnimationEnd;   
            floatingHintAnimator.SetDuration(200);
        }
        private void FloatingHintAnimator_AnimationEnd(object sender, EventArgs e)
        {
            isFloatingHintAnimating = false;
        }
        private void FloatingHintAnimator_Update(object sender, AnimatorUpdateEventArgs e)
        {
            floatingHintPaint.TextSize = (float)e.Animation.GetAnimatedValue("textSize");
            floatingHintYPostion = (float)e.Animation.GetAnimatedValue("YPosition");
            Invalidate();
        }
        private void AnimateFloatingHint()
        {
            isFloatingHintAnimating = true;
            float textSizeStart;
            float textSizeEnd;
            float yStart;
            float yEnd;

            if (!IsFloating())
            {
                textSizeStart = labelsTextSize;
                textSizeEnd = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionFilledBorder;
            }
            else
            {
                textSizeStart = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                textSizeEnd = labelsTextSize;
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionWhenFloating;
            }

            PropertyValuesHolder propertyTextSizeError = PropertyValuesHolder.OfFloat("textSize", textSizeStart, textSizeEnd);
            PropertyValuesHolder propertyYPositionError = PropertyValuesHolder.OfFloat("YPosition", yStart, yEnd);
            floatingHintAnimator.SetValues(propertyTextSizeError, propertyYPositionError);


            //Start animation
            floatingHintAnimator.Start();
        }


        //Animate error helper message
        private void CreateErrorHelperMessageAnimator()
        {
            errorHelperMessageAnimator = new ValueAnimator();
            errorHelperMessageAnimator.Update += ErrorHelperMessageAnimator_Update;
            errorHelperMessageAnimator.SetDuration(250);
        }
        private void AnimateErrorHelperMessage(int alphaStart, int alphaEnd, float yStart, float yEnd)
        {
            //TODO Check if error needed
            PropertyValuesHolder propertyAlphaError = PropertyValuesHolder.OfInt("alphaError", alphaStart, alphaEnd);
            PropertyValuesHolder propertyYPositionError = PropertyValuesHolder.OfFloat("YPositionError", yStart, yEnd);


            //TODO Check if helper needed
            PropertyValuesHolder propertyAlphaHelper = PropertyValuesHolder.OfInt("alphaHelper", alphaEnd, alphaStart); //invert alpha of error since we want one at a time
            PropertyValuesHolder propertyYPositionHelper = PropertyValuesHolder.OfFloat("YPositionHelper", yStart, yEnd);//Same

            errorHelperMessageAnimator.SetValues(propertyAlphaError, propertyYPositionError, propertyAlphaHelper, propertyYPositionHelper);

            //Start animation
            errorHelperMessageAnimator.Start();
        }
        private void ErrorHelperMessageAnimator_Update(object sender, AnimatorUpdateEventArgs e)
        {
            errorPaint.Alpha = (int)e.Animation.GetAnimatedValue("alphaError");
            helperPaint.Alpha = (int)e.Animation.GetAnimatedValue("alphaHelper");
            errorYPosition = (float)e.Animation.GetAnimatedValue("YPositionError");
            helperYPosition = (float)e.Animation.GetAnimatedValue("YPositionHelper");
            Invalidate();
        }

        //On touch listener
        public bool OnTouch(global::Android.Views.View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down && this.rSControl.IsPassword)
            {
                var width = this.Height - this.PaddingBottom - this.PaddingTop;

                if (e.GetX() >= this.Right - this.PaddingLeft - width && e.GetX() <= this.Right)
                {
                    if (this.InputType == InputTypes.TextVariationPassword)
                    {
                        this.InputType = InputTypes.TextVariationVisiblePassword;
                        this.TransformationMethod = new PasswordTransformationMethod();
                        this.Selected = false;//Used as password icon state
                    }
                    else
                    {
                        this.InputType = InputTypes.TextVariationPassword;
                        this.TransformationMethod = null;
                        this.Selected = true;//Used as password icon state
                    }

                    radius = 0;
                    ripplePaint.Alpha = 255;
                    ripple = true;
                    this.SetSelection(this.Length());
                    Invalidate();
                    return true;
                }
                else
                    return base.OnTouchEvent(e);
            }
            else
                return base.OnTouchEvent(e);
        }


        //Edit text listener to set color if error enables used for the counter
        private void CustomEditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (this.Length() > this.rSControl.CounterMaxLength)
                errorEnabled = true;
            else if (!string.IsNullOrEmpty(this.ErrorMessage))
                errorEnabled = true;
            else
                errorEnabled = false;

            this.rSControl.Counter = this.Length();
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                if(floatingHintAnimator != null)
                {
                    floatingHintAnimator.Update -= FloatingHintAnimator_Update;
                    floatingHintAnimator.AnimationEnd -= FloatingHintAnimator_AnimationEnd;
                }

                if (errorHelperMessageAnimator != null)
                    errorHelperMessageAnimator.Update -= ErrorHelperMessageAnimator_Update;

                if (this.rSControl.CounterMaxLength != -1)
                    this.AfterTextChanged -= CustomEditText_AfterTextChanged;
            }
        }
    }
}