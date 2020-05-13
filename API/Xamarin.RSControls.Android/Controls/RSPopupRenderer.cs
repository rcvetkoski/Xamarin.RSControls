using System;
using System.Reflection;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
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

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup, IDisposable
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
        private LinearLayout customLayout;
        private LinearLayout contentView;
        private LinearLayout linearLayout;
        private LinearLayout buttonsLayout;
        public bool ShadowEnabled { get; set; }
        private RSAndroidButton positiveButton;
        private RSAndroidButton neutralButton;
        private RSAndroidButton destructiveButton;
        private int dialogHorizontalMargin = 0;
        private int dialogVerticalMargin = 50;



        public RSPopupRenderer()
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as LinearLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
            linearLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.linearLayout);
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
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
                //Width
                //if (customLayout.MeasuredWidth > minWidth && customLayout.MeasuredWidth < metrics.WidthPixels * 0.9)
                //    attrs.Width = customLayout.MeasuredWidth;
                //else if (customLayout.MeasuredWidth > metrics.WidthPixels * 0.9)
                //    attrs.Width = (int)(metrics.WidthPixels * 0.9);
                //else
                //    attrs.Width = minWidth;




                if(Width != -2 && Width != -1)
                {
                    Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Width, Context.Resources.DisplayMetrics);

                    //Fix if width user inputs greater than creen width
                    if (Width > metrics.WidthPixels - dialogHorizontalMargin)
                        Width = metrics.WidthPixels - dialogHorizontalMargin;
                }

                if (Height != -2 && Height != -1)
                {
                    Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Height, Context.Resources.DisplayMetrics);

                    //Fix if height user inputs greater than creen height
                    if (Height > metrics.HeightPixels - dialogVerticalMargin)
                        Height = metrics.HeightPixels - dialogVerticalMargin;
                }
            }
            else
            {

                Width = ViewGroup.LayoutParams.WrapContent;
                Height = ViewGroup.LayoutParams.WrapContent;
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


                global::Android.Graphics.Point size = new global::Android.Graphics.Point();
                (Context as AppCompatActivity).WindowManager.DefaultDisplay.GetRealSize(size);


                int y = 0;

                int measuredHeight = 0;
                if (Height == -2 || Height == -1)
                    measuredHeight = customLayout.MeasuredHeight;
                else
                    measuredHeight = Height;

                int pos = (locationScreen[1] + dialogVerticalMargin + relativeViewHeight + measuredHeight);
                int fixedHeight = (locationScreen[1] - rectf.Top - dialogVerticalMargin - measuredHeight);
                if (metrics.HeightPixels < pos && fixedHeight > 0)
                    y = fixedHeight;
                else
                    y = locationScreen[1] - rectf.Top + relativeViewHeight - dialogVerticalMargin;


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


            customLayout.Measure(metrics.WidthPixels, metrics.HeightPixels);


            //Popup size
            SetPopupSize(metrics);
            
            //Apply size
            attrs.Width = Width;
            attrs.Height = Height;


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

            //Apply position
            attrs.X = PositionX;
            attrs.Y = PositionY;
            
            //Set dim amount
            attrs.DimAmount = this.DimAmount;
            
            //Set new attributes
            this.Dialog.Window.Attributes = attrs;
        }

        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            //Manipulate color and roundness of border
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.SetColor(BorderFillColor.ToAndroid());
            gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));

            //GradientDrawable gradientDrawable2 = new GradientDrawable();
            //gradientDrawable2.SetColor(global::Android.Graphics.Color.Transparent);
            //linearLayout.SetBackground(gradientDrawable);

            InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, dialogHorizontalMargin, dialogVerticalMargin, dialogHorizontalMargin, dialogVerticalMargin); //Adds margin to alert


            Dialog.Window.SetBackgroundDrawable(insetDrawable);
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
}
