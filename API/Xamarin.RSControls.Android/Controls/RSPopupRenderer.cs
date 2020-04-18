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
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float BorderRadius { get; set; }
        public Forms.Color BorderFillColor { get; set; }
        public float DimAmount { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Forms.View RelativeView { get; set; }
        public Forms.View CustomView { get; set; }
        private LinearLayout customLayout;
        private LinearLayout contentView;
        public bool ShadowEnabled { get; set; }
        private RSAndroidButton positiveButton;
        private RSAndroidButton neutralButton;
        private RSAndroidButton destructiveButton;


        public RSPopupRenderer()
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as LinearLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
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
            Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);

            if (CustomView != null)
            {
                SetCustomView();
            }


            SetDialog();
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

        //Action bar height
        private int OffsetY()
        {
            var height = 0;

            Resources resources = Context.Resources;
            int resourceId = resources.GetIdentifier("navigation_bar_height_landscape", "dimen", "android");
            if (resourceId > 0)
            {
                height = resources.GetDimensionPixelSize(resourceId);
            }

            return height;
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

            //Width
            if (customLayout.MeasuredWidth > minWidth && customLayout.MeasuredWidth < metrics.WidthPixels * 0.9)
                attrs.Width = customLayout.MeasuredWidth;
            else if (customLayout.MeasuredWidth > metrics.WidthPixels * 0.9)
                attrs.Width = (int)(metrics.WidthPixels * 0.9);
            else
                attrs.Width = minWidth;




            //Position




            //attrs.X = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)RelativeView.Bounds.X, Context.Resources.DisplayMetrics));
            //var mainDisplayInfo = Essentials.DeviceDisplay.MainDisplayInfo;
            //var density = mainDisplayInfo.Density;

            //var y = GetScreenCoordinates(RelativeView);

            //attrs.Y = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)y.Y + 56, Context.Resources.DisplayMetrics));


            if (RelativeView != null)
            {
                //Set the gravity top and left so it starts at real 0 coordinates than aply position
                //Dialog.Window.SetGravity(GravityFlags.Top | GravityFlags.Left);

                var nativeView = Xamarin.Forms.Platform.Android.Platform.GetRenderer(RelativeView).View;

                int[] location = new int[2];
                nativeView.GetLocationOnScreen(location);
                //attrs.Gravity = nativeView.ForegroundGravity;
                //attrs.X = (int)location[0];
                //attrs.Y = (int)location[1];
                //attrs.Width = (int)RelativeView.Width;
                //attrs.Height = (int)RelativeView.Height;

                var dpp = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)(RelativeView.Height / 2), Context.Resources.DisplayMetrics);
                //attrs.Y += (int)(dpp / 2);

                //nativeView.IsPaddingRelative.ToString();
                //nativeView.PaddingBottom.ToString();
                //nativeView.PaddingTop.ToString();
                //nativeView.PaddingStart.ToString();
                //nativeView.PaddingEnd.ToString();
                var y1 = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)RelativeView.Bounds.Y + (float)RelativeView.Height + 56, Context.Resources.DisplayMetrics));
                //attrs.Y = y1;

                Rect rectfLocal = new Rect();

                //For coordinates location relative to the parent
                nativeView.GetLocalVisibleRect(rectfLocal);

                Rect rectfGlobal = new Rect();

                //For coordinates location relative to the screen/display
                nativeView.GetGlobalVisibleRect(rectfGlobal);

                rectfLocal.ToString();

                var ppp = GetScreenCoordinates(RelativeView);

                var off = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)(OffsetY()), Context.Resources.DisplayMetrics);
                attrs.X = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)(ppp.X), Context.Resources.DisplayMetrics);
                attrs.Y = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)(ppp.Y) + off, Context.Resources.DisplayMetrics);
                attrs.Gravity = nativeView.ForegroundGravity;
            }


            attrs.DimAmount = this.DimAmount;
            var lololol = attrs.VerticalMargin;
            //Set new attributes
            this.Dialog.Window.Attributes = attrs;
        }

        public (double X, double Y) GetScreenCoordinates(VisualElement view)
        {
            // A view's default X- and Y-coordinates are LOCAL with respect to the boundaries of its parent,
            // and NOT with respect to the screen. This method calculates the SCREEN coordinates of a view.
            // The coordinates returned refer to the top left corner of the view.

            // Initialize with the view's "local" coordinates with respect to its parent
            double screenCoordinateX = view.X;
            double screenCoordinateY = view.Y;

            // Get the view's parent (if it has one...)
            //if (view.Parent.GetType() != typeof(App))
            {
                VisualElement parent = (VisualElement)view.Parent;


                // Loop through all parents
                while (parent != null)
                {
                    // Add in the coordinates of the parent with respect to ITS parent
                    screenCoordinateX += parent.X;
                    screenCoordinateY += parent.Y;

                    // If the parent of this parent isn't the app itself, get the parent's parent.
                    if (parent.Parent != null && parent.Parent is VisualElement)
                        parent = (VisualElement)parent.Parent;
                    else
                        parent = null;
                }
            }

            // Return the final coordinates...which are the global SCREEN coordinates of the view
            return (screenCoordinateX, screenCoordinateY);
        }

        public int getStatusBarHeight()
        {
            int result = 0;
            int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                result = Resources.GetDimensionPixelSize(resourceId);
            }
            return result;
        }

        public float convertPxToDp(Context context, float px)
        {
            return px / context.Resources.DisplayMetrics.Density;
        }


        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            //PaintDrawable paintDrawable = new PaintDrawable(BorderFillColor.ToAndroid());
            //paintDrawable.Paint.AntiAlias = true;
            //paintDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 60, Context.Resources.DisplayMetrics));
            //var shadowRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 120, Context.Resources.DisplayMetrics);
            //paintDrawable.Paint.SetShadowLayer(shadowRadius, 0f, 0f, global::Android.Graphics.Color.Gray);

            //Manipulate color and roundness of border
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.SetColor(BorderFillColor.ToAndroid());

            gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));

            InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, 0, 0, 0, 0); //Adds margin to alert

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
            if(rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
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


    //Custom button used to assign command
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

            if (button.Command != null)
                button.Command.Execute(null);

            if (button.ClickHandler != null)
                button.ClickHandler.Invoke(button, EventArgs.Empty);

            if (button.Id.Equals(Resource.Id.action_positive))
                this.dialog.Dialog.Dismiss();
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
