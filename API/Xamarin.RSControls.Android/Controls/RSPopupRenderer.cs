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
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Interfaces;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float BorderRadius { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool ShadowEnabled { get; set; }


        public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            global::Android.Support.V7.App.AlertDialog.Builder builder = new global::Android.Support.V7.App.AlertDialog.Builder(Context);
            //TextView title = new TextView(Context);
            //title.Text = this.Title;
            //title.SetPadding(10, 10, 10, 10);
            //title.Gravity = GravityFlags.Center;
            //title.TextSize = 16;
            //title.SetTextColor(global::Android.Graphics.Color.Black);
            //title.SetTypeface(title.Typeface, global::Android.Graphics.TypefaceStyle.Bold);
            //builder.SetCustomTitle(title);
            builder.SetTitle(Title);
            builder.SetMessage(this.Message);


            //builder.SetPositiveButton(Resource.Id.ok_action, new EventHandler<Android.Content.DialogClickEventArgs>(ButtonDone));
            //builder.SetNegativeButton(Resource.Id.ok_action, new EventHandler<Android.Content.DialogClickEventArgs>(ButtonDone));

            return builder.Create();
        }

        public void ShowPopup()
        {
            //Check if it is already added & ANDROID context not null
            if (!this.IsAdded && RSAppContext.RSContext != null)
                this.Show(((AppCompatActivity)RSAppContext.RSContext).SupportFragmentManager, "sc");
        }

        public override void OnStart()
        {
            base.OnStart();

            SetDialog();
        }

        //Action bar height
        private int OffsetY()
        {
            int resourceId = Resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            //var id = Resources.GetIdentifier("status_bar_height", "dimen", "android");
            var val = Context.Resources.GetDimensionPixelSize(resourceId);
            return val;
        }


        private void SetDialog()
        {
            var attrs = this.Dialog.Window.Attributes;

            var metrics = Resources.DisplayMetrics;
            var widthScreen = metrics.WidthPixels * 0.6;

            //Set the gravity top and left so it starts at real 0 coordinates than aply position
            Dialog.Window.SetGravity(GravityFlags.Top | GravityFlags.Left);
            attrs.X = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionX, Context.Resources.DisplayMetrics);
            attrs.Y = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionY, Context.Resources.DisplayMetrics) + OffsetY();

            //Dialog width
            attrs.Width = (int)widthScreen;
            attrs.Height = 400;
            //Background 
            attrs.DimAmount = 0.5f;

            //Set new attributes
            this.Dialog.Window.Attributes = attrs;

            //Custom background so we can set border radius shadow ...
            PaintDrawable paintDrawable = new PaintDrawable(global::Android.Graphics.Color.White);
            paintDrawable.SetCornerRadius(38);
            paintDrawable.Paint.SetShadowLayer(TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics), 0.5f, 0.5f, global::Android.Graphics.Color.Gray);
            Dialog.Window.SetBackgroundDrawable(paintDrawable);


            ////Custom layout for dioalog
            //LinearLayout layout = LayoutInflater.From(Context).Inflate(Resource.Layout.custom_dialog_view, null) as LinearLayout;
            //Android.Widget.Button positiveButton = layout.FindViewById<Android.Widget.Button>(Resource.Id.ok_action);
            //positiveButton.Click += PositiveButton_Click;
            //Dialog.SetContentView(layout);
        }

        private void PositiveButton_Click(object sender, EventArgs e)
        {
            this.Dialog.Dismiss();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            SetDialog();
        }
    }
}
