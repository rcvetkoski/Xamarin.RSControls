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
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using Xamarin.RSControls.Helpers;

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

            if (this.Element is IRSControl)
                this.Element.Placeholder = "";
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Error" && this.Control is CustomEditText && !isTextInputLayout)
                (this.Control as CustomEditText).ErrorMessage = (this.Element as RSEntry).Error;
        }

        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
        }

        protected override FormsEditText CreateNativeControl()
        {
            return new CustomEditText(Context, this.Element as IRSControl);
        }
    }

    public class CustomEditText : FormsEditText
    {
        private IRSControl rSControl;
        private int topSpacing;
        private int bottomSpacing;
        private int leftRightSpacingLabels;
        private float borderWidth;
        private float borderWidthFocused;
        private int labelsTextSize;
        private int floatingHintClipPadding;
        private float corectCorners;
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
        private Rect floatingHintBoundsFloating;
        private Rect floatingHintBoundsNotFloating;
        private Rect counterMessageBounds;
        private Paint borderPaint;
        private Paint filledPaint;
        private global::Android.Graphics.Color borderColor;
        private global::Android.Graphics.Color activeColor;
        private global::Android.Graphics.Color errorColor;
        private bool isFloatingHintAnimating = false;
        private float floatingHintXPostion;
        private float floatingHintXPostionFloating;
        private float floatingHintXPostionNotFloating;
        private float floatingHintYPostion;
        private float floatingHintYPostionFloating;
        private float floatingHintYPostionNotFloating;
        private bool hasInitfloatingHintYPosition = false;
        private float errorYPosition;
        private float helperYPosition;
        private float counterYPosition;
        private Rect textRect;

        //icon drawables
        private CustomDrawable leadingDrawable;
        private CustomDrawable rightHelpingDrawable;
        private CustomDrawable leftHelpingDrawable;
        private CustomDrawable trailingDrawable;
        private CustomDrawable leftDrawable;
        private CustomDrawable rightDrawable;
        private Paint iconsSearator;
        private int iconsSpacing;
        private int leftDrawableWidth;
        private int leadingIconWidth;
        private int trailingIconWidth;
        private int leftDrawableClip;
        private int rightDrawableClip;
        private float correctiveY;
        private bool shouldFloat;
        private bool shouldNotFloat;
        public bool IsFloating;


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
                //if (isFocused != this.IsFocused && !isFloatingHintAnimating)
                //    AnimateFloatingHint();

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
            borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.BorderWidth, Context.Resources.DisplayMetrics);
            borderWidthFocused = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.BorderWidth + 1, Context.Resources.DisplayMetrics);
            floatingHintClipPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Context.Resources.DisplayMetrics);
            corectCorners = borderWidthFocused - borderWidth;
            leftRightSpacingLabels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 14, Context.Resources.DisplayMetrics);
            iconsSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics);
            errorMessage = string.Empty;
            borderColor = rSControl.BorderColor.ToAndroid();
            activeColor = rSControl.ActiveColor.ToAndroid();
            errorColor = rSControl.ErrorColor.ToAndroid();


            //Set Padding
            SetPaddingValues();

            //Floating Hint
            CreateFloatingHint();

            //Error Message
            CreateErrorMessage();

            //Helper Message
            CreateHelperMessage();

            //Create Counter Message
            CreateCounterMessage();

            //Rounded border
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                CreateRoundedBorder();

            //Filled container
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                CreateFilledBorder();

            //Underline container
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                CreateUnderlineBorder();


            if (this.rSControl.CounterMaxLength != -1)
                this.AfterTextChanged += CustomEditText_AfterTextChanged;

            //Create errorMessage animator TODO check if error seted
            CreateErrorHelperMessageAnimator();
            CreateFloatingHintAnimator();

            //On touch listener
            //this.SetOnTouchListener(this);
        }


        private void SetPaddingValues()
        {
            //Top and bottomSpacing
            topSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics);
            if (this.rSControl.HasError || !string.IsNullOrEmpty(this.rSControl.Helper) || this.rSControl.CounterMaxLength != -1)
                bottomSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics);
            else
                bottomSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics);


            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                this.SetPadding((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Left,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Top,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Right,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Bottom);

            }
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                this.SetPadding((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Left,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Top,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 6, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Right,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Bottom);

            }
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.SetPadding((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Left,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Top,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Right,
                                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 18, Context.Resources.DisplayMetrics) + (int)this.rSControl.Padding.Bottom);

            }
        }
        private void SetColors()
        {
            if (this.IsFocused)
            {
                borderPaint.StrokeWidth = borderWidthFocused;
                if (errorEnabled)
                {
                    borderPaint.Color = errorColor;
                    floatingHintPaint.Color = errorColor;

                    if (rightDrawable != null && rSControl.RightIcon.IconColor == Forms.Color.DimGray)
                        rightDrawable.drawable.SetTint(borderColor);
                    else if(rightDrawable != null)
                        rightDrawable.drawable.SetTint(rSControl.RightIcon.IconColor.ToAndroid());

                    if (leftDrawable != null && rSControl.LeftIcon.IconColor == Forms.Color.DimGray)
                        leftDrawable.drawable.SetTint(borderColor);
                    else if (leftDrawable != null)
                        leftDrawable.drawable.SetTint(rSControl.LeftIcon.IconColor.ToAndroid());

                    if (leadingDrawable != null && rSControl.LeadingIcon.IconColor == Forms.Color.DimGray)
                        leadingDrawable.drawable.SetTint(borderColor);
                    else if (leadingDrawable != null)
                        leadingDrawable.drawable.SetTint(rSControl.LeadingIcon.IconColor.ToAndroid());

                    if (trailingDrawable != null && rSControl.TrailingIcon.IconColor == Forms.Color.DimGray)
                        trailingDrawable.drawable.SetTint(borderColor);
                    else if (trailingDrawable != null)
                        trailingDrawable.drawable.SetTint(rSControl.TrailingIcon.IconColor.ToAndroid());

                    if (rightHelpingDrawable != null && rSControl.RightHelpingIcon.IconColor == Forms.Color.DimGray)
                        rightHelpingDrawable.drawable.SetTint(borderColor);
                    else if (rightHelpingDrawable != null)
                        rightHelpingDrawable.drawable.SetTint(rSControl.RightHelpingIcon.IconColor.ToAndroid());

                    if (leftHelpingDrawable != null && rSControl.LeftHelpingIcon.IconColor == Forms.Color.DimGray)
                        leftHelpingDrawable.drawable.SetTint(borderColor);
                    else if (leftHelpingDrawable != null)
                        leftHelpingDrawable.drawable.SetTint(rSControl.LeftHelpingIcon.IconColor.ToAndroid());
                }
                else
                {
                    borderPaint.Color = activeColor;
                    floatingHintPaint.Color = activeColor;

                    if (rightDrawable != null)
                        rightDrawable.drawable.SetTint(activeColor);

                    if (leftDrawable != null)
                        leftDrawable.drawable.SetTint(activeColor);

                    if (leadingDrawable != null)
                        leadingDrawable.drawable.SetTint(activeColor);

                    if (trailingDrawable != null)
                        trailingDrawable.drawable.SetTint(activeColor);

                    if (rightHelpingDrawable != null)
                        rightHelpingDrawable.drawable.SetTint(activeColor);

                    if (leftHelpingDrawable != null)
                        leftHelpingDrawable.drawable.SetTint(activeColor);
                }
            }
            else
            {
                borderPaint.StrokeWidth = borderWidth;
                if (errorEnabled)
                {
                    borderPaint.Color = errorColor;
                    floatingHintPaint.Color = errorColor;
                }
                else
                {
                    borderPaint.Color = borderColor;
                    floatingHintPaint.Color = rSControl.PlaceholderStyle.FontColor.ToAndroid();

                    if (rightDrawable != null && rSControl.RightIcon.IconColor == Forms.Color.DimGray)
                        rightDrawable.drawable.SetTint(borderColor);
                    else if (rightDrawable != null)
                        rightDrawable.drawable.SetTint(rSControl.RightIcon.IconColor.ToAndroid());

                    if (leftDrawable != null && rSControl.LeftIcon.IconColor == Forms.Color.DimGray)
                        leftDrawable.drawable.SetTint(borderColor);
                    else if (leftDrawable != null)
                        leftDrawable.drawable.SetTint(rSControl.LeftIcon.IconColor.ToAndroid());

                    if (leadingDrawable != null && rSControl.LeadingIcon.IconColor == Forms.Color.DimGray)
                        leadingDrawable.drawable.SetTint(borderColor);
                    else if (leadingDrawable != null)
                        leadingDrawable.drawable.SetTint(rSControl.LeadingIcon.IconColor.ToAndroid());

                    if (trailingDrawable != null && rSControl.TrailingIcon.IconColor == Forms.Color.DimGray)
                        trailingDrawable.drawable.SetTint(borderColor);
                    else if (trailingDrawable != null)
                        trailingDrawable.drawable.SetTint(rSControl.TrailingIcon.IconColor.ToAndroid());

                    if (rightHelpingDrawable != null && rSControl.RightHelpingIcon.IconColor == Forms.Color.DimGray)
                        rightHelpingDrawable.drawable.SetTint(borderColor);
                    else if (rightHelpingDrawable != null)
                        rightHelpingDrawable.drawable.SetTint(rSControl.RightHelpingIcon.IconColor.ToAndroid());

                    if (leftHelpingDrawable != null && rSControl.LeftHelpingIcon.IconColor == Forms.Color.DimGray)
                        leftHelpingDrawable.drawable.SetTint(borderColor);
                    else if (leftHelpingDrawable != null)
                        leftHelpingDrawable.drawable.SetTint(rSControl.LeftHelpingIcon.IconColor.ToAndroid());
                }
            }
        }

        private bool CanAnimate()
        {
            if (IsFloating)
            {
                if (string.IsNullOrEmpty(this.Text) && !errorEnabled && !this.IsFocused)
                {
                    shouldNotFloat = true;
                }
                else
                {
                    shouldNotFloat = false;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Text) || errorEnabled || this.IsFocused)
                {
                    shouldFloat = true;
                }
                else
                    shouldFloat = false;
            }

            if (shouldFloat || shouldNotFloat)
                return true;
            else
                return false;
        }
        //private bool IsFloating()
        //{
        //    if (!string.IsNullOrEmpty(this.Text) || errorEnabled || this.IsFocused)
        //        return true;
        //    else
        //        return false;
        //}
        private void PasswordCommand()
        {
            if (this.InputType == InputTypes.TextVariationPassword)
            {
                this.InputType = InputTypes.TextVariationVisiblePassword;
                this.TransformationMethod = new PasswordTransformationMethod();
            }
            else
            {
                this.InputType = InputTypes.TextVariationPassword;
                this.TransformationMethod = null;
            }
        }



        //Create Methods
        private void CreateFloatingHint()
        {
            floatingHintPaint = new TextPaint();
            floatingHintBoundsFloating = new Rect();
            floatingHintBoundsNotFloating = new Rect();

            if (rSControl.PlaceholderStyle.FontFamily != null && rSControl.PlaceholderStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.PlaceholderStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.PlaceholderStyle.FontFamily.Substring(0, index);
                floatingHintPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                floatingHintPaint.SetTypeface(Typeface.Create(rSControl.PlaceholderStyle.FontFamily, TypefaceStyle.Italic));

            floatingHintText = this.rSControl.Placeholder != null ? rSControl.Placeholder : "";
            //global::Android.Graphics.Color color = new global::Android.Graphics.Color(this.CurrentHintTextColor);
            floatingHintPaint.Color = rSControl.PlaceholderStyle.FontColor.ToAndroid();
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

            if (rSControl.ErrorStyle.FontFamily != null && rSControl.ErrorStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.ErrorStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.ErrorStyle.FontFamily.Substring(0, index);
                errorPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                errorPaint.SetTypeface(Typeface.Create(rSControl.ErrorStyle.FontFamily, TypefaceStyle.Italic));

            errorPaint.Color = errorColor;
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
            if (rSControl.HelperStyle.FontFamily != null && rSControl.HelperStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.HelperStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.HelperStyle.FontFamily.Substring(0, index);
                helperPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                helperPaint.SetTypeface(Typeface.Create(rSControl.HelperStyle.FontFamily, TypefaceStyle.Italic));

            helperPaint.Color = rSControl.HelperStyle.FontColor.ToAndroid();
            helperPaint.TextSize = labelsTextSize;
            helperPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            helperPaint.AntiAlias = true;
        }
        private void CreateCounterMessage()
        {
            counterPaint = new TextPaint();
            counterMessageBounds = new Rect();
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);


            if (rSControl.CounterStyle.FontFamily != null && rSControl.CounterStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.CounterStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.CounterStyle.FontFamily.Substring(0, index);
                counterPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                counterPaint.SetTypeface(Typeface.Create(rSControl.CounterStyle.FontFamily, TypefaceStyle.Italic));

            counterPaint.Color = rSControl.CounterStyle.FontColor.ToAndroid();
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
            borderPaint.Color = borderColor;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.AntiAlias = true;

            //Filled
            filledPaint = new Paint();
            filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            filledPaint.Color = rSControl.BorderFillColor.ToAndroid();
            filledPaint.AntiAlias = true;
            if(rSControl.ShadowEnabled)
                filledPaint.SetShadowLayer(rSControl.ShadowRadius, 0f, 0.5f, rSControl.ShadowColor.ToAndroid());

            if (!this.IsHardwareAccelerated)
                this.SetLayerType(LayerType.Software, null);
            else
                this.SetLayerType(LayerType.Hardware, null);
        }
        private void CreateFilledBorder()
        {
            //Border
            borderPaint = new Paint();
            borderPaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
            borderPaint.Color = borderColor;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.AntiAlias = true;


            //Filled
            filledPaint = new Paint();
            filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            filledPaint.Color = rSControl.BorderFillColor.ToAndroid();
            filledPaint.AntiAlias = true;
            if (rSControl.ShadowEnabled)
                filledPaint.SetShadowLayer(rSControl.ShadowRadius, 0, 1f, rSControl.ShadowColor.ToAndroid());

            if (!this.IsHardwareAccelerated)
                this.SetLayerType(LayerType.Software, null);
            else
                this.SetLayerType(LayerType.Hardware, null);
        }
        private void CreateUnderlineBorder()
        {
            borderPaint = new Paint();

            borderPaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
            borderPaint.Color = borderColor;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.AntiAlias = true;

            (this.rSControl as Forms.View).BackgroundColor = rSControl.BorderFillColor;
        }
        private void CreatePasswordIcon()
        {
            var passwordDrawable = Context.GetDrawable(RSControls.Droid.Resource.Drawable.custom_design_password_eye);
            passwordDrawable.SetTint(global::Android.Graphics.Color.DarkGray);
            rightDrawable = new CustomDrawable(passwordDrawable, this, correctiveY);
            passwordDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);
            rightDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);

            rightDrawable.Command = new Command(PasswordCommand);

            rSControl.RightIcon = new RSEntryIcon();
        }
        private void CreateIcons()
        {
            this.CompoundDrawablePadding = 15;

            //Corrective Y
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                correctiveY = Math.Abs(topSpacing - bottomSpacing) / 2 + Math.Abs(PaddingTop - PaddingBottom) / 2;
            else
                correctiveY = 0;

            //Password
            if (this.rSControl.IsPassword)
                CreatePasswordIcon();

            //Icon Separator
            if (rSControl.HasRighIconSeparator || rSControl.HasLeftIconSeparator)
            {
                iconsSearator = new Paint();
                iconsSearator.AntiAlias = true;
                iconsSearator.Color = borderColor;
                iconsSearator.StrokeWidth = 2;
            }

            //Leading Icon
            if (this.rSControl.LeadingIcon != null)
            {
                this.leadingDrawable = CreateDrawable(rSControl.LeadingIcon, 0, null);
                leadingIconWidth = leadingDrawable.IntrinsicWidth + (this.CompoundDrawablePadding * 2);
                this.SetPadding(this.PaddingLeft + leadingIconWidth, this.PaddingTop, this.PaddingRight, this.PaddingBottom);
            }

            //Left Icon
            if (rSControl.LeftIcon != null)
            {
                if (rSControl.LeftHelpingIcon != null)
                    leftHelpingDrawable = CreateDrawable(rSControl.LeftHelpingIcon, 0, null);

                leftDrawableClip = leftHelpingDrawable != null ? leftHelpingDrawable.IntrinsicWidth + iconsSpacing : 0;

                this.leftDrawable = CreateDrawable(rSControl.LeftIcon, leftDrawableClip, "left");
            }

            //Right Icon
            if (this.rSControl.RightIcon != null && !rSControl.IsPassword)
            {
                //Custom Icon
                if (rSControl.RightHelpingIcon != null)
                    rightHelpingDrawable = CreateDrawable(rSControl.RightHelpingIcon, 0, null);

                rightDrawableClip = rightHelpingDrawable != null ? rightHelpingDrawable.IntrinsicWidth + iconsSpacing : 0;

                if(rSControl.RightIcon.View != null)
                    this.rightDrawable = CreateDrawable(rSControl.RightIcon, rightDrawableClip, "right");
            }

            //Trailing Icon
            if (rSControl.TrailingIcon != null)
            {
                this.trailingDrawable = CreateDrawable(rSControl.TrailingIcon, 0, null);
                trailingIconWidth = trailingDrawable.IntrinsicWidth + (this.CompoundDrawablePadding * 2);
                this.SetPadding(this.PaddingLeft, this.PaddingTop, this.PaddingRight + trailingIconWidth, this.PaddingBottom);
            }


            //Set Drawable to control
            this.SetCompoundDrawables(this.leftDrawable, null, this.rightDrawable, null);

            leftDrawableWidth = leftDrawable != null ? leftDrawable.IntrinsicWidth + this.CompoundDrawablePadding : 0;
        }
        private CustomDrawable CreateDrawable(RSEntryIcon rsIcon, int customDrawableClip, string type = null)
        {
            int width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)rsIcon.IconWidth, Context.Resources.DisplayMetrics);
            int height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)rsIcon.IconHeight, Context.Resources.DisplayMetrics);

            global::Android.Views.View convertedView = null;
            BitmapDrawable bitmapDrawable = null;

            if (rsIcon.View != null)
                convertedView = Extensions.ViewExtensions.ConvertFormsToNative(rsIcon.View, new Rectangle(0, 0, this.Width, this.Height), Context);

            if (convertedView != null)
                bitmapDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedView, width, height));


            if (type != null && type == "right")
                bitmapDrawable.SetBounds(customDrawableClip, 0, bitmapDrawable.IntrinsicWidth + customDrawableClip, bitmapDrawable.IntrinsicHeight);
            else if(type != null && type == "left")
                bitmapDrawable.SetBounds(0, 0, bitmapDrawable.IntrinsicWidth, bitmapDrawable.IntrinsicHeight);

            var drawable = new CustomDrawable(bitmapDrawable, this, correctiveY);
            drawable.SetBounds(0, 0, bitmapDrawable.IntrinsicWidth + customDrawableClip, bitmapDrawable.IntrinsicHeight);

            if(rsIcon.Source == null)
                rsIcon.Source = (rSControl as Forms.View).BindingContext;

            MethodInfo methodInfo;

            if(rsIcon.Bindings.Any())
            {
                List<object> objects = new List<object>();
                foreach (Binding binding in this.rSControl.RightIcon.Bindings)
                {
                    objects.Add(GetDeepPropertyValue(binding.Source, binding.Path));
                }

                Type[] types = new Type[this.rSControl.RightIcon.Bindings.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    types[i] = objects[i].GetType();
                }

                methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, types);
                drawable.Command = new Command<object>((x) => ExecuteCommand(methodInfo, rsIcon.Source, objects.ToArray()));
            }
            else if(rsIcon.CommandParameter != null)
            {
                methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, new Type[] { rsIcon.CommandParameter.GetType() });
                drawable.Command = new Command<object>((x) => ExecuteCommand(methodInfo, rsIcon.Source, rsIcon.CommandParameter));
            }
            else
            {
                if (rsIcon.Command != null)
                {
                    methodInfo = rsIcon.Source.GetType().GetMethod(rsIcon.Command, new Type[] { });
                    drawable.Command = new Command<object>((x) => ExecuteCommand(methodInfo, rsIcon.Source, rsIcon.CommandParameter));
                }
            }

            return drawable;
        }
        public static object GetDeepPropertyValue(object src, string propName)
        {
            if (propName.Contains('.'))
            {
                string[] Split = propName.Split('.');
                string RemainingProperty = propName.Substring(propName.IndexOf('.') + 1);
                return GetDeepPropertyValue(src.GetType().GetProperty(Split[0]).GetValue(src, null), RemainingProperty);
            }
            else
                return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        private void ExecuteCommand(MethodInfo methodInfo, object source, object[] parameters)
        {
            if(methodInfo != null)
            {
                if (parameters.Any())
                {
                    methodInfo.Invoke(source, parameters);
                }
                else
                    methodInfo.Invoke(source, null);
            }
        }
        private void ExecuteCommand(MethodInfo methodInfo, object source, object parameter)
        {
            if(methodInfo != null)
            {
                if (parameter != null)
                {
                    methodInfo.Invoke(source, new object[] { parameter });
                }
                else
                    methodInfo.Invoke(source, null);
            }
        }

        //Draw
        public override void Draw(Canvas canvas)
        {
            //Text rect bounds
            textRect = new Rect();
            this.GetDrawingRect(textRect);

            //Init floatingHint X and Y values
            if (!hasInitfloatingHintYPosition)
            {
                if(rSControl.IsPlaceholderAlwaysFloating)
                    IsFloating = true;
                else if (this.IsFocused || !string.IsNullOrEmpty(this.Text) || errorEnabled)
                    IsFloating = true;
                else
                    IsFloating = false;

                if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth + leftDrawableClip;
                    floatingHintXPostionNotFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth + leftDrawableClip;

                    //Y
                    floatingHintYPostionFloating = -this.Paint.Ascent() + topSpacing; 
                    floatingHintYPostionNotFloating = this.Baseline - Math.Abs(topSpacing - bottomSpacing) / 2 - Math.Abs(PaddingTop - PaddingBottom) / 2;
                }
                else if(this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + leftRightSpacingLabels + leadingIconWidth;
                    floatingHintXPostionNotFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth + leftDrawableClip;

                    //Y
                    floatingHintYPostionFloating = topSpacing + floatingHintBoundsFloating.Height() / 2;
                    floatingHintYPostionNotFloating = this.Baseline;
                }
                else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth + leftDrawableClip;
                    floatingHintXPostionNotFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth + leftDrawableClip;

                    //Y
                    floatingHintYPostionFloating = -this.Paint.Ascent(); /*topSpacing + floatingHintBoundsFloating.Height();*/
                    floatingHintYPostionNotFloating = this.Baseline;
                }


                if (IsFloating)
                {
                    floatingHintPaint.TextSize = labelsTextSize;
                    floatingHintXPostion = floatingHintXPostionFloating;
                    floatingHintYPostion = floatingHintYPostionFloating;
                }
                else
                {
                    floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                    floatingHintXPostion = floatingHintXPostionNotFloating;
                    floatingHintYPostion = floatingHintYPostionNotFloating;
                }


                if (errorPaint != null)
                    errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent() + 2 + textRect.Top;

                if (helperPaint != null)
                    helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent() + 2 + textRect.Top;

                if (counterPaint != null)
                    counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent() + 2 + textRect.Top;

                //Create Icons if any
                CreateIcons();


                hasInitfloatingHintYPosition = true;
            }

            //When EditText is focused Animate
            if (isFocused != this.IsFocused && !isFloatingHintAnimating)
            {
                if (this.IsFocused)
                {
                    //IsFloating = true;
                }
                else
                {
                    //if (string.IsNullOrEmpty(this.Text) && !errorEnabled)
                    //    IsFloating = false;
                    //else
                    //    IsFloating = true;
                }

                isFocused = this.IsFocused;
            }


            if (CanAnimate() && !isFloatingHintAnimating && !rSControl.IsPlaceholderAlwaysFloating)
                AnimateFloatingHint();


            //Update Colors
            SetColors();

            //Update Rounded border
            UpdateBorder(canvas);

            //Update Floating Hint
            UpdateFloatingHint(canvas);

            //Update Error Message
            if (errorEnabled)
                UpdateErrorMessage(canvas);

            //Update Helper Message
            if (!string.IsNullOrEmpty(this.rSControl.Helper))
                UpdateHelperMessage(canvas);

            //Update Counter Message
            if (this.rSControl.CounterMaxLength != -1)
                UpdateCounterMessage(canvas);

            //Update left helping drawable
            if(leftHelpingDrawable != null)
                UpdateLeftHelpingIcon(canvas);

            //Update right helping drawable
            if (rightHelpingDrawable != null)
                UpdateRightHelpingIcon(canvas);

            //Update Right Separator
            if (rSControl.HasRighIconSeparator && this.rightHelpingDrawable != null)
                UpdateRighIconSeparator(canvas);

            //Update Left Separator
            if (rSControl.HasLeftIconSeparator && this.leftHelpingDrawable != null)
                UpdateLeftIconSeparator(canvas);

            //Update leading drawable
            if (leadingDrawable != null)
                UpdateLeadingIcon(canvas);

            //Update trailing drawable
            if (trailingDrawable != null)
                UpdateTrailingIcon(canvas);
            

            base.OnDraw(canvas);
        }
        private void UpdateLeftHelpingIcon(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - leftHelpingDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + leftHelpingDrawable.IntrinsicHeight / 2;

            leftHelpingDrawable.drawable.SetBounds(textRect.Left + this.PaddingLeft + iconsSpacing + leftHelpingDrawable.IntrinsicWidth, center, textRect.Left + this.PaddingLeft + iconsSpacing + leftHelpingDrawable.IntrinsicWidth * 2, center2);
            leftHelpingDrawable.SetBounds(textRect.Left + this.PaddingLeft + iconsSpacing + leftHelpingDrawable.IntrinsicWidth, center, textRect.Left + this.PaddingLeft + iconsSpacing + leftHelpingDrawable.IntrinsicWidth * 2, center2);


            leftHelpingDrawable.Draw(canvas);
        }
        private void UpdateRightHelpingIcon(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - rightHelpingDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + rightHelpingDrawable.IntrinsicHeight / 2;

            rightHelpingDrawable.drawable.SetBounds(textRect.Right - this.PaddingRight - iconsSpacing - rightHelpingDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - iconsSpacing - rightHelpingDrawable.IntrinsicWidth, center2);
            rightHelpingDrawable.SetBounds(textRect.Right - this.PaddingRight - iconsSpacing - rightHelpingDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - iconsSpacing - rightHelpingDrawable.IntrinsicWidth, center2);


            rightHelpingDrawable.Draw(canvas);
        }
        private void UpdateLeftIconSeparator(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - leftDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + leftDrawable.IntrinsicHeight / 2;

            canvas.DrawLine(textRect.Left + this.PaddingLeft + leftDrawable.IntrinsicWidth + iconsSpacing / 2,
                            center - correctiveY,
                            textRect.Left + this.PaddingLeft + leftDrawable.IntrinsicWidth + iconsSpacing / 2,
                            center2 - correctiveY,
                            iconsSearator);
        }
        private void UpdateRighIconSeparator(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - rightDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + rightDrawable.IntrinsicHeight / 2;

            canvas.DrawLine(textRect.Right - this.PaddingRight - rightDrawable.IntrinsicWidth - iconsSpacing / 2,
                            center - correctiveY,
                            textRect.Right - this.PaddingRight - rightDrawable.IntrinsicWidth - iconsSpacing / 2,
                            center2 - correctiveY,
                            iconsSearator);
        }
        private void UpdateLeadingIcon(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - leadingDrawable.IntrinsicHeight / 2 + textRect.Top;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + leadingDrawable.IntrinsicHeight / 2 + textRect.Top;

            leadingDrawable.drawable.SetBounds(textRect.Left + this.CompoundDrawablePadding, center, textRect.Left + this.CompoundDrawablePadding + leadingDrawable.IntrinsicWidth, center2);
            leadingDrawable.SetBounds(textRect.Left + this.CompoundDrawablePadding, center, textRect.Left + this.CompoundDrawablePadding + this.PaddingLeft + this.PaddingLeft + leadingDrawable.IntrinsicWidth, center2);

            leadingDrawable.Draw(canvas);
        }
        private void UpdateTrailingIcon(Canvas canvas)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - trailingDrawable.IntrinsicHeight / 2 + textRect.Top;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + trailingDrawable.IntrinsicHeight / 2 + textRect.Top;

            trailingDrawable.drawable.SetBounds(textRect.Right - this.CompoundDrawablePadding - trailingDrawable.IntrinsicWidth, center, textRect.Right - this.CompoundDrawablePadding, center2);
            trailingDrawable.SetBounds(textRect.Right - this.CompoundDrawablePadding - trailingDrawable.IntrinsicWidth, center, textRect.Right - this.CompoundDrawablePadding + this.PaddingRight + this.PaddingRight, center2);

            trailingDrawable.Draw(canvas);
        }
        private void UpdateFloatingHint(Canvas canvas)
        {
            canvas.DrawText(floatingHintText, textRect.Left + floatingHintXPostion, floatingHintYPostion + textRect.Top, floatingHintPaint);
        }
        private void UpdateErrorMessage(Canvas canvas)
        {
            errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent() + 2 + textRect.Top;
            canvas.DrawText(errorMessage, textRect.Left + leftRightSpacingLabels + leadingIconWidth, errorYPosition, errorPaint);
        }
        private void UpdateHelperMessage(Canvas canvas)
        {
            helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent() + 2 + textRect.Top;
            canvas.DrawText(helperMessage, textRect.Left + leftRightSpacingLabels + leadingIconWidth, helperYPosition, helperPaint);
        }
        private void UpdateCounterMessage(Canvas canvas)
        {
            counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent() + 2 + textRect.Top;
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);
            canvas.DrawText(counterMessage, textRect.Right - trailingIconWidth - (counterMessageBounds.Width()) - leftRightSpacingLabels, counterYPosition, counterPaint);
        }
        private void UpdateBorder(Canvas canvas)
        {
            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                //Clip top border first and check if api < 26
                canvas.Save();

                if (IsFloating)
                {
                    if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                    {
                        canvas.ClipRect(textRect.Left + leftRightSpacingLabels - floatingHintClipPadding + leadingIconWidth,
                                        textRect.Top,
                                        textRect.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels + floatingHintClipPadding + leadingIconWidth,
                                        textRect.Top + topSpacing + floatingHintBoundsFloating.Height(),
                                        global::Android.Graphics.Region.Op.Difference);
                    }
                    else
                    {
                        canvas.ClipOutRect(textRect.Left + leftRightSpacingLabels - floatingHintClipPadding + leadingIconWidth,
                                           textRect.Top,
                                           textRect.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels + floatingHintClipPadding + leadingIconWidth,
                                           textRect.Top + topSpacing + floatingHintBoundsFloating.Height());
                    }
                }


                canvas.DrawRoundRect(new RectF(textRect.Left + leadingIconWidth + rSControl.ShadowRadius,
                                               textRect.Top + topSpacing,
                                               textRect.Right - trailingIconWidth - rSControl.ShadowRadius,
                                               textRect.Bottom - bottomSpacing),
                                               this.rSControl.BorderRadius, this.rSControl.BorderRadius,
                                               filledPaint);


                canvas.DrawRoundRect(new RectF(textRect.Left  + leadingIconWidth + rSControl.ShadowRadius,
                                               textRect.Top  + topSpacing,
                                               textRect.Right  - trailingIconWidth - rSControl.ShadowRadius,
                                               textRect.Bottom  - bottomSpacing),
                                               this.rSControl.BorderRadius, this.rSControl.BorderRadius,
                                               borderPaint);

                canvas.Restore();

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                canvas.DrawLine(textRect.Left + leadingIconWidth,
                                textRect.Bottom - bottomSpacing - corectCorners - (borderPaint.StrokeWidth / borderPaint.StrokeWidth),
                                textRect.Right - trailingIconWidth,
                                textRect.Bottom - bottomSpacing - corectCorners,
                                borderPaint);

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                //canvas.Save();

                //if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                //{
                //    canvas.ClipRect(textRect.Left + leadingIconWidth,
                //                    textRect.Bottom - bottomSpacing,
                //                    textRect.Right - trailingIconWidth,
                //                    textRect.Bottom,
                //                    global::Android.Graphics.Region.Op.Difference);
                //}
                //else
                //{
                //    canvas.ClipOutRect(textRect.Left + leadingIconWidth,
                //                       textRect.Bottom - bottomSpacing,
                //                       textRect.Right - trailingIconWidth,
                //                       textRect.Bottom - bottomSpacing + borderPaint.StrokeWidth);
                //}


                //canvas.DrawRoundRect(new RectF(textRect.Left + leadingIconWidth + rSControl.ShadowRadius,
                //                               textRect.Top + topSpacing,
                //                               textRect.Right - trailingIconWidth - rSControl.ShadowRadius,
                //                               textRect.Bottom - bottomSpacing + borderPaint.StrokeWidth),
                //                               this.rSControl.BorderRadius, this.rSControl.BorderRadius,
                //                               filledPaint);

                //canvas.Restore();

                //canvas.DrawLine(textRect.Left  + leadingIconWidth + rSControl.ShadowRadius,
                //                textRect.Bottom - bottomSpacing - corectCorners - (borderPaint.StrokeWidth / borderPaint.StrokeWidth),
                //                textRect.Right - trailingIconWidth - corectCorners - rSControl.ShadowRadius,
                //                textRect.Bottom - bottomSpacing,
                //                borderPaint);

                var path = RoundedRect(7, topSpacing, this.Width - 7, this.Height - bottomSpacing, rSControl.BorderRadius, rSControl.BorderRadius, true);
                Path path2 = new Path();
                path2.MoveTo(7, this.Height - bottomSpacing - borderWidth / 2);
                path2.LineTo(this.Width - 7, this.Height - bottomSpacing - borderWidth / 2);
                path2.Close();//Given close, last lineto can be removed.

                canvas.DrawPath(path, filledPaint);
                canvas.DrawPath(path2, borderPaint);
            }
        }
        public Path RoundedRect(float left, float top, float right, float bottom, float rx, float ry, bool conformToOriginalPost)
        {
            Path path = new Path();
            if (rx < 0) rx = 0;
            if (ry < 0) ry = 0;
            float width = right - left;
            float height = bottom - top;
            if (rx > width / 2) rx = width / 2;
            if (ry > height / 2) ry = height / 2;
            float widthMinusCorners = (width - (2 * rx));
            float heightMinusCorners = (height - (2 * ry));

            path.MoveTo(right, top + ry);
            path.RQuadTo(0, -ry, -rx, -ry);//top-right corner
            path.RLineTo(-widthMinusCorners, 0);
            path.RQuadTo(-rx, 0, -rx, ry); //top-left corner
            path.RLineTo(0, heightMinusCorners);

            if (conformToOriginalPost)
            {
                path.RLineTo(0, ry);
                path.RLineTo(width, 0);
                path.RLineTo(0, -ry);
            }
            else
            {
                path.RQuadTo(0, ry, rx, ry);//bottom-left corner
                path.RLineTo(widthMinusCorners, 0);
                path.RQuadTo(rx, 0, rx, -ry); //bottom-right corner
            }

            path.RLineTo(0, -heightMinusCorners);

            path.Close();//Given close, last lineto can be removed.

            return path;
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
            shouldNotFloat = false;
            shouldFloat = false;
        }
        private void FloatingHintAnimator_Update(object sender, AnimatorUpdateEventArgs e)
        {
            floatingHintPaint.TextSize = (float)e.Animation.GetAnimatedValue("textSize");
            floatingHintXPostion = (float)e.Animation.GetAnimatedValue("XPosition");
            floatingHintYPostion = (float)e.Animation.GetAnimatedValue("YPosition");
            Invalidate();
        }
        public bool AnimateFloatingHint()
        {
            isFloatingHintAnimating = true;
            float textSizeStart;
            float textSizeEnd;
            float xStart;
            float xEnd;
            float yStart;
            float yEnd;

            if (shouldNotFloat)
            {
                textSizeStart = labelsTextSize;
                textSizeEnd = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                xStart = floatingHintXPostionFloating;
                xEnd = floatingHintXPostionNotFloating;
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionNotFloating;

                IsFloating = false;
            }
            else if (shouldFloat)
            {
                textSizeStart = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                textSizeEnd = labelsTextSize;
                xStart = floatingHintXPostionNotFloating;
                xEnd = floatingHintXPostionFloating;
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionFloating;

                IsFloating = true;
            }
            else
                return false;

            PropertyValuesHolder propertyTextSizeError = PropertyValuesHolder.OfFloat("textSize", textSizeStart, textSizeEnd);
            PropertyValuesHolder propertyXPositionError = PropertyValuesHolder.OfFloat("XPosition", xStart, xEnd);
            PropertyValuesHolder propertyYPositionError = PropertyValuesHolder.OfFloat("YPosition", yStart, yEnd);
            floatingHintAnimator.SetValues(propertyTextSizeError, propertyXPositionError, propertyYPositionError);


            //Start animation
            floatingHintAnimator.Start();

            return true;
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

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool rightIconCondition = this.rightDrawable != null && e.GetX() >= Right - trailingIconWidth - rightDrawable.IntrinsicWidth - iconsSpacing && e.GetX() <= Right - trailingIconWidth;
            bool trailingIconCondition = trailingDrawable != null && e.GetX() >= Right - trailingIconWidth && e.GetX() <= Right;
            bool rightHelpingIconCondition = rightHelpingDrawable != null && e.GetX() >= rightHelpingDrawable.Bounds.Left - textRect.Left && e.GetX() <= rightHelpingDrawable.Bounds.Right - textRect.Left + iconsSpacing;
            bool leadingIconCondition = leadingDrawable != null && e.GetX() >= Left && e.GetX() <= Left + leadingIconWidth;
            bool leftIconCondition = leftDrawable != null && e.GetX() >= Left + leadingIconWidth && e.GetX() <= Left + leadingIconWidth + leftDrawable.IntrinsicWidth + iconsSpacing;
            bool leftHelpingIconCondition = leftHelpingDrawable != null && e.GetX() >= leftHelpingDrawable.Bounds.Left - textRect.Left - iconsSpacing && e.GetX() <= leftHelpingDrawable.Bounds.Right - textRect.Left;

            if (e.Action == MotionEventActions.Down)
            {
                if (rightIconCondition)
                {
                    return true;
                }
                else if (trailingIconCondition)
                {
                    return true;
                }
                else if (rightHelpingIconCondition)
                {
                    return true;
                }
                else if (leadingIconCondition)
                {
                    return true;
                }
                else if (leftIconCondition)
                {
                    return true;
                }
                else if (leftHelpingIconCondition)
                {
                    return true;
                }
                else
                    return base.OnTouchEvent(e);
            }
            if (e.Action == MotionEventActions.Up)
            {
                if (rightIconCondition)
                {
                    if (this.rSControl.IsPassword)
                    {
                        if (rightDrawable.Selected)
                            rightDrawable.Selected = false;
                        else
                            rightDrawable.Selected = true;
                    }
                    else
                    {
                        if ((rightDrawable as CustomDrawable).Selected)
                        {
                            (rightDrawable as CustomDrawable).Selected = false;
                        }
                        else
                        {
                            (rightDrawable as CustomDrawable).Selected = true;
                        }
                    }
                }
                else if (trailingIconCondition)
                {
                    if ((trailingDrawable as CustomDrawable).Selected)
                    {
                        (trailingDrawable as CustomDrawable).Selected = false;
                    }
                    else
                    {
                        (trailingDrawable as CustomDrawable).Selected = true;
                    }
                }
                else if (rightHelpingIconCondition)
                {
                    if (rightHelpingDrawable.Selected)
                    {
                        rightHelpingDrawable.Selected = false;
                    }
                    else
                    {
                        rightHelpingDrawable.Selected = true;
                    }
                }
                else if (leadingIconCondition)
                {
                    if ((leadingDrawable as CustomDrawable).Selected)
                    {
                        (leadingDrawable as CustomDrawable).Selected = false;
                    }
                    else
                    {
                        (leadingDrawable as CustomDrawable).Selected = true;
                    }
                }
                else if (leftIconCondition)
                {
                    if ((leftDrawable as CustomDrawable).Selected)
                    {
                        (leftDrawable as CustomDrawable).Selected = false;
                    }
                    else
                    {
                        (leftDrawable as CustomDrawable).Selected = true;
                    }
                }
                else if (leftHelpingIconCondition)
                {
                    if (leftHelpingDrawable.Selected)
                    {
                        leftHelpingDrawable.Selected = false;
                    }
                    else
                    {
                        leftHelpingDrawable.Selected = true;
                    }
                }
                else
                    return base.OnTouchEvent(e);

                this.SetSelection(this.Length());
                Invalidate();
                return false;
            }
            else
                return base.OnTouchEvent(e);
        }

        //On touch listener
        //public bool OnTouch(global::Android.Views.View v, MotionEvent e)
        //{

        //}

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


        //Dispose
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
        private float y;

        //Constructor
        public CustomDrawable(Drawable drawable, CustomEditText editText, float y) : base()
        {
            this.drawable = drawable;
            this.editText = editText;
            this.selected = false;
            this.y = y;
            clicked = false;

            CreateRippleEffect();
        }


        //Draw
        public override void Draw(Canvas canvas)
        {
            //Icon click effect
            if (ripple)
                DrawFingerPrint(canvas, y);


            //align to top
            canvas.Save();
            canvas.Translate(0, -y);
            this.drawable.Draw(canvas);
            canvas.Restore();
        }


        public override int Opacity => this.Opacity;
        public override void SetAlpha(int alpha)
        {
            this.SetAlpha(alpha);
        }
        public override void SetColorFilter(ColorFilter colorFilter)
        {
            this.SetColorFilter(colorFilter);
        }
        public override int IntrinsicHeight => this.drawable.IntrinsicHeight;
        public override int IntrinsicWidth => this.drawable.IntrinsicWidth;


        //Selected
        private static int[] STATE_SELECTED;
        private bool selected;
        public bool Selected
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

                if(this.Command != null)
                    this.Command.Execute(null);
            }
        }


        //Clicked
        private bool clicked;
        public bool Clicked
        {
            get
            {
                return clicked;
            }
            set
            {
                clicked = value;
            }
        }


        //Ripple effect
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
            }
            else
            {
                if (ripplePaint.Alpha > 0)
                {
                    ripplePaint.Alpha -= 15;
                }
                else
                {
                    ripplePaint.Alpha = 0;
                    radius = 0f;
                    ripple = false;
                }
            }

            //Force redraw call
            this.editText.PostInvalidate();
        }


        //Command
        public System.Windows.Input.ICommand Command { get; set; }
    }
}