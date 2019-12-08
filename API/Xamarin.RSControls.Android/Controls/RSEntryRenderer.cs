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
        Rect floatingHintBoundsFloating;
        Rect floatingHintBoundsNotFloating;
        Rect counterMessageBounds;
        Paint borderPaint;
        Paint filledPaint;
        private bool isFloatingHintAnimating = false;
        private float floatingHintXPostion;
        private float floatingHintXPostionFloating;
        private float floatingHintXPostionNotFloating;
        private float floatingHintYPostion;
        private float floatingHintYPostionWhenFloating;
        private float floatingHintYPostionNotFloating;
        private bool hasInitfloatingHintYPosition = false;
        private float errorYPosition;
        private float helperYPosition;
        private float counterYPosition;
        public Thickness CustomPadding;

        //icon drawables
        private Drawable leftDrawable;
        private Drawable rightDrawable;
        CustomDrawable customDrawable;
        private int leftDrawableWidthBorder;
        private int leftDrawableWidth;


        //Animators
        ValueAnimator errorHelperMessageAnimator;
        ValueAnimator floatingHintAnimator;



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




        //Constructor
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
            SetPaddingValues();

            //Set Icons if any
            SetIcons();

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


            if (this.rSControl.CounterMaxLength != -1)
                this.AfterTextChanged += CustomEditText_AfterTextChanged;

            //Create errorMessage animator TODO check if error seted
            CreateErrorHelperMessageAnimator();
            CreateFloatingHintAnimator();

            //On touch listener
            this.SetOnTouchListener(this);
        }


        private void SetPaddingValues()
        {
            if (this.rSControl.Padding.IsEmpty)
            {
                if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                    this.rSControl.Padding = new Thickness(5, 10, 5, 10);
                else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                    this.rSControl.Padding = new Thickness(0, 6, 0, 4);
                else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                    this.rSControl.Padding = new Thickness(5, 15, 5, 6);
            }

            CustomPadding = this.rSControl.Padding;


            int leftPadd = (int)CustomPadding.Left;
            int topPadd = (int)CustomPadding.Top;
            int rightPadd = (int)CustomPadding.Right;
            int bottomPad = (int)CustomPadding.Bottom;


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
            //var paddingLeft = leftDrawable != null ? 0 : this.PaddingLeft + leftSpacing;


            //Set Padding
            this.SetPadding(this.PaddingLeft + leftSpacing,
                            topPadding + topSpacing,
                            this.PaddingRight + rightSpacing,
                            bottomPadding + bottomSpacing);
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
                if (IsFloating() && string.IsNullOrEmpty(this.Text) && !errorEnabled && this.leftDrawable == null)
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
            if (!string.IsNullOrEmpty(this.Text) || errorEnabled || this.IsFocused || this.leftDrawable != null)
                return true;
            else
                return false;
        }


        //Create Methods
        private void CreateFloatingHint()
        {
            floatingHintPaint = new TextPaint();
            floatingHintBoundsFloating = new Rect();
            floatingHintBoundsNotFloating = new Rect();

            floatingHintText = this.rSControl.Placeholder;
            global::Android.Graphics.Color color = new global::Android.Graphics.Color(this.CurrentHintTextColor);
            floatingHintPaint.Color = color;
            floatingHintPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            floatingHintPaint.AntiAlias = true;

            //Set width and height of floating hint paint when floating
            floatingHintPaint.TextSize = labelsTextSize;
            floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBoundsFloating);

            //Set width and height of floating hint paint when not floating
            floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics); ;
            floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBoundsNotFloating);

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
            counterMessageBounds = new Rect();
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);

            counterPaint.Color = global::Android.Graphics.Color.DarkGray;
            counterPaint.TextSize = labelsTextSize;
            counterPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            counterPaint.AntiAlias = true;

            //Set bounds to get width
            counterPaint.GetTextBounds(counterMessage, 0, counterMessage.Length, counterMessageBounds);
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
            filledPaint.Color = color;
            //filledPaint.StrokeWidth = borderWidth;
            //filledPaint.SetShadowLayer(borderWidth, 0, borderWidth, global::Android.Graphics.Color.Gray);
            filledPaint.AntiAlias = true;
        }
        private void CreatePasswordIcon()
        {
            var passwordDrawable = Context.GetDrawable(RSControls.Droid.Resource.Drawable.custom_design_password_eye);
            passwordDrawable.SetTint(global::Android.Graphics.Color.DarkGray);
            rightDrawable = new CustomDrawable(passwordDrawable, this);
            passwordDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);
            rightDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);
        }

        private void  SetIcons()
        {
            string rightPath = string.Empty;
            string leftPath = string.Empty;

            //Password
            if(this.rSControl.IsPassword)
                CreatePasswordIcon();


            //Right Icon
            if (!this.rSControl.IsPassword)
            {
                if (rSControl.RightIcon == null)
                    rightPath = "Samples/Data/SVG/calendar.svg";
                else
                    rightPath = rSControl.RightIcon;

                int pixel = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)rSControl.IconSize, Context.Resources.DisplayMetrics);
                RSSvgImage rightSvgIcon = new RSSvgImage() { Source = rightPath, HeightRequest = pixel, WidthRequest = pixel, Color = rSControl.IconColor };
                var convertedRightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new Rectangle(), Context);
                var drawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedRightView, pixel, pixel));
                this.rightDrawable = new CustomDrawable(drawable, this);
                drawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
                this.rightDrawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
            }


            //Left Icon
            if (rSControl.LeftIcon != null)
            {
                leftPath = rSControl.LeftIcon;
                int pixel = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)rSControl.IconSize, Context.Resources.DisplayMetrics);
                RSSvgImage leftSvgIcon = new RSSvgImage() { Source = leftPath, HeightRequest = pixel, WidthRequest = pixel, Color = rSControl.IconColor };
                var convertedLeftView = Extensions.ViewExtensions.ConvertFormsToNative(leftSvgIcon, new Rectangle(), Context);
                var drawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedLeftView, pixel, pixel));
                this.leftDrawable = new CustomDrawable(drawable, this);
                drawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
                this.leftDrawable.SetBounds(0, 0, drawable.IntrinsicWidth, drawable.IntrinsicHeight);
            }


            var dr = Context.GetDrawable(RSControls.Droid.Resource.Drawable.avd_hide_password);
            dr.SetTint(global::Android.Graphics.Color.DarkGray);
            customDrawable = new CustomDrawable(dr, this);
            

            //Set Drawable to control
            this.SetCompoundDrawables(this.leftDrawable, null, this.rightDrawable, null);
            this.CompoundDrawablePadding = 10;

            leftDrawableWidth = leftDrawable != null ? leftDrawable.IntrinsicWidth + this.CompoundDrawablePadding : 0;
        }

        //Draw
        public override void Draw(Canvas canvas)
        {
            //Text rect bounds
            Rect textRect = new Rect();
            this.GetDrawingRect(textRect);


            //Init floatingHint X and Y values
            if (!hasInitfloatingHintYPosition)
            {
                if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + this.PaddingLeft;
                    floatingHintXPostionNotFloating = textRect.Left + this.PaddingLeft;



                    //Y
                    floatingHintYPostionWhenFloating = topSpacing + floatingHintBoundsFloating.Height();
                    floatingHintYPostionNotFloating = (this.Height - bottomSpacing) / 2 - ((floatingHintBoundsNotFloating.Bottom + floatingHintBoundsNotFloating.Top) / 2);

                    if (IsFloating())
                    {
                        floatingHintPaint.TextSize = labelsTextSize;
                        floatingHintXPostion = floatingHintXPostionFloating;
                        floatingHintYPostion = floatingHintYPostionWhenFloating;
                    }
                    else
                    {
                        floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                        floatingHintXPostion = floatingHintXPostionNotFloating;
                        floatingHintYPostion = floatingHintYPostionNotFloating;
                    }
                }
                else
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + leftRightSpacingLabels;
                    floatingHintXPostionNotFloating = textRect.Left + leftRightSpacingLabels;


                    //Y
                    floatingHintYPostionWhenFloating = topSpacing + floatingHintBoundsFloating.Height() / 2;
                    floatingHintYPostionNotFloating = this.Baseline;

                    if (IsFloating())
                    {
                        floatingHintPaint.TextSize = labelsTextSize;
                        floatingHintXPostion = floatingHintXPostionFloating;
                        floatingHintYPostion = floatingHintYPostionWhenFloating;
                    }
                    else
                    {
                        floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                        floatingHintXPostion = floatingHintXPostionNotFloating;
                        floatingHintYPostion = floatingHintYPostionNotFloating;
                    }
                }




                if (errorPaint != null)
                    errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent();

                if (helperPaint != null)
                    helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent();

                if (counterPaint != null)
                    counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent();


                hasInitfloatingHintYPosition = true;
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

            UpdateClick(canvas, textRect);//Adds lag to investigate

            base.OnDraw(canvas);
        }


        private void UpdateClick(Canvas canvas, Rect textRect)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - customDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + customDrawable.IntrinsicHeight / 2;

            customDrawable.drawable.SetBounds(textRect.Right - this.PaddingRight - 10 - customDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - 10 - customDrawable.IntrinsicWidth, center2);
            customDrawable.SetBounds(textRect.Right - this.PaddingRight - 10 - customDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - 10 - customDrawable.IntrinsicWidth, center2);

            customDrawable.Callback = this;
            customDrawable.Draw(canvas);
            //Invalidate();
        }


        //Update Methods
        private void UpdateFloatingHint(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(floatingHintText, textRect.Left + floatingHintXPostion, floatingHintYPostion, floatingHintPaint);
        }
        private void UpdateErrorMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(errorMessage, textRect.Left + floatingHintXPostionFloating, errorYPosition, errorPaint);
        }
        private void UpdateHelperMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(helperMessage, textRect.Left + floatingHintXPostionFloating, helperYPosition, helperPaint);
        }
        private void UpdateCounterMessage(Canvas canvas, Rect textRect)
        {
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);
            canvas.DrawText(counterMessage, textRect.Right - (counterMessageBounds.Width()) - leftRightSpacingLabels, counterYPosition, counterPaint);
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
                                        textRect.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels + floatingHintClipPadding,
                                        textRect.Top + topSpacing + floatingHintBoundsFloating.Height(),
                                        global::Android.Graphics.Region.Op.Difference);
                    }
                    else
                    {
                        canvas.ClipOutRect(textRect.Left + leftRightSpacingLabels - floatingHintClipPadding,
                                           textRect.Top,
                                           textRect.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels + floatingHintClipPadding,
                                           textRect.Top + topSpacing + floatingHintBoundsFloating.Height());
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
                canvas.DrawRect(textRect.Left + corectCorners,
                                textRect.Bottom - bottomSpacing,
                                textRect.Right + rightSpacing - corectCorners,
                                textRect.Bottom - bottomSpacing,
                                borderPaint);

                canvas.Save();

                if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                {
                    canvas.ClipRect(textRect.Left,
                                    textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth,
                                    textRect.Right,
                                    textRect.Bottom,
                                    global::Android.Graphics.Region.Op.Difference);
                }
                else
                {
                    canvas.ClipOutRect(textRect.Left, textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth, textRect.Right, textRect.Bottom);
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
                yEnd = floatingHintYPostionNotFloating;
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
            if (e.Action == MotionEventActions.Down)
            {
                if(this.rSControl.IsPassword)
                {
                    var width = rightDrawable.IntrinsicWidth;
                    var width2 = customDrawable.IntrinsicWidth;

                    if (e.GetX() >= this.Right - this.PaddingLeft - width && e.GetX() <= this.Right)
                    {
                        if (this.InputType == InputTypes.TextVariationPassword)
                        {
                            this.InputType = InputTypes.TextVariationVisiblePassword;
                            this.TransformationMethod = new PasswordTransformationMethod();
                            (rightDrawable as CustomDrawable).Selected = false;
                            this.Selected = false;//Used as password icon state
                        }
                        else
                        {
                            (rightDrawable as CustomDrawable).Selected = true;
                            this.InputType = InputTypes.TextVariationPassword;
                            this.TransformationMethod = null;
                            this.Selected = true;//Used as password icon state
                        }

                        this.SetSelection(this.Length());
                        Invalidate();
                        return true;
                    }
                    else if (e.GetX() >= customDrawable.Bounds.Left && e.GetX() <= customDrawable.Bounds.Left + width2)
                    {
                        if (customDrawable.Selected)
                        {
                            customDrawable.Selected = false;
                        }
                        else
                        {
                            customDrawable.Selected = true;
                        }

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

    public class CustomDrawable : Drawable
    {
        public Drawable drawable;
        private CustomEditText editText;

        //Click effect on drawable
        private Paint ripplePaint;
        private float radius = 0;
        private bool ripple = false;

        public CustomDrawable(Drawable drawable, CustomEditText editText) : base()
        {
            this.drawable = drawable;
            this.editText = editText;
            this.selected = true;

            CreateRippleEffect();
        }

        public override int Opacity => throw new NotImplementedException();

        public override void Draw(Canvas canvas)
        {
            var y = (float)Math.Abs(this.editText.CustomPadding.Top - this.editText.CustomPadding.Bottom) * 2;

            //Icon click effect
            if (ripple)
                DrawFingerPrint(canvas, y);


            //align to top
            canvas.Save();
            canvas.Translate(0, -y);
            this.drawable.Draw(canvas);
            canvas.Restore();

            //this.drawable.Draw(canvas);
        }

        public override void SetAlpha(int alpha)
        {

        }

        public override void SetColorFilter(ColorFilter colorFilter)
        {

        }

        public override int IntrinsicHeight => this.drawable.IntrinsicHeight;
        public override int IntrinsicWidth => this.drawable.IntrinsicWidth;


        private static int[] STATE_SELECTED;
        private bool selected;
        public virtual bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;

                if (value)
                    STATE_SELECTED = new int[] { global::Android.Resource.Attribute.StateSelected };
                else
                    STATE_SELECTED = new int[] { -global::Android.Resource.Attribute.StateSelected };

                this.drawable.SetState(STATE_SELECTED);


                radius = 0;
                ripplePaint.Alpha = 255;
                ripple = true;
            }
        }


        private void CreateRippleEffect()
        {
            ripplePaint = new Paint();
            ripplePaint.AntiAlias = true;
            ripplePaint.Color = global::Android.Graphics.Color.LightGray;
        }
        private void DrawFingerPrint(Canvas canvas, float y)
        {
            var divideRadiusValue = this.drawable.IntrinsicWidth / 12;

            canvas.DrawCircle(this.drawable.Bounds.CenterX(),
                              this.drawable.Bounds.CenterY() - y,
                              radius,
                              ripplePaint);

            if (radius <= this.drawable.IntrinsicWidth / 2 + 10)
            {
                radius += divideRadiusValue;
                //InvalidateSelf();
            }
            else
            {
                if (ripplePaint.Alpha > 0)
                {
                    ripplePaint.Alpha -= 15;
                    //InvalidateSelf();
                }
                else
                {
                    ripplePaint.Alpha = 0;
                    radius = 0f;
                    ripple = false;
                    //InvalidateSelf();
                }
            }

            this.editText.PostInvalidate();
        }
    }
}