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
            //Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);

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

        //Navigation bar height
        private int GetNavigationBarHeight()
        {
            var height = 0;

            Resources resources = Context.Resources;
            int resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                height = resources.GetDimensionPixelSize(resourceId);
            }

            return height;
        }

        //Status bar height
        public int GetStatusBarHeight()
        {
            int result = 0;
            int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                result = Resources.GetDimensionPixelSize(resourceId);
            }
            return result;
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
            //if (customLayout.MeasuredWidth > minWidth && customLayout.MeasuredWidth < metrics.WidthPixels * 0.9)
            //    attrs.Width = customLayout.MeasuredWidth;
            //else if (customLayout.MeasuredWidth > metrics.WidthPixels * 0.9)
            //    attrs.Width = (int)(metrics.WidthPixels * 0.9);
            //else
            //    attrs.Width = minWidth;



            //Position
            if (RelativeView != null)
            {
                //Set the gravity top and left so it starts at real 0 coordinates than aply position
                Dialog.Window.SetGravity(GravityFlags.Left | GravityFlags.Top);

                var nativeView = Xamarin.Forms.Platform.Android.Platform.GetRenderer(RelativeView).View;
                Rect rectf = new Rect();
                nativeView.GetWindowVisibleDisplayFrame(rectf);
                int[] locationScreen = new int[2];
                nativeView.GetLocationOnScreen(locationScreen);
                var relativeViewHeight = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)RelativeView.Height, Context.Resources.DisplayMetrics));


                attrs.X = locationScreen[0] - rectf.Left;
                attrs.Y = locationScreen[1] - rectf.Top + relativeViewHeight - 30;

            }


            //Set dim amount
            attrs.DimAmount = this.DimAmount;


            //Set new attributes
            this.Dialog.Window.Attributes = attrs;
        }


        private int[] findDropDownPosition(global::Android.Views.View anchor, WindowManagerLayoutParams p, int xoff, int yoff)
        {
            p.Flags = WindowManagerFlags.LayoutInScreen;
            int mPopupHeight = 150;
            int mPopupWidth = 250;
            bool mClipToScreen = false;
            bool mAllowScrollingAnchorParent = true;


            int[] mDrawingLocation = new int[2];
            int[] mScreenLocation = new int[2];


            anchor.GetLocationInWindow(mDrawingLocation);
            p.X = mDrawingLocation[0] + xoff;
            p.Y = mDrawingLocation[1] + anchor.Height + yoff;

            bool onTop = false;
            p.Gravity = GravityFlags.Left | GravityFlags.Top;

            anchor.GetLocationOnScreen(mScreenLocation);
            Rect displayFrame = new Rect();
            anchor.GetWindowVisibleDisplayFrame(displayFrame);

            global::Android.Views.View root = anchor.RootView;
            if (p.Y + mPopupHeight > displayFrame.Bottom || p.X + mPopupWidth - root.Width > 0)
            {
                // if the drop down disappears at the bottom of the screen. we try to
                // scroll a parent scrollview or move the drop down back up on top of
                // the edit box
                if (mAllowScrollingAnchorParent)
                {
                    int scrollX = anchor.ScrollX;
                    int scrollY = anchor.ScrollY;
                    Rect r = new Rect(scrollX, scrollY, scrollX + mPopupWidth + xoff,
                            scrollY + mPopupHeight + anchor.Height + yoff);
                    anchor.RequestRectangleOnScreen(r, true);
                }
                // now we re-evaluate the space available, and decide from that
                // whether the pop-up will go above or below the anchor.
                anchor.GetLocationOnScreen(mDrawingLocation);
                p.X = mDrawingLocation[0] + xoff;
                p.Y = mDrawingLocation[1] + anchor.Height + yoff;

                // determine whether there is more space above or below the anchor
                anchor.GetLocationOnScreen(mScreenLocation);

                onTop = (displayFrame.Bottom - mScreenLocation[1] - anchor.Height - yoff) <
                        (mScreenLocation[1] - yoff - displayFrame.Top);
                if (onTop)
                {
                    p.Gravity = GravityFlags.Left | GravityFlags.Bottom;
                    p.Y = root.Height - mDrawingLocation[1] + yoff;
                }
                else
                {
                    p.Y = mDrawingLocation[1] + anchor.Height + yoff;
                }
            }
            if (mClipToScreen)
            {
                int displayFrameWidth = displayFrame.Right - displayFrame.Left;
                int right = p.X + p.Width;
                if (right > displayFrameWidth)
                {
                    p.X -= right - displayFrameWidth;
                }
                if (p.X < displayFrame.Left)
                {
                    p.X = displayFrame.Left;
                    p.Width = Math.Min(p.Width, displayFrameWidth);
                }
                if (onTop)
                {
                    int popupTop = mScreenLocation[1] + yoff - mPopupHeight;
                    if (popupTop < 0)
                    {
                        p.Y += popupTop;
                    }
                }
                else
                {
                    p.Y = Math.Max(p.Y, displayFrame.Top);
                }
            }
            p.Gravity |= GravityFlags.DisplayClipVertical;


            int[] position = new int[2];
            position[0] = p.X;
            position[1] = p.Y;

            return position;
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

            InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, 0, 30, 0, 30); //Adds margin to alert

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


    //public class RSPopupRenderer : IDialogPopup, IDisposable
    //{
    //    public float PositionX { get; set; }
    //    public float PositionY { get; set; }
    //    public float BorderRadius { get; set; }
    //    public Forms.Color BorderFillColor { get; set; }
    //    public float DimAmount { get; set; }
    //    public string Title { get; set; }
    //    public string Message { get; set; }
    //    public Forms.View RelativeView { get; set; }
    //    public Forms.View CustomView { get; set; }
    //    private LinearLayout customLayout;
    //    private LinearLayout contentView;
    //    public bool ShadowEnabled { get; set; }
    //    private RSAndroidButton positiveButton;
    //    private RSAndroidButton neutralButton;
    //    private RSAndroidButton destructiveButton;
    //    private Context context;
    //    private PopupWindow popupWindow;


    //    public RSPopupRenderer()
    //    {
    //        //Set context
    //        context = ((AppCompatActivity)RSAppContext.RSContext);

    //        popupWindow = new PopupWindow(context);

    //        //Inflate custom layout
    //        this.customLayout = LayoutInflater.From(context).Inflate(Resource.Layout.rs_dialog_view, null) as LinearLayout;
    //        this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);


    //    }


    //    //Show popup
    //    public void ShowPopup()
    //    {
    //        if (CustomView != null)
    //        {
    //            SetCustomView();
    //        }


    //        SetDialog();

    //        var nativeView2 = Xamarin.Forms.Platform.Android.Platform.GetRenderer(RelativeView).View;
    //        var lol = findDropDownPosition(nativeView2, new WindowManagerLayoutParams(), 0, 0);


    //        Rect rectf = new Rect();
    //        Rect rectf2 = new Rect();
    //        Rect rectf3 = new Rect();


    //        //For coordinates location relative to the parent
    //        nativeView2.GetLocalVisibleRect(rectf);

    //        //For coordinates location relative to the screen/display
    //        nativeView2.GetGlobalVisibleRect(rectf2);

    //        nativeView2.GetWindowVisibleDisplayFrame(rectf3);


    //        int[] location = new int[2];
    //        nativeView2.GetLocationInWindow(location);

    //        int[] locationScreen = new int[2];
    //        nativeView2.GetLocationOnScreen(locationScreen);



    //        if (RelativeView != null)
    //        {
    //            var nativeView = Xamarin.Forms.Platform.Android.Platform.GetRenderer(RelativeView).View;
    //            popupWindow.ShowAtLocation((context as AppCompatActivity).Window.DecorView, GravityFlags.Left | GravityFlags.Top, locationScreen[0], 0);
    //        }
    //        else
    //            popupWindow.ShowAtLocation((context as AppCompatActivity).Window.DecorView, GravityFlags.Center, 0, 0);



    //    }

    //    //Set and add custom view 
    //    private void SetCustomView()
    //    {
    //        var renderer = Platform.CreateRendererWithContext(CustomView, context);
    //        Platform.SetRenderer(CustomView, renderer);
    //        var convertView = new Extensions.ViewCellContainer(context, CustomView, renderer);

    //        this.contentView.AddView(convertView);
    //    }

    //    //Set native view 
    //    public void SetNativeView(global::Android.Views.View nativeView)
    //    {
    //        this.contentView.AddView(nativeView);
    //    }

    //    //Set dialog properties
    //    private void SetDialog()
    //    {
    //        SetBackground();
    //        SetCustomLayout();

    //        //Set dim amount
    //        //popupWindow = this.DimAmount;

    //        popupWindow.Focusable = true;
    //        //popupWindow.Update();


    //        var metrics = context.Resources.DisplayMetrics;
    //        var minWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 270, context.Resources.DisplayMetrics);
    //        var minHeigth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 150, context.Resources.DisplayMetrics);


    //        //popupWindow.Width = minWidth;
    //        //popupWindow.Height = minHeigth;

    //        popupWindow.Width = ViewGroup.LayoutParams.WrapContent;
    //        popupWindow.Height = ViewGroup.LayoutParams.WrapContent;

    //    }


    //    //Custom background so we can set border radius shadow ...
    //    private void SetBackground()
    //    {
    //        //Manipulate color and roundness of border
    //        GradientDrawable gradientDrawable = new GradientDrawable();
    //        gradientDrawable.SetColor(global::Android.Graphics.Color.White);

    //        gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, context.Resources.DisplayMetrics));

    //        InsetDrawable insetDrawable = new InsetDrawable(gradientDrawable, 0, 30, 0, 30); //Adds margin to alert

    //        popupWindow.SetBackgroundDrawable(insetDrawable);
    //        //popupWindow.Elevation = 40;
    //    }

    //    //Custom layout for dialog
    //    private void SetCustomLayout()
    //    {
    //        global::Android.Widget.TextView title = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_title);
    //        title.Text = this.Title;
    //        if (string.IsNullOrEmpty(title.Text))
    //            title.Visibility = ViewStates.Gone;
    //        global::Android.Widget.TextView message = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_message);
    //        message.Text = this.Message;
    //        if (string.IsNullOrEmpty(message.Text))
    //            message.Visibility = ViewStates.Gone;

    //        popupWindow.ContentView = customLayout;
    //    }

    //    //Buttons
    //    public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter)
    //    {
    //        if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
    //        {
    //            positiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_positive);
    //            positiveButton.dialog = popupWindow;
    //            positiveButton.CommandParameter = commandParameter;
    //            positiveButton.Command = command;
    //            positiveButton.Text = title;
    //            positiveButton.Visibility = ViewStates.Visible;
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
    //        {
    //            neutralButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_neutral);
    //            neutralButton.dialog = popupWindow;
    //            neutralButton.CommandParameter = commandParameter;
    //            neutralButton.Command = command;
    //            neutralButton.Text = title;
    //            neutralButton.Visibility = ViewStates.Visible;
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
    //        {
    //            destructiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_destructive);
    //            destructiveButton.dialog = popupWindow;
    //            destructiveButton.CommandParameter = commandParameter;
    //            destructiveButton.Command = command;
    //            //destructiveButton.SetTextColor(global::Android.Graphics.Color.Red);
    //            destructiveButton.Text = title;
    //            destructiveButton.Visibility = ViewStates.Visible;
    //        }
    //        else
    //        {

    //        }
    //    }
    //    public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, EventHandler handler)
    //    {
    //        if (rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
    //        {
    //            positiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_positive);
    //            positiveButton.ClickHandler = handler;
    //            positiveButton.dialog = popupWindow;
    //            positiveButton.Text = title;
    //            positiveButton.Visibility = ViewStates.Visible;
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
    //        {
    //            neutralButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_neutral);
    //            neutralButton.ClickHandler = handler;
    //            neutralButton.dialog = popupWindow;
    //            neutralButton.Text = title;
    //            neutralButton.Visibility = ViewStates.Visible;
    //        }
    //        else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
    //        {
    //            destructiveButton = customLayout.FindViewById<RSAndroidButton>(Resource.Id.action_destructive);
    //            destructiveButton.ClickHandler = handler;
    //            destructiveButton.dialog = popupWindow;
    //            destructiveButton.Text = title;
    //            destructiveButton.Visibility = ViewStates.Visible;
    //        }
    //        else
    //        {

    //        }
    //    }


    //    //Dismiss
    //    public void Dismiss()
    //    {
    //        popupWindow.Dismiss();
    //        this.Dispose();
    //    }

    //    //Dispose
    //    public void Dispose()
    //    {
    //        //Force dispose so we can remove CanExecuteChanged event on command in buttons dispose method
    //        positiveButton?.Dispose();
    //        neutralButton?.Dispose();
    //        destructiveButton?.Dispose();
    //    }




    //    private int[] findDropDownPosition(global::Android.Views.View anchor, WindowManagerLayoutParams p, int xoff, int yoff)
    //    {
    //        int mPopupHeight = 150;
    //        int mPopupWidth = 250;
    //        bool mClipToScreen = false;
    //        bool mAllowScrollingAnchorParent = true;


    //        int[] mDrawingLocation = new int[2];
    //        int[] mScreenLocation = new int[2];


    //        anchor.GetLocationInWindow(mDrawingLocation);
    //        p.X = mDrawingLocation[0] + xoff;
    //        p.Y = mDrawingLocation[1] + anchor.Height + yoff;

    //        bool onTop = false;
    //        p.Gravity = GravityFlags.Left | GravityFlags.Top;

    //        anchor.GetLocationOnScreen(mScreenLocation);
    //        Rect displayFrame = new Rect();
    //        anchor.GetWindowVisibleDisplayFrame(displayFrame);

    //        global::Android.Views.View root = anchor.RootView;
    //        if (p.Y + mPopupHeight > displayFrame.Bottom || p.X + mPopupWidth - root.Width > 0)
    //        {
    //            // if the drop down disappears at the bottom of the screen. we try to
    //            // scroll a parent scrollview or move the drop down back up on top of
    //            // the edit box
    //            if (mAllowScrollingAnchorParent)
    //            {
    //                int scrollX = anchor.ScrollX;
    //                int scrollY = anchor.ScrollY;
    //                Rect r = new Rect(scrollX, scrollY, scrollX + mPopupWidth + xoff,
    //                        scrollY + mPopupHeight + anchor.Height + yoff);
    //                anchor.RequestRectangleOnScreen(r, true);
    //            }
    //            // now we re-evaluate the space available, and decide from that
    //            // whether the pop-up will go above or below the anchor.
    //            anchor.GetLocationOnScreen(mDrawingLocation);
    //            p.X = mDrawingLocation[0] + xoff;
    //            p.Y = mDrawingLocation[1] + anchor.Height + yoff;

    //            // determine whether there is more space above or below the anchor
    //            anchor.GetLocationOnScreen(mScreenLocation);

    //            onTop = (displayFrame.Bottom - mScreenLocation[1] - anchor.Height - yoff) <
    //                    (mScreenLocation[1] - yoff - displayFrame.Top);
    //            if (onTop)
    //            {
    //                p.Gravity = GravityFlags.Left | GravityFlags.Bottom;
    //                p.Y = root.Height - mDrawingLocation[1] + yoff;
    //            }
    //            else
    //            {
    //                p.Y = mDrawingLocation[1] + anchor.Height + yoff;
    //            }
    //        }
    //        if (mClipToScreen)
    //        {
    //            int displayFrameWidth = displayFrame.Right - displayFrame.Left;
    //            int right = p.X + p.Width;
    //            if (right > displayFrameWidth)
    //            {
    //                p.X -= right - displayFrameWidth;
    //            }
    //            if (p.X < displayFrame.Left)
    //            {
    //                p.X = displayFrame.Left;
    //                p.Width = Math.Min(p.Width, displayFrameWidth);
    //            }
    //            if (onTop)
    //            {
    //                int popupTop = mScreenLocation[1] + yoff - mPopupHeight;
    //                if (popupTop < 0)
    //                {
    //                    p.Y += popupTop;
    //                }
    //            }
    //            else
    //            {
    //                p.Y = Math.Max(p.Y, displayFrame.Top);
    //            }
    //        }
    //        p.Gravity |= GravityFlags.DisplayClipVertical;


    //        int[] position = new int[2];
    //        position[0] = p.X;
    //        position[1] = p.Y;

    //        return position;
    //    }
    //}


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
