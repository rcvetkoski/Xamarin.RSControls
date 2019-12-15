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

[assembly: ExportRenderer(typeof(RSEntry), typeof(RSEntryRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSEntryRenderer : EntryRenderer
    {
        private bool isTextInputLayout;

        public RSEntryRenderer(Context context) : base(context)
        {
            var lol = this.Element;
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
            {
                if((this.Element as IRSControl).RightIcon == null)
                    (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon() { Path = "Samples/Data/SVG/calendarAndTime.svg" };

                return new CustomEditText(Context, this.Element as IRSControl);
            }
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
        private float floatingHintYPostionWhenFloating;
        private float floatingHintYPostionNotFloating;
        private bool hasInitfloatingHintYPosition = false;
        private float errorYPosition;
        private float helperYPosition;
        private float counterYPosition;
        public Thickness CustomPadding;

        //icon drawables
        private CustomDrawable leadingDrawable;
        private CustomDrawable customDrawable;
        private CustomDrawable trailingDrawable;
        private CustomDrawable leftDrawable;
        private CustomDrawable rightDrawable;
        private Paint iconsSearator;
        private int iconsSpacing;
        private int leftDrawableWidth;
        private int leadingIconWidth;
        private int trailingIconWidth;

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
            iconsSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics);
            errorMessage = string.Empty;
            borderColor = rSControl.BorderColor.ToAndroid();
            activeColor = rSControl.ActiveColor.ToAndroid();
            errorColor = rSControl.ErrorColor.ToAndroid();


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
                    this.rSControl.Padding = new Thickness(5, 12, 5, 12);
                else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.Underline)
                    this.rSControl.Padding = new Thickness(0, 6, 0, 4);
                else if (rSControl.RSEntryStyle == Enums.RSEntryStyleSelectionEnum.FilledBorder)
                    this.rSControl.Padding = new Thickness(5, 16, 5, 7);
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
                    borderPaint.Color = errorColor;
                    floatingHintPaint.Color = errorColor;
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

                    if (customDrawable != null)
                        customDrawable.drawable.SetTint(activeColor);
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
                    floatingHintPaint.Color = borderColor;

                    if (rightDrawable != null)
                        rightDrawable.drawable.SetTint(borderColor);

                    if (leftDrawable != null)
                        leftDrawable.drawable.SetTint(borderColor);

                    if (leadingDrawable != null)
                        leadingDrawable.drawable.SetTint(borderColor);

                    if (trailingDrawable != null)
                        trailingDrawable.drawable.SetTint(borderColor);

                    if (customDrawable != null)
                        customDrawable.drawable.SetTint(borderColor);
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
            helperPaint.Color = borderColor;
            helperPaint.TextSize = labelsTextSize;
            helperPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            helperPaint.AntiAlias = true;
        }
        private void CreateCounterMessage()
        {
            counterPaint = new TextPaint();
            counterMessageBounds = new Rect();
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);

            counterPaint.Color = borderColor;
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

            rightDrawable.Command = new Command(PasswordCommand);
        }

        private void  SetIcons()
        {
            this.CompoundDrawablePadding = 15;

            string rightPath = string.Empty;
            string leftPath = string.Empty;


            //Leading Icon
            if(!string.IsNullOrEmpty(this.rSControl.LeadingIcon))
            {
                var leadingDraw = Context.GetDrawable(RSControls.Droid.Resource.Drawable.avd_show_password);
                leadingDraw.SetTint(borderColor);
                leadingDrawable = new CustomDrawable(leadingDraw, this);
                leadingIconWidth = leadingDrawable.IntrinsicWidth + (this.CompoundDrawablePadding * 2);
                this.SetPadding(this.PaddingLeft + leadingIconWidth, this.PaddingTop, this.PaddingRight, this.PaddingBottom);
            }



            //Password
            if (this.rSControl.IsPassword)
                CreatePasswordIcon();


            //Right Icon
            if (this.rSControl.RightIcon != null)
            {
                //Custom Icon
                var dr = Context.GetDrawable(RSControls.Droid.Resource.Drawable.avd_hide_password);
                dr.SetTint(borderColor);
                customDrawable = new CustomDrawable(dr, this);
                int customDrawableClip = customDrawable != null ? customDrawable.IntrinsicWidth + iconsSpacing + this.CompoundDrawablePadding : 0;

                //Icons Separator
                iconsSearator = new Paint();
                iconsSearator.AntiAlias = true;
                iconsSearator.Color = borderColor;
                iconsSearator.StrokeWidth = 2;

                this.rightDrawable = CreateDrawable(rSControl.RightIcon.Path, rSControl.RightIcon.Command, rSControl.RightIcon.CommandParameters, customDrawableClip);
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

                leftDrawableWidth = this.leftDrawable.IntrinsicWidth;
            }

            //Trailing Icon
            if (!string.IsNullOrEmpty(this.rSControl.TrailingIcon))
            {
                var trailingDraw = Context.GetDrawable(RSControls.Droid.Resource.Drawable.ic_mtrl_chip_checked_circle);
                trailingDraw.SetTint(borderColor);
                trailingDrawable = new CustomDrawable(trailingDraw, this);
                trailingIconWidth = trailingDrawable.IntrinsicWidth + (this.CompoundDrawablePadding * 2);
                this.SetPadding(this.PaddingLeft, this.PaddingTop, this.PaddingRight + trailingIconWidth, this.PaddingBottom);
            }




            //Set Drawable to control
            this.SetCompoundDrawables(this.leftDrawable, null, this.rightDrawable, null);

            leftDrawableWidth = leftDrawable != null ? leftDrawable.IntrinsicWidth + this.CompoundDrawablePadding : 0;
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


        private CustomDrawable CreateDrawable(string path, string commandName, List<object> commandParameters, int customDrawableClip)
        {
            int pixel = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)rSControl.IconSize, Context.Resources.DisplayMetrics);
            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = path, HeightRequest = pixel, WidthRequest = pixel, Color = rSControl.IconColor };
            var convertedView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new Rectangle(), Context);
            var bitmapDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedView, pixel, pixel));
            bitmapDrawable.SetBounds(customDrawableClip, 0, bitmapDrawable.IntrinsicWidth + customDrawableClip, bitmapDrawable.IntrinsicHeight);
            var drawable = new CustomDrawable(bitmapDrawable, this);
            drawable.SetBounds(0, 0, bitmapDrawable.IntrinsicWidth + customDrawableClip, bitmapDrawable.IntrinsicHeight);

            var source = (rSControl as RSEntry).BindingContext;
            MethodInfo methodInfo;

            if (commandParameters != null)
            {
                List<Type> t = new List<Type>();
                foreach (var item in commandParameters)
                {
                    t.Add(item.GetType());
                }
                methodInfo = source.GetType().GetMethod(commandName, t.ToArray());
            }
            else
                methodInfo = source.GetType().GetMethod(commandName);

            drawable.Command = new Command<object>((x) => ExecuteCommand(methodInfo, source, commandParameters.ToArray()));


            return drawable;
        }


        private void ExecuteCommand(MethodInfo methodInfo, object source, object[] parameters)
        {
            if (parameters.Any())
            {
                methodInfo.Invoke(source, parameters);
            }
            else
                methodInfo.Invoke(source, null);
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
                    floatingHintXPostionFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth;
                    floatingHintXPostionNotFloating = textRect.Left + this.PaddingLeft + leftDrawableWidth;



                    //Y
                    floatingHintYPostionWhenFloating = -this.Paint.Ascent(); /*topSpacing + floatingHintBoundsFloating.Height();*/
                    floatingHintYPostionNotFloating = (this.Height - bottomSpacing) / 2 - ((floatingHintBoundsNotFloating.Bottom + floatingHintBoundsNotFloating.Top) / 2);
                }
                else
                {
                    //X
                    floatingHintXPostionFloating = textRect.Left + leftRightSpacingLabels + leadingIconWidth;
                    floatingHintXPostionNotFloating = textRect.Left + leftRightSpacingLabels + leftDrawableWidth + leadingIconWidth;


                    //Y
                    floatingHintYPostionWhenFloating = topSpacing + floatingHintBoundsFloating.Height() / 2;
                    floatingHintYPostionNotFloating = this.Baseline;
                }


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


                if (errorPaint != null)
                    errorYPosition = this.Height - bottomSpacing - errorPaint.Ascent() + 2;

                if (helperPaint != null)
                    helperYPosition = this.Height - bottomSpacing - helperPaint.Ascent() + 2;

                if (counterPaint != null)
                    counterYPosition = this.Height - bottomSpacing - counterPaint.Ascent() + 2;


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

            if(customDrawable != null)
                UpdateCustomIcon(canvas, textRect);

            if(leadingDrawable != null)
                UpdateLeadingIcon(canvas, textRect);

            if (trailingDrawable != null)
                UpdateTrailingIcon(canvas, textRect);
            

            base.OnDraw(canvas);
        }

        private void UpdateCustomIcon(Canvas canvas, Rect textRect)
        {
            var correctiveY = (int)Math.Abs(CustomPadding.Top - CustomPadding.Bottom) * 2;
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - customDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + customDrawable.IntrinsicHeight / 2;

            customDrawable.drawable.SetBounds(textRect.Right - this.PaddingRight - iconsSpacing - customDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - iconsSpacing - customDrawable.IntrinsicWidth, center2);
            customDrawable.SetBounds(textRect.Right - this.PaddingRight - iconsSpacing - customDrawable.IntrinsicWidth * 2, center, textRect.Right - this.PaddingRight - iconsSpacing - customDrawable.IntrinsicWidth, center2);


            customDrawable.Draw(canvas);

            //Separator
            canvas.DrawLine(textRect.Right - this.PaddingRight - rightDrawable.IntrinsicWidth - iconsSpacing / 2,
                            center - correctiveY,
                            textRect.Right - this.PaddingRight - rightDrawable.IntrinsicWidth - iconsSpacing / 2,
                            center2 - correctiveY,
                            iconsSearator);
        }
        private void UpdateLeadingIcon(Canvas canvas, Rect textRect)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - leadingDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + leadingDrawable.IntrinsicHeight / 2;

            leadingDrawable.drawable.SetBounds(textRect.Left + this.CompoundDrawablePadding, center, textRect.Left + this.CompoundDrawablePadding + leadingDrawable.IntrinsicWidth, center2);
            leadingDrawable.SetBounds(textRect.Left + this.CompoundDrawablePadding, center, textRect.Left + this.CompoundDrawablePadding + this.PaddingLeft + this.PaddingLeft + leadingDrawable.IntrinsicWidth, center2);

            leadingDrawable.Draw(canvas);
        }
        private void UpdateTrailingIcon(Canvas canvas, Rect textRect)
        {
            var center = (int)(this.Height - PaddingBottom + PaddingTop) / 2 - trailingDrawable.IntrinsicHeight / 2;
            var center2 = (int)(this.Height - PaddingBottom + PaddingTop) / 2 + trailingDrawable.IntrinsicHeight / 2;

            trailingDrawable.drawable.SetBounds(textRect.Right - this.CompoundDrawablePadding - trailingDrawable.IntrinsicWidth, center, textRect.Right - this.CompoundDrawablePadding, center2);
            trailingDrawable.SetBounds(textRect.Right - this.CompoundDrawablePadding - trailingDrawable.IntrinsicWidth, center, textRect.Right - this.CompoundDrawablePadding + this.PaddingRight + this.PaddingRight, center2);

            trailingDrawable.Draw(canvas);
        }


        //Update Methods
        private void UpdateFloatingHint(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(floatingHintText, textRect.Left + floatingHintXPostion, floatingHintYPostion, floatingHintPaint);
        }
        private void UpdateErrorMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(errorMessage, textRect.Left + leftRightSpacingLabels + leadingIconWidth, errorYPosition, errorPaint);
        }
        private void UpdateHelperMessage(Canvas canvas, Rect textRect)
        {
            canvas.DrawText(helperMessage, textRect.Left + leftRightSpacingLabels + leadingIconWidth, helperYPosition, helperPaint);
        }
        private void UpdateCounterMessage(Canvas canvas, Rect textRect)
        {
            counterMessage = string.Format("{0}/{1}", this.Length(), rSControl.CounterMaxLength);
            canvas.DrawText(counterMessage, textRect.Right - trailingIconWidth - (counterMessageBounds.Width()) - leftRightSpacingLabels, counterYPosition, counterPaint);
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

                canvas.DrawRoundRect(new RectF(textRect.Left + corectCorners + leadingIconWidth,
                                               textRect.Top + corectCorners + topSpacing,
                                               textRect.Right - corectCorners - trailingIconWidth,
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
                canvas.DrawRect(textRect.Left + corectCorners + leadingIconWidth,
                                textRect.Bottom - bottomSpacing,
                                textRect.Right - corectCorners - trailingIconWidth,
                                textRect.Bottom - bottomSpacing,
                                borderPaint);

                canvas.Save();

                if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.O)
                {
                    canvas.ClipRect(textRect.Left + leadingIconWidth,
                                    textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth,
                                    textRect.Right - trailingIconWidth,
                                    textRect.Bottom,
                                    global::Android.Graphics.Region.Op.Difference);
                }
                else
                {
                    canvas.ClipOutRect(textRect.Left + leadingIconWidth,
                                       textRect.Bottom - bottomSpacing - borderPaint.StrokeWidth,
                                       textRect.Right - trailingIconWidth,
                                       textRect.Bottom);
                }


                canvas.DrawRoundRect(new RectF(textRect.Left + leadingIconWidth,
                                               textRect.Top,
                                               textRect.Right - trailingIconWidth,
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
            floatingHintXPostion = (float)e.Animation.GetAnimatedValue("XPosition");
            floatingHintYPostion = (float)e.Animation.GetAnimatedValue("YPosition");
            Invalidate();
        }
        private void AnimateFloatingHint()
        {
            isFloatingHintAnimating = true;
            float textSizeStart;
            float textSizeEnd;
            float xStart;
            float xEnd;
            float yStart;
            float yEnd;

            if (!IsFloating())
            {
                textSizeStart = labelsTextSize;
                textSizeEnd = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                xStart = floatingHintXPostionFloating;
                xEnd = floatingHintXPostionNotFloating;
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionNotFloating;
            }
            else
            {
                textSizeStart = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)this.rSControl.FontSize, Context.Resources.DisplayMetrics);
                textSizeEnd = labelsTextSize;
                xStart = floatingHintXPostionNotFloating;
                xEnd = floatingHintXPostionFloating;
                yStart = floatingHintYPostion;
                yEnd = floatingHintYPostionWhenFloating;
            }

            PropertyValuesHolder propertyTextSizeError = PropertyValuesHolder.OfFloat("textSize", textSizeStart, textSizeEnd);
            PropertyValuesHolder propertyXPositionError = PropertyValuesHolder.OfFloat("XPosition", xStart, xEnd);
            PropertyValuesHolder propertyYPositionError = PropertyValuesHolder.OfFloat("YPosition", yStart, yEnd);
            floatingHintAnimator.SetValues(propertyTextSizeError, propertyXPositionError, propertyYPositionError);


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
                if (this.rightDrawable != null && e.GetX() >= Right + leadingIconWidth - trailingIconWidth - PaddingLeft - rightDrawable.IntrinsicWidth - iconsSpacing / 2 && e.GetX() <= Right - trailingIconWidth)
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
                else if (trailingDrawable != null && e.GetX() >= Right - trailingIconWidth && e.GetX() <= Right)
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
                else if (leadingDrawable != null && e.GetX() >= Left && e.GetX() <= Left + leadingIconWidth)
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
                else if (leftDrawable != null && e.GetX() >= Left + leadingIconWidth && e.GetX() <= Left + leadingIconWidth + leftDrawable.IntrinsicWidth + this.PaddingLeft)
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
                else if (customDrawable != null && e.GetX() >= Right - trailingIconWidth + leadingIconWidth - PaddingLeft - iconsSpacing - customDrawable.IntrinsicWidth * 2 && e.GetX() <= customDrawable.Bounds.Right - trailingIconWidth + leadingIconWidth + iconsSpacing / 2)
                {
                    if (customDrawable.Selected)
                    {
                        customDrawable.Selected = false;
                    }
                    else
                    {
                        customDrawable.Selected = true;
                    }
                }
                else
                    return base.OnTouchEvent(e);

                this.SetSelection(this.Length());
                Invalidate();
                return true;
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
            var y = (float)Math.Abs(this.editText.CustomPadding.Top - this.editText.CustomPadding.Bottom) * 2;

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