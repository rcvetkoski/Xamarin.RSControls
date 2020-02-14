using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Interfaces;

//[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public RSPopupRenderer(Context context)
        {
            
        }


        public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Context);
            TextView title = new TextView(Context);
            title.Text = "Title";
            title.SetPadding(10, 10, 10, 10);
            title.Gravity = GravityFlags.Center;
            title.TextSize = 16;
            title.SetTextColor(global::Android.Graphics.Color.Black);
            title.SetTypeface(title.Typeface, global::Android.Graphics.TypefaceStyle.Bold);
            builder.SetCustomTitle(title);
            builder.SetMessage("Message");


            //builder.SetPositiveButton(Resource.Id.ok_action, new EventHandler<Android.Content.DialogClickEventArgs>(ButtonDone));
            //builder.SetNegativeButton(Resource.Id.ok_action, new EventHandler<Android.Content.DialogClickEventArgs>(ButtonDone));

            return builder.Create();
        }

        public void ShowPopup()
        {
            //Check if it is already added


            if (!this.IsAdded)
                this.Show(this.FragmentManager, "sc");
        }

        public override void OnStart()
        {
            base.OnStart();

            SetDialog();
        }



        private void SetDialog()
        {
            var attrs = this.Dialog.Window.Attributes;

            var metrics = Resources.DisplayMetrics;
            var widthScreen = metrics.WidthPixels * 0.4;

            //Set the gravity top and left so it starts at real 0 coordinates than aply position
            Dialog.Window.SetGravity(GravityFlags.Top | GravityFlags.Left);
            attrs.X = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionX, Context.Resources.DisplayMetrics);
            attrs.Y = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)PositionY, Context.Resources.DisplayMetrics);

            //Dialog width
            attrs.Width = (int)widthScreen;

            //Background 
            attrs.DimAmount = 0;

            //Set new attributes
            this.Dialog.Window.Attributes = attrs;

            ////Custom background so we can set border radius shadow ...
            //PaintDrawable paintDrawable = new PaintDrawable(global::Android.Graphics.Color.White);
            //paintDrawable.SetCornerRadius(38);
            //paintDrawable.Paint.SetShadowLayer(TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics), 0.5f, 0.5f, Android.Graphics.Color.Gray);
            //Dialog.Window.SetBackgroundDrawable(paintDrawable);


            ////Custom layout for dioalog
            //LinearLayout layout = LayoutInflater.From(Context).Inflate(Resource.Layout.custom_dialog_view, null) as LinearLayout;
            //global::Android.Widget.Button positiveButton = layout.FindViewById<global::Android.Widget.Button>(Resource.Id.ok_action);
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

            //SetDialog();
        }
    }
}
