using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
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
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup, global::Android.Views.View.IOnClickListener
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
        public bool ShadowEnabled { get; set; }
        private global::Android.Widget.Button positiveButton;
        private global::Android.Widget.Button neutralButton;
        private global::Android.Widget.Button destructiveButton;


        public RSPopupRenderer()
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as LinearLayout;
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

            SetDialog();

            if(CustomView != null)
            {
                SetCustomView();
            }
        }

        public void ShowPopup()
        {
            //Check if it is already added & ANDROID context not null
            if (!this.IsAdded && RSAppContext.RSContext != null)
                this.Show(((AppCompatActivity)RSAppContext.RSContext).SupportFragmentManager, "sc");
        }


        //Action bar height
        private int OffsetY()
        {
            int resourceId = Resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            //var id = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            var val = Context.Resources.GetDimensionPixelSize(resourceId);
            return val;
        }

        //Set and add custom view 
        private void SetCustomView()
        {
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

            //Set the gravity top and left so it starts at real 0 coordinates than aply position
            //Dialog.Window.SetGravity(GravityFlags.Top | GravityFlags.Left);
            //attrs.X = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionX, Context.Resources.DisplayMetrics);
            //attrs.Y = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionY, Context.Resources.DisplayMetrics) + OffsetY();

            customLayout.Measure(metrics.WidthPixels, metrics.HeightPixels);

            //Width
            if(customLayout.MeasuredWidth > minWidth && customLayout.MeasuredWidth < metrics.WidthPixels * 0.9)
                attrs.Width = customLayout.MeasuredWidth;
            else if(customLayout.MeasuredWidth > metrics.WidthPixels * 0.9)
                attrs.Width = (int)(metrics.WidthPixels * 0.9);
            else
                attrs.Width = minWidth;

            //Height
            if (customLayout.MeasuredHeight > minHeigth && customLayout.MeasuredHeight < metrics.HeightPixels * 0.8)
                attrs.Height = customLayout.MeasuredHeight;
            else if (customLayout.MeasuredHeight > metrics.HeightPixels * 0.9)
                attrs.Height = (int)(metrics.HeightPixels * 0.9);
            else
                attrs.Height = minHeigth;


            attrs.DimAmount = this.DimAmount;
            //Set new attributes
            this.Dialog.Window.Attributes = attrs;



        }

        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            PaintDrawable paintDrawable = new PaintDrawable(BorderFillColor.ToAndroid());
            paintDrawable.Paint.AntiAlias = true;
            paintDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));
            var shadowRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
            paintDrawable.Paint.SetShadowLayer(shadowRadius, 0f, 0f, global::Android.Graphics.Color.Gray);
            Dialog.Window.SetBackgroundDrawable(paintDrawable);
        }

        //Custom layout for dialog
        private void SetCustomLayout()
        {
            global::Android.Widget.TextView title = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_title);
            title.Text = this.Title;
            global::Android.Widget.TextView message = customLayout.FindViewById<global::Android.Widget.TextView>(Resource.Id.dialog_message);
            message.Text = this.Message;
            customLayout.RemoveFromParent();
            Dialog.SetContentView(customLayout);
        }

        //Buttons
        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType)
        {
            if(rSPopupButtonType == RSPopupButtonTypeEnum.Positive)
            {
                positiveButton = customLayout.FindViewById<global::Android.Widget.Button>(Resource.Id.action_positive);
                positiveButton.SetOnClickListener(this);
                positiveButton.Text = title;
                positiveButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Neutral)
            {
                neutralButton = customLayout.FindViewById<global::Android.Widget.Button>(Resource.Id.action_neutral);
                neutralButton.Text = title;
                neutralButton.Visibility = ViewStates.Visible;
            }
            else if (rSPopupButtonType == RSPopupButtonTypeEnum.Destructive)
            {
                destructiveButton = customLayout.FindViewById<global::Android.Widget.Button>(Resource.Id.action_destructive);
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

        //Button click
        public void OnClick(global::Android.Views.View v)
        {
            if((v as global::Android.Widget.Button).Id.Equals(Resource.Id.action_positive))
            {
                Dialog.Dismiss();
            }
            if ((v as global::Android.Widget.Button).Id.Equals(Resource.Id.action_neutral))
            {
                Dialog.Dismiss();
            }
        }
    }
}
