using System;
using System.Reflection;
using System.Windows.Input;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using static Android.Views.ViewTreeObserver;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup, IDisposable, global::Android.Views.View.IOnClickListener, IOnGlobalLayoutListener
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool UserSetPosition { get; set; }
        public bool UserSetSize { get; set; }
        public float BorderRadius { get; set; }
        public Forms.Color BorderFillColor { get; set; }
        public float DimAmount { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Forms.View RelativeView { get; set; }
        public Forms.View CustomView { get; set; }
        private global::Android.Widget.RelativeLayout customLayout;
        private LinearLayout contentView;
        private LinearLayout linearLayout;
        private LinearLayout buttonsLayout;
        private global::Android.Views.View arrow;
        private global::Android.Graphics.Point arrowSize;
        private ArrowTypeEnum arrowType;
        public bool ShadowEnabled { get; set; }
        public bool IsModal { get; set; }
        private RSAndroidButton positiveButton;
        private RSAndroidButton neutralButton;
        private RSAndroidButton destructiveButton;
        private int dialogHorizontalMargin = 0;
        private int dialogVerticalMargin = 0;


        private int widthSpec;
        private int heightSpec;



        public RSPopupRenderer()
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as global::Android.Widget.RelativeLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
            linearLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.linearLayout);
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
            arrow = customLayout.FindViewById<global::Android.Views.View>(Resource.Id.arrow);

            customLayout.SetOnClickListener(this);
            linearLayout.SetOnClickListener(this);


            ////Set here so it will be given good dimensions
            //if (CustomView != null)
            //{
            //    SetCustomView();
            //}

            //this.contentView.AddView(new TextView(((AppCompatActivity)RSAppContext.RSContext)) { Text = "trolol", TextAlignment = global::Android.Views.TextAlignment.Center, Gravity = GravityFlags.Center });
        }


        public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            global::Android.Support.V7.App.AlertDialog.Builder builder = new global::Android.Support.V7.App.AlertDialog.Builder(Context, Resource.Style.RSDialogAnimationTheme);
            //builder.SetTitle(Title);
            //builder.SetMessage(this.Message);
            //builder.SetPositiveButton("Done", new EventHandler<DialogClickEventArgs>(PositiveButton_Click));
            //builder.SetNegativeButton("Cancel", new EventHandler<DialogClickEventArgs>(PositiveButton_Click));
            return builder.Create();
        }

        public override void OnStart()
        {
            base.OnStart();

            //Fix for Control that call keyboard like entry (keyboard not showing up)
            Dialog.Window.ClearFlags(WindowManagerFlags.NotFocusable | WindowManagerFlags.AltFocusableIm);
            //Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);




            SetDialog();

            if (CustomView != null)
            {
                SetCustomView();
            }
        }

        //Set PopupSize
        private void SetPopupSize(DisplayMetrics metrics)
        {
            if(UserSetSize)
            {
                if(Width != -2 && Width != -1)
                {
                    Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Width, Context.Resources.DisplayMetrics);

                    //Fix if width user inputs greater than creen width
                    if (Width > metrics.WidthPixels - dialogHorizontalMargin)
                        Width = metrics.WidthPixels - dialogHorizontalMargin;

                    widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Width, MeasureSpecMode.Exactly);
                }
                else
                {
                    if (Width == -2)
                        widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Width, MeasureSpecMode.AtMost);
                    else
                        widthSpec = Width;
                }

                if (Height != -2 && Height != -1)
                {
                    Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Height, Context.Resources.DisplayMetrics);

                    //Fix if height user inputs greater than creen height
                    if (Height > metrics.HeightPixels - dialogVerticalMargin)
                        Height = metrics.HeightPixels - dialogVerticalMargin;

                    heightSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Height, MeasureSpecMode.Exactly);

                }
                else
                {
                    if(Height == -2)
                        heightSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Height, MeasureSpecMode.AtMost);
                    else
                        heightSpec = Height;
                }
            }
            else
            {
                Width = ViewGroup.LayoutParams.WrapContent;
                Height = ViewGroup.LayoutParams.WrapContent;


                widthSpec = Width;
                heightSpec = Height;
            }
        }

        //Set Popup position at coordinates
        private void SetPopupAtPosition()
        {
            PositionX -= dialogHorizontalMargin;
            PositionY -= dialogVerticalMargin;
        }

        //Set Popup position relative to view
        private void SetPopupPositionRelativeTo(Forms.View formsView, DisplayMetrics metrics)
        {
            if(formsView != null)
            {
                var nativeView = Xamarin.Forms.Platform.Android.Platform.GetRenderer(formsView).View;
                Rect rectf = new Rect();
                nativeView.GetWindowVisibleDisplayFrame(rectf);
                int[] locationScreen = new int[2];
                nativeView.GetLocationOnScreen(locationScreen);
                var relativeViewHeight = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Height, Context.Resources.DisplayMetrics));
                relativeViewHeight = 0;

                global::Android.Graphics.Point size = new global::Android.Graphics.Point();
                (Context as AppCompatActivity).WindowManager.DefaultDisplay.GetRealSize(size);


                linearLayout.Measure(widthSpec, heightSpec);

                int y = 0;

                int measuredHeight = 0;
                if (Height == -2 || Height == -1)
                    measuredHeight = linearLayout.MeasuredHeight;
                else
                    measuredHeight = Height;

                //int pos = (locationScreen[1] + dialogVerticalMargin + relativeViewHeight + measuredHeight);
                //int fixedHeight = (locationScreen[1] - rectf.Top - dialogVerticalMargin - measuredHeight);
                //if (metrics.HeightPixels < pos && fixedHeight > 0)
                //    y = fixedHeight;
                //else
                //    y = locationScreen[1] - rectf.Top + relativeViewHeight + dialogVerticalMargin;




                if (metrics.HeightPixels > (locationScreen[1] + measuredHeight + relativeViewHeight - rectf.Top))
                    y = locationScreen[1] + relativeViewHeight - rectf.Top;
                else if ((locationScreen[1] - measuredHeight - rectf.Top) > 0)
                {
                    y = locationScreen[1] - measuredHeight - rectf.Top;
                }
                else
                {
                    if (locationScreen[1] > (metrics.HeightPixels / 2))
                        y = locationScreen[1] - relativeViewHeight - measuredHeight - rectf.Top + Math.Abs(metrics.HeightPixels - locationScreen[1] - measuredHeight);
                    else
                        y = locationScreen[1] + relativeViewHeight - rectf.Top - Math.Abs(metrics.HeightPixels - locationScreen[1] - measuredHeight);
                }
                //y = (locationScreen[1] - measuredHeight - rectf.Top) + (Math.Abs(locationScreen[1] - measuredHeight));
                //y = (metrics.HeightPixels / 2) - (measuredHeight / 2);



                //y = locationScreen[1] + relativeViewHeight - rectf.Top;

                PositionX = locationScreen[0] - rectf.Left - dialogHorizontalMargin;
                PositionY = y;
            }
        }

        //Show popup
        public void ShowPopup()
        {
            //Check if it is already added & ANDROID context not null
            // if (!this.IsAdded && RSAppContext.RSContext != null)
            this.Show(((AppCompatActivity)RSAppContext.RSContext).SupportFragmentManager, "sc");
        }

        //Dismiss
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);

            this.Dispose();
        }

        //Set and add custom view 
        private void SetCustomView()
        {
            var renderer = Platform.CreateRendererWithContext(CustomView, Context);
            Platform.SetRenderer(CustomView, renderer);
            var convertView = new Extensions.ViewCellContainer(Context, CustomView, renderer);
            EditText editText = new EditText(Context);
            editText.Text = "TrozzuuzzukzkzukzukMeht";
            this.contentView.AddView(convertView);
            contentView.LayoutParameters = new LinearLayout.LayoutParams(Width, ViewGroup.LayoutParams.WrapContent);
        }

        //Set native view 
        public void SetNativeView(global::Android.Views.View nativeView)
        {
            this.contentView.AddView(nativeView);
        }

        //Set dialog properties
        private void SetDialog()
        {
            var attrs = this.Dialog.Window.Attributes;
            var metrics = Resources.DisplayMetrics;
            var minWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 270, Context.Resources.DisplayMetrics);
            var minHeigth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 150, Context.Resources.DisplayMetrics);

            arrowType = ArrowTypeEnum.Right;
            SetBackground();
            SetCustomLayout();


            //customLayout.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            //customLayout.Measure(metrics.WidthPixels, metrics.HeightPixels);
            //linearLayout.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);


            //Popup size
            SetPopupSize(metrics);

            //Apply size
            attrs.Width = ViewGroup.LayoutParams.MatchParent;
            attrs.Height = ViewGroup.LayoutParams.MatchParent;


            linearLayout.LayoutParameters = new global::Android.Widget.RelativeLayout.LayoutParams(widthSpec, heightSpec);
            linearLayout.Measure(widthSpec, heightSpec);
            linearLayout.ViewTreeObserver.AddOnGlobalLayoutListener(this);

            //Position
            if (UserSetPosition)
            {
                //Set the gravity top and left so it starts at real 0 coordinates than aply position
                Dialog.Window.SetGravity(GravityFlags.Left | GravityFlags.Top);


                if (RelativeView != null)
                    SetPopupPositionRelativeTo(RelativeView, metrics);
                else
                    SetPopupAtPosition();
            }
            else
            {
                customLayout.SetGravity(GravityFlags.Center);
            }


            arrowSize = new global::Android.Graphics.Point(0, 0);
            if(RelativeView != null)
            {
                arrowSize.X = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, Context.Resources.DisplayMetrics));
                arrowSize.Y = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Context.Resources.DisplayMetrics));
            }

            //Apply position
            setArrow();


            linearLayout.SetX(PositionX );
            linearLayout.SetY(PositionY );


            //Set dim amount
            attrs.DimAmount = this.DimAmount;
            
            //Set new attributes
            this.Dialog.Window.Attributes = attrs;
        }


        private void setArrow()
        {
            if (arrowType == ArrowTypeEnum.Up)
            {
                var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.X, arrowSize.Y);
                lp.AddRule(LayoutRules.AlignTop, linearLayout.Id);
                arrow.LayoutParameters = lp;
                arrow.SetX(arrow.GetX() + PositionX + 250);
                arrow.SetY(arrow.GetY() + PositionY + 1);
            }
            else if (arrowType == ArrowTypeEnum.Down)
            {
                var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.X, arrowSize.Y);
                lp.AddRule(LayoutRules.AlignBottom, linearLayout.Id);
                arrow.LayoutParameters = lp;
                arrow.SetX(arrow.GetX() + PositionX + 100);
                arrow.SetY(arrow.GetY() + PositionY - 1);
                arrow.Rotation = 180;
            }
            else if(arrowType == ArrowTypeEnum.Right)
            {
                var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                arrow.LayoutParameters = lp;
                arrow.SetX(PositionX - arrowSize.Y);
                arrow.SetY(arrow.GetY() + PositionY + 40);
            }
            else if(arrowType == ArrowTypeEnum.Left)
            {
                var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                lp.AddRule(LayoutRules.AlignLeft, linearLayout.Id);
                arrow.LayoutParameters = lp;
                arrow.SetX(arrow.GetX()  + 1);
                arrow.SetY(arrow.GetY() + PositionY + 100);                
            }
        }

        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            //Manipulate color and roundness of border
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.SetColor(BorderFillColor.ToAndroid());
            gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));
            gradientDrawable.SetStroke(1, global::Android.Graphics.Color.LightGray);
            InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, dialogHorizontalMargin, dialogVerticalMargin, dialogHorizontalMargin, dialogVerticalMargin); //Adds margin to alert
            linearLayout.SetBackground(insetDrawable);


            arrow.Background = new CustomArrow(arrow, arrowType, Context);
            //arrowDown.Background = new CustomArrow(arrowDown, ArrowTypeEnum.Right, Context);


            GradientDrawable transparentDrawable = new GradientDrawable();
            transparentDrawable.SetColor(global::Android.Graphics.Color.Transparent);
            Dialog.Window.SetBackgroundDrawable(transparentDrawable);
        }

        //Custom layout for dialog
        private void SetCustomLayout()
        {
            global::Android.Widget.TextView title = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_title);
            title.Text = this.Title;
            if (string.IsNullOrEmpty(title.Text))
                title.Visibility = ViewStates.Gone;
            global::Android.Widget.TextView message = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_message);
            message.Text = this.Message;
            if (string.IsNullOrEmpty(message.Text))
                message.Visibility = ViewStates.Gone;

            //customLayout.RemoveFromParent();
            Dialog.SetContentView(customLayout);
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter)
        {
            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_positive);
                positiveButton.dialog = this;
                positiveButton.CommandParameter = commandParameter;
                positiveButton.Command = command;
                positiveButton.Text = title;
                positiveButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_neutral);
                neutralButton.dialog = this;
                neutralButton.CommandParameter = commandParameter;
                neutralButton.Command = command;
                neutralButton.Text = title;
                neutralButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_destructive);
                destructiveButton.dialog = this;
                destructiveButton.CommandParameter = commandParameter;
                destructiveButton.Command = command;
                //destructiveButton.SetTextColor(global::Android.Graphics.Color.Red);
                destructiveButton.Text = title;
                destructiveButton.Visibility = ViewStates.Visible;
            }
            else
            {

            }

            //Make buttons layout visible / Hides divider if no button visible
            if (buttonsLayout.Visibility == ViewStates.Gone)
                buttonsLayout.Visibility = ViewStates.Visible;
        }
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, EventHandler handler)
        {
            if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_positive);
                positiveButton.ClickHandler = handler;
                positiveButton.dialog = this;
                positiveButton.Text = title;
                positiveButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_neutral);
                neutralButton.ClickHandler = handler;
                neutralButton.dialog = this;
                neutralButton.Text = title;
                neutralButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_destructive);
                destructiveButton.ClickHandler = handler;
                destructiveButton.dialog = this;
                destructiveButton.Text = title;
                destructiveButton.Visibility = ViewStates.Visible;
            }
            else
            {

            }

            //Make buttons layout visible / Hides divider if no button visible
            if (buttonsLayout.Visibility == ViewStates.Gone)
                buttonsLayout.Visibility = ViewStates.Visible;
        }

        //Orientation change
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            SetDialog();
        }

        //Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //Force dispose so we can remove CanExecuteChanged event on command in buttons dispose method
            positiveButton?.Dispose();
            neutralButton?.Dispose();
            destructiveButton?.Dispose();
        }

        public void OnClick(global::Android.Views.View v)
        {
            if (v.Id == customLayout.Id && !IsModal)
                this.Dialog.Dismiss();
        }

        public void OnGlobalLayout()
        {
            linearLayout.MeasuredWidth.ToString();
        }
    }


    public class RSAndroidButton : global::Android.Widget.Button, global::Android.Views.View.IOnClickListener
    {
        public EventHandler ClickHandler { get; set; }
        public object CommandParameter { get; set; }
        private Command command;
        public Command Command
        {
            get
            {
                return command;
            }
            set
            {
                command = value;

                if(command != null)
                {
                    Command.CanExecuteChanged += Command_CanExecuteChanged;
                    Command.ChangeCanExecute();
                }
            }
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            if ((sender as Command).CanExecute(CommandParameter))
                this.Enabled = true;
            else
                this.Enabled = false;
        }

        public global::Android.Support.V4.App.DialogFragment dialog { get; set; }
        //public PopupWindow dialog { get; set; }


        public RSAndroidButton(Context context) : base(context)
        {
            this.SetOnClickListener(this);
        }
        public RSAndroidButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.SetOnClickListener(this);
        }
        public RSAndroidButton(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            this.SetOnClickListener(this);
        }
        public RSAndroidButton(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            this.SetOnClickListener(this);
        }
        protected RSAndroidButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            this.SetOnClickListener(this);
        }

  
        //Button click
        public void OnClick(global::Android.Views.View v)
        {
            var button = v as RSAndroidButton;

            if (button.Id.Equals(Resource.Id.action_positive))
                this.dialog.Dismiss();

            if (button.Command != null)
                button.Command.Execute(null);

            if (button.ClickHandler != null)
                button.ClickHandler.Invoke(button, EventArgs.Empty);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if(this.Command != null)
                    this.Command.CanExecuteChanged -= Command_CanExecuteChanged;
            }
        }
    }

    public class CustomArrow : Drawable
    {
        private global::Android.Views.View arrow;
        private ArrowTypeEnum arrowType;
        private float shadowThikness;

        public CustomArrow(global::Android.Views.View arrow, ArrowTypeEnum arrowType, Context context) : base()
        {
            this.arrow = arrow;
            this.arrowType = arrowType;

            if(arrowType == ArrowTypeEnum.Up)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, context.Resources.DisplayMetrics);
            else if (arrowType == ArrowTypeEnum.Down)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1.5f, context.Resources.DisplayMetrics);
            else if (arrowType == ArrowTypeEnum.Right)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1f, context.Resources.DisplayMetrics);
            else if (arrowType == ArrowTypeEnum.Left)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1f, context.Resources.DisplayMetrics);
        }

        public override int Opacity => 1;

        public override void Draw(Canvas canvas)
        {
            if(arrowType == ArrowTypeEnum.Up || arrowType == ArrowTypeEnum.Down)
                drawArrow(canvas);
            else if(arrowType == ArrowTypeEnum.Right)
                drawArrowRight(canvas);
            else if (arrowType == ArrowTypeEnum.Left)
                drawArrowLeft(canvas);
        }

        public override void SetAlpha(int alpha)
        {
            
        }

        public override void SetColorFilter(ColorFilter colorFilter)
        {
            
        }

        private void drawArrow(Canvas canvas)
        {
            var width = arrow.Width;
            var height = arrow.Height;

            //Shadow arrow
            Paint paint = new Paint();
            paint.Color = global::Android.Graphics.Color.LightGray;
            paint.AntiAlias = true;
            //paint.Dither = true;
            //paint.SetShader(new RadialGradient(50, 40, 100, global::Android.Graphics.Color.Transparent, global::Android.Graphics.Color.LightGray, Shader.TileMode.Clamp));
            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);

            path.MoveTo(0, height);
            path.LineTo(width / 2, 0);
            path.LineTo(width, height);
            path.LineTo(0, height);
            path.Close();


            //Arrow
            Paint paint2 = new Paint();
            paint2.Color = global::Android.Graphics.Color.White;
            paint2.AntiAlias = true;
            Path path2 = new Path();
            path2.SetFillType(Path.FillType.EvenOdd);


            path2.MoveTo(shadowThikness, height);
            path2.LineTo(width / 2, shadowThikness);
            path2.LineTo(width - shadowThikness, height);
            path2.Close();

            //canvas.DrawPath(path, paint);
            canvas.DrawPath(path2, paint2);
        }

        private void drawArrowRight(Canvas canvas)
        {
            var width = arrow.Width;
            var height = arrow.Height;

            //Shadow arrow
            Paint paint = new Paint();
            paint.Color = global::Android.Graphics.Color.LightGray;
            paint.AntiAlias = true;
            //paint.Dither = true;
            //paint.SetShader(new RadialGradient(50, 40, 100, global::Android.Graphics.Color.Transparent, global::Android.Graphics.Color.LightGray, Shader.TileMode.Clamp));
            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);


            path.MoveTo(0, 0);
            path.LineTo(width, height / 2);
            path.LineTo(0, height);
            path.Close();


            //Arrow
            Paint paint2 = new Paint();
            paint2.Color = global::Android.Graphics.Color.White;
            paint2.AntiAlias = true;
            Path path2 = new Path();
            path2.SetFillType(Path.FillType.EvenOdd);


            path2.MoveTo(0, shadowThikness);
            path2.LineTo(width - shadowThikness, height / 2);
            path2.LineTo(0, height - shadowThikness);
            path2.Close();

            canvas.DrawPath(path, paint);
            canvas.DrawPath(path2, paint2);
        }

        private void drawArrowLeft(Canvas canvas)
        {
            var width = arrow.Width;
            var height = arrow.Height;

            //Shadow arrow
            Paint paint = new Paint();
            paint.Color = global::Android.Graphics.Color.LightGray;
            paint.AntiAlias = true;
            //paint.Dither = true;
            //paint.SetShader(new RadialGradient(50, 40, 100, global::Android.Graphics.Color.Transparent, global::Android.Graphics.Color.LightGray, Shader.TileMode.Clamp));
            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);


            path.MoveTo(width, 0);
            path.LineTo(0, height / 2);
            path.LineTo(width, height);
            path.Close();


            //Arrow
            Paint paint2 = new Paint();
            paint2.Color = global::Android.Graphics.Color.White;
            paint2.AntiAlias = true;
            Path path2 = new Path();
            path2.SetFillType(Path.FillType.EvenOdd);


            path2.MoveTo(width, shadowThikness);
            path2.LineTo(shadowThikness, height / 2);
            path2.LineTo(width, height - shadowThikness);
            path2.Close();

            canvas.DrawPath(path, paint);
            canvas.DrawPath(path2, paint2);
        }

    }

    public enum ArrowTypeEnum
    {
        Up,
        Down,
        Left,
        Right
    }
}
