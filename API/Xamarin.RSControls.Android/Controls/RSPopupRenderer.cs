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
        private global::Android.Views.View relativeViewAsNativeView;
        public Forms.View CustomView { get; set; }
        private global::Android.Widget.RelativeLayout customLayout;
        private LinearLayout contentView;
        private LinearLayout linearLayout;
        private LinearLayout buttonsLayout;
        private global::Android.Views.View arrow;
        private global::Android.Graphics.Point arrowSize;
        public RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
        public bool ShadowEnabled { get; set; }
        public bool IsModal { get; set; }
        private RSAndroidButton positiveButton;
        private RSAndroidButton neutralButton;
        private RSAndroidButton destructiveButton;
        private global::Android.Content.Res.Orientation Orientation;
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

            Orientation = Context.Resources.Configuration.Orientation;

            //Fix for Control that call keyboard like entry (keyboard not showing up)
            Dialog.Window.ClearFlags(WindowManagerFlags.NotFocusable | WindowManagerFlags.AltFocusableIm);
            //Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);


            if (CustomView != null)
            {
                SetCustomView();
            }

            SetDialog();


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

                    widthSpec = Width;
                }
                else
                {
                    if (Width == -2)
                        widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Resources.DisplayMetrics.WidthPixels, MeasureSpecMode.AtMost);
                    else
                        widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Resources.DisplayMetrics.WidthPixels, MeasureSpecMode.Exactly);
                }

                if (Height != -2 && Height != -1)
                {
                    Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Height, Context.Resources.DisplayMetrics);

                    //Fix if height user inputs greater than creen height
                    if (Height > metrics.HeightPixels - dialogVerticalMargin)
                        Height = metrics.HeightPixels - dialogVerticalMargin;

                    heightSpec = Height;

                }
                else
                {
                    if(Height == -2)
                        heightSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Height, MeasureSpecMode.AtMost);
                    else
                        heightSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec(Resources.DisplayMetrics.HeightPixels, MeasureSpecMode.Exactly);
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
                int x = 0;
                int y = 0;


                relativeViewAsNativeView = Xamarin.Forms.Platform.Android.Platform.GetRenderer(formsView).View;
                Rect rectf = new Rect();
                relativeViewAsNativeView.GetWindowVisibleDisplayFrame(rectf);
                int[] locationScreen = new int[2];
                relativeViewAsNativeView.GetLocationOnScreen(locationScreen);



                var relativeViewWidth = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Width, Context.Resources.DisplayMetrics));
                var relativeViewHeight = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Height, Context.Resources.DisplayMetrics));


                //X
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    x = locationScreen[0] - rectf.Left - dialogHorizontalMargin - arrowSize.Y;
                    y = locationScreen[1] - rectf.Top - dialogVerticalMargin + relativeViewHeight / 2 - arrowSize.X / 2 ;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    x = locationScreen[0] - rectf.Left - dialogHorizontalMargin + relativeViewWidth;
                    y = locationScreen[1] - rectf.Top - dialogVerticalMargin + relativeViewHeight / 2 - arrowSize.X / 2;
                }


                //Y
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    y = locationScreen[1] - rectf.Top - dialogVerticalMargin - arrowSize.Y;
                    x = locationScreen[0] - rectf.Left - dialogHorizontalMargin + relativeViewWidth / 2 - arrowSize.X / 2;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                {
                    y = locationScreen[1] + relativeViewHeight - rectf.Top - dialogVerticalMargin;
                    x = locationScreen[0] - rectf.Left - dialogHorizontalMargin + relativeViewWidth / 2 - arrowSize.X / 2;
                }



                //Set position
                PositionX = x;
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
            this.contentView.AddView(convertView);
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

            SetBackground();
            SetCustomLayout();


            //Popup size
            SetPopupSize(metrics);

            //Apply size
            attrs.Width = ViewGroup.LayoutParams.MatchParent;
            attrs.Height = ViewGroup.LayoutParams.MatchParent;

            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = Width;
            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;
            linearLayout.ViewTreeObserver.AddOnGlobalLayoutListener(this);
            linearLayout.LayoutChange += LinearLayout_LayoutChange;

            arrowSize = new global::Android.Graphics.Point(0, 0);
            if (RelativeView != null)
            {
                arrowSize.X = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics));
                arrowSize.Y = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics));
            }

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


            //Set dim amount
            attrs.DimAmount = this.DimAmount;
            
            //Set new attributes
            this.Dialog.Window.Attributes = attrs;
        }

        private void LinearLayout_LayoutChange(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
        {
            if (RelativeView != null)
            {
                SetPopupPositionRelativeTo(RelativeView, Resources.DisplayMetrics);
                setArrow();
                SetLinearLayoutPosition();
            }

            //Buttons
            if (buttonsLayout.Visibility == ViewStates.Visible)
            {
                if (buttonsLayout.Width < linearLayout.Width)
                {
                    (buttonsLayout.LayoutParameters as LinearLayout.LayoutParams).Width = ViewGroup.LayoutParams.MatchParent;
                    (buttonsLayout.LayoutParameters as LinearLayout.LayoutParams).Height = ViewGroup.LayoutParams.WrapContent;
                }
                else
                {
                    (buttonsLayout.LayoutParameters as LinearLayout.LayoutParams).Width = ViewGroup.LayoutParams.WrapContent;
                    (buttonsLayout.LayoutParameters as LinearLayout.LayoutParams).Height = ViewGroup.LayoutParams.WrapContent;
                }
            }
        }

        private void SetLinearLayoutPosition()
        {
            //Left Right
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                if (linearLayout.MeasuredWidth > PositionX - arrowSize.X)
                {
                    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = PositionX - arrowSize.X;
                    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;
                }

                //correctedX();
                linearLayout.SetX(arrow.GetX() - linearLayout.MeasuredWidth + 1);
                linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayout.Height / 2);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                if (linearLayout.MeasuredWidth > customLayout.Width - PositionX - arrowSize.X)
                {
                    var lol = customLayout.Width - PositionX - arrowSize.X;
                    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = customLayout.Width - PositionX - arrowSize.X;
                    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;
                }
                //else
                //{
                //    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = Width;
                //    (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;
                //}

                //correctedX();
                linearLayout.SetX(arrow.GetX() + arrowSize.Y - 1);
                linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayout.Height / 2);
            }


           //Top Bottom
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                correctedX();
                linearLayout.SetY(arrow.GetY() - linearLayout.MeasuredHeight + 1);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                correctedX();
                linearLayout.SetY(arrow.GetY() + arrowSize.Y - 1);
            }
        }

        private void correctedX()
        {
            if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {

                }
                else
                {

                }
            }
            else
            {
                if (arrow.GetX() > (Resources.DisplayMetrics.WidthPixels - dialogHorizontalMargin) / 2)
                {
                    if ((arrow.GetX() + arrowSize.X / 2 + linearLayout.MeasuredWidth / 2) > Resources.DisplayMetrics.WidthPixels - dialogHorizontalMargin)
                    {
                        var offset = (arrow.GetX() + arrowSize.X / 2 + linearLayout.MeasuredWidth / 2) - Resources.DisplayMetrics.WidthPixels;
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2 - offset);
                    }
                    else
                    {
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2);
                    }
                }
                else if (arrow.GetX() < (Resources.DisplayMetrics.WidthPixels - dialogHorizontalMargin) / 2)
                {
                    if ((arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2) < dialogHorizontalMargin)
                    {
                        var offset = (arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2) + dialogHorizontalMargin;
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2 - offset);
                    }
                    else
                    {
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2);
                    }
                }
                else
                {
                    linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2);
                }
            }
        }

        private void setArrow()
        {
            arrow.SetBackground(new CustomArrow(arrow, RSPopupPositionSideEnum, Context));
            //arrow.Background = new CustomArrow(arrow, RSPopupPositionSideEnum, Context);

            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.X, arrowSize.Y);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.X;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.Y;
                arrow.SetX(PositionX );
                arrow.SetY(PositionY);
                arrow.Rotation = 180;
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.X, arrowSize.Y);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.X;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.Y;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
            }
            else if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.Y;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.X;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
            }
            else if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.Y;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.X;
                arrow.SetX(PositionX );
                arrow.SetY(PositionY);                
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
            //InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, dialogHorizontalMargin, dialogVerticalMargin, dialogHorizontalMargin, dialogVerticalMargin); //Adds margin to alert
            linearLayout.SetBackground(gradientDrawable);


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


            //SetDialog();
        }

        public void OnClick(global::Android.Views.View v)
        {
            if (v.Id == customLayout.Id && !IsModal)
                this.Dialog.Dismiss();
        }

        public void OnGlobalLayout()
        {
            //if(linearLayout.ViewTreeObserver.IsAlive)
            //    linearLayout.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);


            //if (RelativeView != null)
            //{
            //    SetPopupPositionRelativeTo(RelativeView, Resources.DisplayMetrics);
            //    setArrow();
            //    SetLinearLayoutPosition();

            //}


            //if (buttonsLayout.Visibility == ViewStates.Visible)
            //{
            //    if (buttonsLayout.Width < linearLayout.Width)
            //        buttonsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //    else
            //        buttonsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            //}
        }

        //Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //Force dispose so we can remove CanExecuteChanged event on command in buttons dispose method
            positiveButton?.Dispose();
            neutralButton?.Dispose();
            destructiveButton?.Dispose();
            linearLayout.LayoutChange -= LinearLayout_LayoutChange;
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
        private RSPopupPositionSideEnum rSPopupPositionSideEnum;
        private float shadowThikness;

        public CustomArrow(global::Android.Views.View arrow, RSPopupPositionSideEnum rSPopupPositionSideEnum, Context context) : base()
        {
            this.arrow = arrow;
            this.rSPopupPositionSideEnum = rSPopupPositionSideEnum;

            if(rSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, context.Resources.DisplayMetrics);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1.5f, context.Resources.DisplayMetrics);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1f, context.Resources.DisplayMetrics);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                this.shadowThikness = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1f, context.Resources.DisplayMetrics);
        }

        public override int Opacity => 1;

        public override void Draw(Canvas canvas)
        {
            if(rSPopupPositionSideEnum == RSPopupPositionSideEnum.Top || rSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                drawArrow(canvas);
            else if(rSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                drawArrowRight(canvas);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
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

            canvas.DrawPath(path, paint);
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
}
