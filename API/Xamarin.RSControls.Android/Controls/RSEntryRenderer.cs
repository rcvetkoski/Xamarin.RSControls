using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Controls;
using System.ComponentModel;
using Android.Graphics;
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

            //Element.Placeholder = "";
            Element.PlaceholderColor = Xamarin.Forms.Color.Transparent;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var control = (this.Control as CustomEditText);

            if (e.PropertyName == "Error" && this.Control is CustomEditText && !isTextInputLayout)
                control.ErrorMessage = (this.Element as RSEntry).Error;
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
        private float textSpacingFromBorderTop;
        private float textSpacingFromBorderBottom;
        private int leftRightSpacingLabels;
        private float borderWidth;
        private float borderWidthFocused;
        private float borderRadius;
        private float shadowRadius;
        private int labelsTextSize;
        private int floatingHintClipPadding;
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
        private global::Android.Graphics.Rect floatingHintBoundsFloating;
        private global::Android.Graphics.Rect floatingHintBoundsNotFloating;
        private global::Android.Graphics.Rect counterMessageBounds;
        public Paint borderPaint;
        private Paint filledPaint;
        private global::Android.Graphics.Color borderColor;
        private global::Android.Graphics.Color activeColor;
        private global::Android.Graphics.Color errorColor;
        private bool isFloatingHintAnimating = false;
        private float floatingHintXPostion;
        private float floatingHintXPostionFloating;
        private float floatingHintXPostionNotFloating;
        private float floatingHintYPostion;
        private float floatingHintYPositionFloating;
        private float floatingHintYPostionNotFloating;
        private bool hasInitfloatingHintYPosition = false;
        private float errorYPosition;
        private float helperYPosition;
        private float counterYPosition;
        private global::Android.Graphics.Rect textRect;
        private int requiredWidth;
        private double maxIconHeight = 0;

        //icon drawables
        private CustomDrawable leadingDrawable;
        private CustomDrawable rightHelpingDrawable;
        private CustomDrawable leftHelpingDrawable;
        private CustomDrawable trailingDrawable;
        private CustomDrawable leftDrawable;
        private CustomDrawable rightDrawable;
        private Paint iconsSearator;
        private int iconPadding;
        private int leftDrawableWidth;
        private int leadingDrawableWidth;
        private int trailingDrawableWidth;
        private int leftHelpingDrawableWidth;
        private int rightDrawableWidth;
        private int rightHelpingDrawableWidth;
        private global::Android.Graphics.Rect borderPosition;
        private int leftHelpingIconPadding;
        private int rightHelpingIconPadding;


        private bool shouldFloat;
        private bool shouldNotFloat;
        public bool IsFloating;

        //Padding
        public Thickness padding;

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
                    AnimateErrorHelperMessage(0, 255, errorYPosition + textSpacingFromBorderBottom, this.Height - textSpacingFromBorderBottom - errorPaint.Ascent());
                }
                else
                {
                    errorEnabled = false;
                    AnimateErrorHelperMessage(255, 0, errorYPosition + textSpacingFromBorderBottom, this.Height - textSpacingFromBorderBottom - errorPaint.Ascent() + (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Context.Resources.DisplayMetrics));
                }
            }
        }


        //Constructor
        public CustomEditText(Context context, IRSControl rSControl) : base(context)
        {
            //Init values
            this.rSControl = rSControl;
            isFocused = this.IsFocused;
            labelsTextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 12, Context.Resources.DisplayMetrics);
            borderWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.BorderWidth, Context.Resources.DisplayMetrics);
            borderWidthFocused = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.BorderWidth + 1, Context.Resources.DisplayMetrics);
            borderRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.BorderRadius, Context.Resources.DisplayMetrics);
            floatingHintClipPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Context.Resources.DisplayMetrics);
            shadowRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, rSControl.ShadowEnabled ? rSControl.ShadowRadius : borderWidth, Context.Resources.DisplayMetrics);
            leftRightSpacingLabels = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 14, Context.Resources.DisplayMetrics);
            iconPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics);
            errorMessage = string.Empty;
            borderColor = rSControl.BorderColor.ToAndroid();
            activeColor = rSControl.ActiveColor.ToAndroid();
            errorColor = rSControl.ErrorColor.ToAndroid();
            textSpacingFromBorderTop = TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics);
            textSpacingFromBorderBottom = TypedValue.ApplyDimension(ComplexUnitType.Dip, 14, Context.Resources.DisplayMetrics);
            borderPosition = new global::Android.Graphics.Rect();
            leftHelpingIconPadding = rSControl.LeftHelpingIcon != null ? (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics) : 0;
            rightHelpingIconPadding = rSControl.RightHelpingIcon != null ? (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics) : 0;

            //Set Padding
            padding = new Thickness(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Context.Resources.DisplayMetrics), TypedValue.ApplyDimension(ComplexUnitType.Dip, 17, Context.Resources.DisplayMetrics));
            SetPaddingValues();

            //Create Icons if any
            CreateIcons();

            //Floating Hint
            CreateFloatingHint();

            //Error Message
            CreateErrorMessage();

            //Helper Message
            CreateHelperMessage();

            //Create Counter Message
            CreateCounterMessage();

            // Create border
            CreateBorder();

            if (this.rSControl.CounterMaxLength != -1)
                this.AfterTextChanged += CustomEditText_AfterTextChanged;

            //Create errorMessage animator TODO check if error seted
            CreateErrorHelperMessageAnimator();
            CreateFloatingHintAnimator();

            //Is Placeholder Always Floating
            if (rSControl.IsPlaceholderAlwaysFloating)
                IsFloating = true;
            else if (this.IsFocused || !string.IsNullOrEmpty(this.Text) || errorEnabled)
                IsFloating = true;
            else
                IsFloating = false;


            //Calculate required width and height of view
            //Used in adjust Size if needed
            AdjustSize();
        }

        //Adjust Size if needed
        private void AdjustSize()
        {
            //Width
            int errorLabelWidth = 0;
            int helperLabelWidth = (int)helperPaint.MeasureText(helperMessage);
            int counterLabelWidth = rSControl.CounterMaxLength != -1 ? (int)counterPaint.MeasureText(counterMessage) + leftRightSpacingLabels : 0;
            int labelsWidth;

            foreach (var behavior in (rSControl as Forms.View).Behaviors)
            {
                if (behavior is Validators.ValidationBehaviour)
                {
                    foreach (var iValidator in (behavior as Validators.ValidationBehaviour).Validators)
                    {
                        if (errorLabelWidth < errorPaint.MeasureText(iValidator.Message))
                            errorLabelWidth = (int)errorPaint.MeasureText(iValidator.Message);
                    }
                }
            }


            if(helperLabelWidth > errorLabelWidth)
                labelsWidth = helperLabelWidth + counterLabelWidth + leadingDrawableWidth + trailingDrawableWidth + leftRightSpacingLabels * 2;
            else 
                labelsWidth = errorLabelWidth + counterLabelWidth + leadingDrawableWidth + trailingDrawableWidth + leftRightSpacingLabels * 2;

            
            if (labelsWidth > floatingHintBoundsNotFloating.Width() + PaddingLeft + PaddingRight)
                requiredWidth = labelsWidth;
            else
                requiredWidth = floatingHintBoundsNotFloating.Width() + PaddingLeft + PaddingRight;


            SetMinimumWidth(requiredWidth);


            //Height
            var h = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)maxIconHeight, Context.Resources.DisplayMetrics) + textSpacingFromBorderTop + textSpacingFromBorderBottom + iconPadding * 2;
            SetMinimumHeight((int)h);
        }

        public void SetPaddingValues()
        {
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                this.SetPadding((int)padding.Left + (int)this.rSControl.Padding.Left,
                                (int)padding.Top + (int)this.rSControl.Padding.Top,
                                (int)padding.Right +  (int)this.rSControl.Padding.Right,
                                (int)padding.Bottom + (int)(textSpacingFromBorderBottom - textSpacingFromBorderTop) + (int)this.rSControl.Padding.Bottom);

            }
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                this.SetPadding((int)padding.Left + (int)this.rSControl.Padding.Left,
                                (int)padding.Top + (int)this.rSControl.Padding.Top,
                                (int)padding.Right + (int)this.rSControl.Padding.Right,
                                (int)padding.Bottom + (int)this.rSControl.Padding.Bottom);

            }
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                this.SetPadding((int)padding.Left + (int)this.rSControl.Padding.Left,
                                (int)padding.Top + (int)this.rSControl.Padding.Top,
                                (int)padding.Right + (int)this.rSControl.Padding.Right,
                                (int)padding.Bottom + (int)this.rSControl.Padding.Bottom);

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

                    //if (rightDrawable != null && rSControl.RightIcon.IconColor == Forms.Color.DimGray)
                    //    rightDrawable.drawable.SetTint(borderColor);
                    //else if(rightDrawable != null)
                    //    rightDrawable.drawable.SetTint(rSControl.RightIcon.IconColor.ToAndroid());

                    //if (leftDrawable != null && rSControl.LeftIcon.IconColor == Forms.Color.DimGray)
                    //    leftDrawable.drawable.SetTint(borderColor);
                    //else if (leftDrawable != null)
                    //    leftDrawable.drawable.SetTint(rSControl.LeftIcon.IconColor.ToAndroid());

                    //if (leadingDrawable != null && rSControl.LeadingIcon.IconColor == Forms.Color.DimGray)
                    //    leadingDrawable.drawable.SetTint(borderColor);
                    //else if (leadingDrawable != null)
                    //    leadingDrawable.drawable.SetTint(rSControl.LeadingIcon.IconColor.ToAndroid());

                    //if (trailingDrawable != null && rSControl.TrailingIcon.IconColor == Forms.Color.DimGray)
                    //    trailingDrawable.drawable.SetTint(borderColor);
                    //else if (trailingDrawable != null)
                    //    trailingDrawable.drawable.SetTint(rSControl.TrailingIcon.IconColor.ToAndroid());

                    //if (rightHelpingDrawable != null && rSControl.RightHelpingIcon.IconColor == Forms.Color.DimGray)
                    //    rightHelpingDrawable.drawable.SetTint(borderColor);
                    //else if (rightHelpingDrawable != null)
                    //    rightHelpingDrawable.drawable.SetTint(rSControl.RightHelpingIcon.IconColor.ToAndroid());

                    //if (leftHelpingDrawable != null && rSControl.LeftHelpingIcon.IconColor == Forms.Color.DimGray)
                    //    leftHelpingDrawable.drawable.SetTint(borderColor);
                    //else if (leftHelpingDrawable != null)
                    //    leftHelpingDrawable.drawable.SetTint(rSControl.LeftHelpingIcon.IconColor.ToAndroid());
                }
                else
                {
                    borderPaint.Color = activeColor;
                    floatingHintPaint.Color = activeColor;

                    //if (rightDrawable != null)
                    //    rightDrawable.drawable.SetTint(activeColor);

                    //if (leftDrawable != null)
                    //    leftDrawable.drawable.SetTint(activeColor);

                    //if (leadingDrawable != null)
                    //    leadingDrawable.drawable.SetTint(activeColor);

                    //if (trailingDrawable != null)
                    //    trailingDrawable.drawable.SetTint(activeColor);

                    //if (rightHelpingDrawable != null)
                    //    rightHelpingDrawable.drawable.SetTint(activeColor);

                    //if (leftHelpingDrawable != null)
                    //    leftHelpingDrawable.drawable.SetTint(activeColor);
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
                    //floatingHintPaint.Color = rSControl.PlaceholderColor.ToAndroid();
                    floatingHintPaint.Color = Forms.Color.DimGray.ToAndroid();


                    //if (rightDrawable != null && rSControl.RightIcon.IconColor == Forms.Color.DimGray)
                    //    rightDrawable.drawable.SetTint(borderColor);
                    //else if (rightDrawable != null)
                    //    rightDrawable.drawable.SetTint(rSControl.RightIcon.IconColor.ToAndroid());

                    //if (leftDrawable != null && rSControl.LeftIcon.IconColor == Forms.Color.DimGray)
                    //    leftDrawable.drawable.SetTint(borderColor);
                    //else if (leftDrawable != null)
                    //    leftDrawable.drawable.SetTint(rSControl.LeftIcon.IconColor.ToAndroid());

                    //if (leadingDrawable != null && rSControl.LeadingIcon.IconColor == Forms.Color.DimGray)
                    //    leadingDrawable.drawable.SetTint(borderColor);
                    //else if (leadingDrawable != null)
                    //    leadingDrawable.drawable.SetTint(rSControl.LeadingIcon.IconColor.ToAndroid());

                    //if (trailingDrawable != null && rSControl.TrailingIcon.IconColor == Forms.Color.DimGray)
                    //    trailingDrawable.drawable.SetTint(borderColor);
                    //else if (trailingDrawable != null)
                    //    trailingDrawable.drawable.SetTint(rSControl.TrailingIcon.IconColor.ToAndroid());

                    //if (rightHelpingDrawable != null && rSControl.RightHelpingIcon.IconColor == Forms.Color.DimGray)
                    //    rightHelpingDrawable.drawable.SetTint(borderColor);
                    //else if (rightHelpingDrawable != null)
                    //    rightHelpingDrawable.drawable.SetTint(rSControl.RightHelpingIcon.IconColor.ToAndroid());

                    //if (leftHelpingDrawable != null && rSControl.LeftHelpingIcon.IconColor == Forms.Color.DimGray)
                    //    leftHelpingDrawable.drawable.SetTint(borderColor);
                    //else if (leftHelpingDrawable != null)
                    //    leftHelpingDrawable.drawable.SetTint(rSControl.LeftHelpingIcon.IconColor.ToAndroid());
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
            floatingHintBoundsFloating = new global::Android.Graphics.Rect();
            floatingHintBoundsNotFloating = new global::Android.Graphics.Rect();

            if (rSControl.PlaceholderStyle.FontFamily != null && rSControl.PlaceholderStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.PlaceholderStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.PlaceholderStyle.FontFamily.Substring(0, index);
                floatingHintPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                floatingHintPaint.SetTypeface(Typeface.Create(rSControl.PlaceholderStyle.FontFamily, TypefaceStyle.Normal));

            floatingHintText = this.rSControl.Placeholder != null ? rSControl.Placeholder : "";
            //global::Android.Graphics.Color color = new global::Android.Graphics.Color(this.CurrentHintTextColor);
            floatingHintPaint.Color = rSControl.PlaceholderStyle.FontColor.ToAndroid();
            floatingHintPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            floatingHintPaint.AntiAlias = true;


            //Set width and height of floating hint paint when floating
            floatingHintPaint.TextSize = labelsTextSize;
            floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBoundsFloating);

            //Set width and height of floating hint paint when not floating
            floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);

            if(!string.IsNullOrEmpty(floatingHintText))
                floatingHintPaint.GetTextBounds(floatingHintText, 0, floatingHintText.Length, floatingHintBoundsNotFloating);
            else
                floatingHintPaint.GetTextBounds("Hint", 0, 4, floatingHintBoundsNotFloating); //Just to get the floatingHintBoundsNotFloating Height, hint won't be apear
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
                errorPaint.SetTypeface(Typeface.Create(rSControl.ErrorStyle.FontFamily, TypefaceStyle.Normal));

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
                helperPaint.SetTypeface(Typeface.Create(rSControl.HelperStyle.FontFamily, TypefaceStyle.Normal));

            helperPaint.Color = rSControl.HelperStyle.FontColor.ToAndroid();
            helperPaint.TextSize = labelsTextSize;
            helperPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            helperPaint.AntiAlias = true;
        }
        private void CreateCounterMessage()
        {
            counterPaint = new TextPaint();
            counterMessageBounds = new global::Android.Graphics.Rect();
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);


            if (rSControl.CounterStyle.FontFamily != null && rSControl.CounterStyle.FontFamily.Contains("#"))
            {
                var index = rSControl.CounterStyle.FontFamily.IndexOf("#");
                var fontFamily = rSControl.CounterStyle.FontFamily.Substring(0, index);
                counterPaint.SetTypeface(Typeface.CreateFromAsset(Context.Assets, fontFamily));
            }
            else
                counterPaint.SetTypeface(Typeface.Create(rSControl.CounterStyle.FontFamily, TypefaceStyle.Normal));

            counterPaint.Color = rSControl.CounterStyle.FontColor.ToAndroid();
            counterPaint.TextSize = labelsTextSize;
            counterPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            counterPaint.AntiAlias = true;

            //Set bounds to get width
            counterPaint.GetTextBounds(counterMessage, 0, counterMessage.Length, counterMessageBounds);

            //Set validator behaviour
            if(rSControl.CounterMaxLength != -1)
            {
                bool existingBehaviour = false;

                foreach(var behavior in (rSControl as Forms.View).Behaviors)
                {
                    if(behavior is Validators.ValidationBehaviour)
                    {
                        if((behavior as Validators.ValidationBehaviour).PropertyName == "Text")
                        {
                            (behavior as Validators.ValidationBehaviour).Validators.Add(new Validators.CounterValidation() { CounterMaxLength = rSControl.CounterMaxLength });
                            existingBehaviour = true;
                        }
                    }
                }

                if(!existingBehaviour)
                {
                    Validators.ValidationBehaviour counterValidationBehaviour = new Validators.ValidationBehaviour() { PropertyName = "Text" };
                    counterValidationBehaviour.Validators.Add(new Validators.CounterValidation() { CounterMaxLength = rSControl.CounterMaxLength });
                    (rSControl as Forms.View).Behaviors.Add(counterValidationBehaviour);
                }
            }
        }
        public void SetBorderColor()
        {
            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                filledPaint.Color = rSControl.BorderFillColor == Forms.Color.Default ? global::Android.Graphics.Color.Transparent : rSControl.BorderFillColor.ToAndroid();
            else
                filledPaint.Color = rSControl.BorderFillColor == Forms.Color.Default ? Forms.Color.FromHex("#OA000000").ToAndroid() : rSControl.BorderFillColor.ToAndroid();
        }
        private void CreateBorder()
        {
            borderPaint = new Paint();
            borderPaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
            borderPaint.Color = borderColor;
            borderPaint.StrokeWidth = borderWidth;
            borderPaint.AntiAlias = true;

            //Filled
            filledPaint = new Paint();
            filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            SetBorderColor();
            filledPaint.AntiAlias = true; 
            if (rSControl.ShadowEnabled)
                filledPaint.SetShadowLayer(this.shadowRadius, 0, this.borderWidth, rSControl.ShadowColor.ToAndroid());

            if (!this.IsHardwareAccelerated)
                this.SetLayerType(LayerType.Software, null);
            else
                this.SetLayerType(LayerType.Hardware, null);
        }
        private void CreatePasswordIcon()
        {
            var passwordDrawable = Context.GetDrawable(RSControls.Droid.Resource.Drawable.custom_design_password_eye);
            passwordDrawable.SetTint(global::Android.Graphics.Color.DarkGray);
            rightDrawable = new CustomDrawable(passwordDrawable, this);
            passwordDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);
            rightDrawable.SetBounds(0, 0, passwordDrawable.IntrinsicWidth, passwordDrawable.IntrinsicHeight);
            
            rightDrawable.Command = new Command(PasswordCommand);

            rSControl.RightIcon = new RSEntryIcon();
        }
        private void CreateIcons()
        {
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
                rSControl.LeadingIcon.View.Height.ToString();
                this.leadingDrawable = CreateDrawable(rSControl.LeadingIcon);

                leadingDrawableWidth = leadingDrawable.IntrinsicWidth + iconPadding;

                //Set max height of icon so we can resize RSEntry if necessary
                if (maxIconHeight < rSControl.LeadingIcon.View.Height)
                    maxIconHeight = rSControl.LeadingIcon.View.Height;
            }
            else
            {
                leadingDrawable = null;
            }

            //Left Icon
            if (rSControl.LeftIcon != null)
            {
                if (rSControl.LeftHelpingIcon != null)
                {
                    leftHelpingDrawable = CreateDrawable(rSControl.LeftHelpingIcon);

                    //Set max height of icon so we can resize RSEntry if necessary
                    if (maxIconHeight < rSControl.LeftHelpingIcon.View.Height)
                        maxIconHeight = rSControl.LeftHelpingIcon.View.Height;
                }

                leftHelpingDrawableWidth = leftHelpingDrawable != null ? leftHelpingDrawable.IntrinsicWidth + iconPadding : 0;

                this.leftDrawable = CreateDrawable(rSControl.LeftIcon);
                leftDrawableWidth = leftDrawable != null ? leftDrawable.IntrinsicWidth + iconPadding : 0;

                //Set max height of icon so we can resize RSEntry if necessary
                if (maxIconHeight < rSControl.LeftIcon.View.Height)
                    maxIconHeight = rSControl.LeftIcon.View.Height;

            }
            else
            {
                leftDrawable = null;
                leftHelpingDrawable = null;
            }

            //Right Icon
            if (this.rSControl.RightIcon != null && !rSControl.IsPassword)
            {
                //Custom Icon
                if (rSControl.RightHelpingIcon != null)
                {
                    rightHelpingDrawable = CreateDrawable(rSControl.RightHelpingIcon);

                    //Set max height of icon so we can resize RSEntry if necessary
                    if (maxIconHeight < rSControl.RightHelpingIcon.View.Height)
                        maxIconHeight = rSControl.RightHelpingIcon.View.Height;
                }

                rightHelpingDrawableWidth = rightHelpingDrawable != null ? rightHelpingDrawable.IntrinsicWidth + iconPadding : 0;

                if (rSControl.RightIcon.View != null)
                {
                    this.rightDrawable = CreateDrawable(rSControl.RightIcon);
                    rightDrawableWidth = rightDrawable != null ? rightDrawable.IntrinsicWidth + iconPadding : 0;

                    //Set max height of icon so we can resize RSEntry if necessary
                    if (maxIconHeight < rSControl.RightIcon.View.Height)
                        maxIconHeight = rSControl.RightIcon.View.Height;
                }
            }
            else
            {
                rightDrawable = null;
                rightHelpingDrawable = null;
            }

            //Trailing Icon
            if (rSControl.TrailingIcon != null)
            {
                this.trailingDrawable = CreateDrawable(rSControl.TrailingIcon);
                trailingDrawableWidth = trailingDrawable.IntrinsicWidth + iconPadding;

                //Set max height of icon so we can resize RSEntry if necessary
                if (maxIconHeight < rSControl.TrailingIcon.View.Height)
                    maxIconHeight = rSControl.TrailingIcon.View.Height;
            }
            else
            {
                trailingDrawable = null;
            }

            this.SetPadding(this.PaddingLeft + leadingDrawableWidth + leftDrawableWidth + leftHelpingIconPadding + leftHelpingDrawableWidth,
                            this.PaddingTop,
                            this.PaddingRight + trailingDrawableWidth + rightDrawableWidth + rightHelpingIconPadding + rightHelpingDrawableWidth,
                            this.PaddingBottom);

            //Set Drawable to control
            //this.SetCompoundDrawables(this.leftDrawable, null, this.rightDrawable, null);
        }
        private CustomDrawable CreateDrawable(RSEntryIcon rsIcon)
        {
            global::Android.Views.View convertedView = null;
            BitmapDrawable bitmapDrawable = null;

            if (rsIcon.View != null)
                convertedView = Extensions.ViewExtensions.ConvertFormsToNative(rsIcon.View, new Rectangle(0, 0, this.Width, this.Height), Context);

            if (convertedView != null)
                bitmapDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedView, convertedView.Width, convertedView.Height));

            var drawable = new CustomDrawable(bitmapDrawable, this);

            if(rsIcon.Source == null)
                rsIcon.Source = (rSControl as Forms.View).BindingContext;

            MethodInfo methodInfo;

            if(rsIcon.Bindings.Any())
            {
                List<object> objects = new List<object>();
                foreach (RSCommandParameter rSCommandParameter in rsIcon.Bindings)
                {
                    //objects.Add(GetDeepPropertyValue(binding.Source, binding.Path));
                    objects.Add(rSCommandParameter.CommandParameter);
                }

                Type[] types = new Type[rsIcon.Bindings.Count];
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
            textRect = new global::Android.Graphics.Rect();
            this.GetDrawingRect(textRect);


            //When EditText is focused Animate
            if (isFocused != this.IsFocused && !isFloatingHintAnimating)
                isFocused = this.IsFocused;


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

            //Update Icons
            UpdateIconsPosition(canvas);

            base.OnDraw(canvas);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            if (changed)
            {
                //if (!hasInitfloatingHintYPosition)
                //{
                //    hasInitfloatingHintYPosition = true;
                //}
                floatingHintPositionUpdate();

            }
        }

        //Update Icons
        private int GetTopIconPosition(CustomDrawable icon)
        {
            int top;

            if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                top = Baseline - icon.IntrinsicHeight / 2 - (int)textSpacingFromBorderBottom / 2;
            else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
                top = this.Height / 2 - (int)(textSpacingFromBorderBottom - textSpacingFromBorderTop) / 2 - icon.IntrinsicWidth / 2;
            else
                top = this.Height / 2 - (int)textSpacingFromBorderBottom / 2 - icon.IntrinsicWidth / 2;

            return top;
        }
        private void UpdateIconsPosition(Canvas canvas)
        {
            //Update left drawable
            if (leftDrawable != null)
                UpdateLeftIcon(canvas);

            //Update left helping drawable
            if (leftHelpingDrawable != null)
                UpdateLeftHelpingIcon(canvas);

            //Update right drawable
            if (rightDrawable != null)
                UpdateRightIcon(canvas);

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
        }
        private void UpdateLeftIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(leftDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - leftDrawable.IntrinsicHeight / 2;
            var bottom = top + leftDrawable.IntrinsicHeight;

            var left = borderPosition.Left + iconPadding;
            var right = left + leftDrawable.IntrinsicWidth;

            leftDrawable.drawable.SetBounds(left, top, right, bottom);
            leftDrawable.SetBounds(left, top, right, bottom);

            leftDrawable.Draw(canvas);
        }
        private void UpdateLeftHelpingIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(leftHelpingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - leftHelpingDrawable.IntrinsicHeight / 2;
            var bottom = top + leftHelpingDrawable.IntrinsicHeight;

            var left = borderPosition.Left + leftDrawableWidth + iconPadding + leftHelpingIconPadding;
            var right = left + leftHelpingDrawable.IntrinsicWidth;

            leftHelpingDrawable.drawable.SetBounds(left, top, right, bottom);
            leftHelpingDrawable.SetBounds(left, top, right, bottom);


            leftHelpingDrawable.Draw(canvas);
        }
        private void UpdateRightIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(rightDrawable);
            //top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - rightDrawable.IntrinsicHeight / 2;
            var bottom = top + rightDrawable.IntrinsicHeight;

            var left = borderPosition.Right - rightDrawable.IntrinsicWidth - iconPadding;
            var right = borderPosition.Right - iconPadding;

            rightDrawable.drawable.SetBounds(left, top, right, bottom);
            rightDrawable.SetBounds(left, top, right, bottom);

            rightDrawable.Draw(canvas);
        }
        private void UpdateRightHelpingIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(rightHelpingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - rightHelpingDrawable.IntrinsicHeight / 2;
            var bottom = top + rightHelpingDrawable.IntrinsicHeight;

            var left = borderPosition.Right - rightDrawableWidth - rightHelpingDrawable.IntrinsicWidth - iconPadding - rightHelpingIconPadding;
            var right = borderPosition.Right - rightDrawableWidth - iconPadding - rightHelpingIconPadding;

            rightHelpingDrawable.drawable.SetBounds(left, top, right, bottom);
            rightHelpingDrawable.SetBounds(left, top, right, bottom);
            
            rightHelpingDrawable.Draw(canvas);
        }
        private void UpdateLeftIconSeparator(Canvas canvas)
        {
            int top = GetTopIconPosition(leftHelpingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - leftHelpingDrawable.IntrinsicHeight / 2;
            var bottom = top + leftHelpingDrawable.IntrinsicHeight;

            var left = borderPosition.Left + leftDrawableWidth + iconPadding / 2 + leftHelpingIconPadding / 2;

            canvas.DrawLine(left, top, left, bottom, iconsSearator);
        }
        private void UpdateRighIconSeparator(Canvas canvas)
        {
            int top = GetTopIconPosition(rightHelpingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - rightHelpingDrawable.IntrinsicHeight / 2;
            var bottom = top + rightHelpingDrawable.IntrinsicHeight;

            var left = borderPosition.Right - rightDrawableWidth - iconPadding / 2 - rightHelpingIconPadding / 2;

            canvas.DrawLine(left, top, left, bottom, iconsSearator);
        }
        private void UpdateLeadingIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(leadingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - leadingDrawable.IntrinsicHeight / 2;
            var bottom = top + leadingDrawable.IntrinsicHeight;

            var left = textRect.Left;
            var right = textRect.Left + leadingDrawable.IntrinsicWidth;

            leadingDrawable.drawable.SetBounds(left, top, right, bottom);
            leadingDrawable.SetBounds(left, top, right, bottom);

            leadingDrawable.Draw(canvas);
        }
        private void UpdateTrailingIcon(Canvas canvas)
        {
            int top = GetTopIconPosition(trailingDrawable);
            //var top = (int)floatingHintYPostionNotFloating - floatingHintBoundsNotFloating.Height() / 2 - trailingDrawable.IntrinsicHeight / 2;
            var bottom = top + trailingDrawable.IntrinsicHeight;

            var left = textRect.Right - trailingDrawable.IntrinsicWidth;
            var right = textRect.Right;

            trailingDrawable.drawable.SetBounds(left, top, right, bottom);
            trailingDrawable.SetBounds(left, top, right, bottom);

            trailingDrawable.Draw(canvas);
        }

        //Update Border
        private void UpdateBorder(Canvas canvas)
        {
            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                //border
                borderPosition.Left = canvas.ClipBounds.Left + leadingDrawableWidth;
                borderPosition.Top = canvas.ClipBounds.Top + (int)textSpacingFromBorderTop;
                borderPosition.Right = canvas.ClipBounds.Right - trailingDrawableWidth;
                borderPosition.Bottom = canvas.ClipBounds.Bottom - (int)textSpacingFromBorderBottom;


                //Clip top border first and check if api < 26
                canvas.Save();

                if (IsFloating && !string.IsNullOrEmpty(floatingHintText))
                {
                    if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                    {
                        canvas.ClipRect(borderPosition.Left + leftRightSpacingLabels,
                                        canvas.ClipBounds.Top,
                                        borderPosition.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels,
                                        canvas.ClipBounds.Top + textSpacingFromBorderTop + floatingHintBoundsFloating.Height(),
                                        global::Android.Graphics.Region.Op.Difference);
                    }
                    else
                    {
                        canvas.ClipOutRect(borderPosition.Left + leftRightSpacingLabels - floatingHintClipPadding,
                                           canvas.ClipBounds.Top,
                                           borderPosition.Left + floatingHintBoundsFloating.Width() + leftRightSpacingLabels + floatingHintClipPadding,
                                           canvas.ClipBounds.Top + textSpacingFromBorderTop + floatingHintBoundsFloating.Height());
                    }
                }

                canvas.DrawRoundRect(new RectF(borderPosition.Left + borderWidth,
                                               borderPosition.Top,
                                               borderPosition.Right - borderWidth,
                                               borderPosition.Bottom),
                                               borderRadius, borderRadius,
                                               filledPaint);


                canvas.DrawRoundRect(new RectF(borderPosition.Left + borderWidth,
                                               borderPosition.Top,
                                               borderPosition.Right - borderWidth,
                                               borderPosition.Bottom),
                                               borderRadius, borderRadius,
                                               borderPaint);

                canvas.Restore();

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                //border
                borderPosition.Left = canvas.ClipBounds.Left + leadingDrawableWidth;
                borderPosition.Top = canvas.ClipBounds.Top;
                borderPosition.Right = canvas.ClipBounds.Right - trailingDrawableWidth;
                borderPosition.Bottom = textRect.Bottom - (int)textSpacingFromBorderBottom;


                canvas.DrawRect(new RectF(borderPosition.Left,
                                          borderPosition.Top,
                                          borderPosition.Right,
                                          borderPosition.Bottom),
                                          filledPaint);


                Path path2 = new Path();
                path2.MoveTo(borderPosition.Left, canvas.ClipBounds.Top + this.Height - textSpacingFromBorderBottom);
                path2.LineTo(borderPosition.Right, canvas.ClipBounds.Top + this.Height - textSpacingFromBorderBottom);
                path2.Close();//Given close, last lineto can be removed.
                canvas.DrawPath(path2, borderPaint);

            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                //border
                borderPosition.Left = canvas.ClipBounds.Left + leadingDrawableWidth;
                borderPosition.Top = canvas.ClipBounds.Top;
                borderPosition.Right = canvas.ClipBounds.Right - trailingDrawableWidth;
                borderPosition.Bottom = textRect.Bottom - (int)textSpacingFromBorderBottom;


                var path = RoundedRect(borderPosition.Left, borderPosition.Top, borderPosition.Right, borderPosition.Bottom, this.borderRadius, this.borderRadius, true);
                Path path2 = new Path();
                path2.MoveTo(borderPosition.Left, borderPosition.Bottom);
                path2.LineTo(borderPosition.Right, borderPosition.Bottom);
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

        //Update Floating Hint
        public void floatingHintPositionUpdate()
        {
            //floatingHintXPostionNotFloating same for all
            if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.Center)
                floatingHintXPostionNotFloating = this.Width / 2 - (PaddingRight - PaddingLeft) / 2 - floatingHintBoundsNotFloating.Width() / 2;
            else if (rSControl.HorizontalTextAlignment == Forms.TextAlignment.End)
                floatingHintXPostionNotFloating = this.Width - PaddingRight - floatingHintBoundsNotFloating.Width();
            else
                floatingHintXPostionNotFloating = this.PaddingLeft;


            if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
            {
                //X
                floatingHintXPostionFloating = this.PaddingLeft;

                //Y
                floatingHintYPositionFloating = Baseline - textSpacingFromBorderBottom / 2 - floatingHintBoundsNotFloating.Height();
                floatingHintYPostionNotFloating = Baseline - textSpacingFromBorderBottom / 2;

                //var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + floatingHintBoundsNotFloating.Height() / 2;
                //floatingHintYPostionNotFloating = center;
            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.OutlinedBorder)
            {
                //X
                floatingHintXPostionFloating = (int)leadingDrawableWidth + leftRightSpacingLabels;

                //Y
                floatingHintYPositionFloating = textSpacingFromBorderTop + floatingHintBoundsFloating.Height() / 2;
                floatingHintYPostionNotFloating = Baseline;

                //This gives accurate center position but the other one is used to match edit text baseline
                //var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2  + floatingHintBoundsNotFloating.Height() / 2;
                //floatingHintYPostionNotFloating = center;
            }
            else if (this.rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
            {
                //X
                floatingHintXPostionFloating = this.PaddingLeft;
                
                //Y
                floatingHintYPositionFloating = Baseline - textSpacingFromBorderBottom / 2 - floatingHintBoundsNotFloating.Height();
                floatingHintYPostionNotFloating = Baseline;
            }

            
            if (IsFloating)
            {
                floatingHintPaint.TextSize = labelsTextSize;
                floatingHintXPostion = floatingHintXPostionFloating;
                floatingHintYPostion = floatingHintYPositionFloating;
            }
            else
            {
                floatingHintPaint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                floatingHintXPostion = floatingHintXPostionNotFloating;
                floatingHintYPostion = floatingHintYPostionNotFloating;
            }
        }
        private void UpdateFloatingHint(Canvas canvas)
        {
            canvas.DrawText(floatingHintText, floatingHintXPostion + textRect.Left, floatingHintYPostion + textRect.Top, floatingHintPaint);
        }

        //Update error / helper / counter
        private void UpdateErrorMessage(Canvas canvas)
        {
            errorYPosition = this.Height - textSpacingFromBorderBottom - errorPaint.Ascent() + 2 + textRect.Top;
            canvas.DrawText(errorMessage, textRect.Left + leftRightSpacingLabels + leadingDrawableWidth, errorYPosition, errorPaint);
        }
        private void UpdateHelperMessage(Canvas canvas)
        {
            helperYPosition = this.Height - textSpacingFromBorderBottom - helperPaint.Ascent() + 2 + textRect.Top;
            canvas.DrawText(helperMessage, textRect.Left + leftRightSpacingLabels + leadingDrawableWidth, helperYPosition, helperPaint);
        }
        private void UpdateCounterMessage(Canvas canvas)
        {
            counterYPosition = this.Height - textSpacingFromBorderBottom - counterPaint.Ascent() + 2 + textRect.Top;
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);
            canvas.DrawText(counterMessage, textRect.Right - trailingDrawableWidth - (counterMessageBounds.Width()) - leftRightSpacingLabels, counterYPosition, counterPaint);
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
                yEnd = floatingHintYPositionFloating;

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
            bool rightIconCondition = this.rightDrawable != null && e.GetX() >= Right - trailingDrawableWidth - rightDrawable.IntrinsicWidth - iconPadding && e.GetX() <= Right - trailingDrawableWidth;
            bool trailingIconCondition = trailingDrawable != null && e.GetX() >= Right - trailingDrawableWidth && e.GetX() <= Right;
            bool rightHelpingIconCondition = rightHelpingDrawable != null && e.GetX() >= rightHelpingDrawable.Bounds.Left - textRect.Left && e.GetX() <= rightHelpingDrawable.Bounds.Right - textRect.Left + iconPadding;
            bool leadingIconCondition = leadingDrawable != null && e.GetX() >= Left && e.GetX() <= Left + leadingDrawableWidth;
            bool leftIconCondition = leftDrawable != null && e.GetX() >= Left + leadingDrawableWidth && e.GetX() <= Left + leadingDrawableWidth + leftDrawable.IntrinsicWidth + iconPadding;
            bool leftHelpingIconCondition = leftHelpingDrawable != null && e.GetX() >= leftHelpingDrawable.Bounds.Left - textRect.Left - iconPadding && e.GetX() <= leftHelpingDrawable.Bounds.Right - textRect.Left;

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
                    else if (rSControl.RightIcon.View.IsEnabled)
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
                else if (trailingIconCondition && rSControl.TrailingIcon.View.IsEnabled)
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
                else if (rightHelpingIconCondition && rSControl.RightHelpingIcon.View.IsEnabled)
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
                else if (leadingIconCondition && rSControl.LeadingIcon.View.IsEnabled)
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
                else if (leftIconCondition && rSControl.LeftIcon.View.IsEnabled)
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
                else if (leftHelpingIconCondition && rSControl.LeftHelpingIcon.View.IsEnabled)
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

        //Constructor
        public CustomDrawable(Drawable drawable, CustomEditText editText) : base()
        {
            this.drawable = drawable;
            this.editText = editText;
            this.selected = false;
            clicked = false;

            CreateRippleEffect();
        }


        //Draw
        public override void Draw(Canvas canvas)
        {
            //Icon click effect
            if (ripple)
                DrawFingerPrint(canvas);

            this.drawable.Draw(canvas);
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
        private void DrawFingerPrint(Canvas canvas)
        {
            var divideRadiusValue = this.drawable.IntrinsicWidth / 12;

            canvas.DrawCircle(this.drawable.Bounds.CenterX(),
                              this.drawable.Bounds.CenterY(),
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