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
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Xaml;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::AndroidX.Fragment.App.DialogFragment, IDialogPopup, IDisposable, global::Android.Views.View.IOnClickListener
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool UserSetPosition { get; set; }
        public bool UserSetMargin { get; set; }
        public float BorderRadius { get; set; }
        public Forms.Color BorderFillColor { get; set; }
        public float DimAmount { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Forms.View RelativeView { get; set; }
        public float RSPopupOffsetX { get; set; }
        public float RSPopupOffsetY { get; set; }
        private global::Android.Views.View relativeViewAsNativeView;
        public Forms.View CustomView { get; set; }
        private global::Android.Widget.RelativeLayout customLayout;
        private global::Android.Widget.ImageButton closeButton;
        private LinearLayout contentView;
        private CustomLinearLayout linearLayout;
        private CustomLinearLayout linearLayout2;
        private LinearLayout buttonsLayout;
        public RSPopupAnimationEnum RSPopupAnimationEnum { get; set; }
        public RSPopupPositionEnum RSPopupPositionEnum { get; set; }
        public RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
        public RSPopupStyleEnum RSPopupStyleEnum { get; set; }
        public bool ShadowEnabled { get; set; }
        public bool IsModal { get; set; }
        private RSAndroidButton positiveButton;
        private RSAndroidButton neutralButton;
        private RSAndroidButton destructiveButton;
        private global::Android.Content.Res.Orientation Orientation;
        public int RightMargin { get; set; }
        public int LeftMargin { get; set; }
        public int TopMargin { get; set; }
        public int BottomMargin { get; set; }
        public bool HasCloseButton { get; set; }
        public bool HasArrow { get; set; }
        private float arrowSize;

        public bool canRequestLayout = false;
        private int linearLayoutMinWidth = 0;
        public int screenUsableWidth;
        public int screenUsableHeight;
        private bool backFromSleep;
        private Extensions.ViewCellContainer convertView;

        public RSPopupRenderer()
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as global::Android.Widget.RelativeLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
            linearLayout = customLayout.FindViewById<CustomLinearLayout>(Resource.Id.linearLayout);
            linearLayout2 = customLayout.FindViewById<CustomLinearLayout>(Resource.Id.linearLayout2);
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
            closeButton = customLayout.FindViewById<global::Android.Widget.ImageButton>(Resource.Id.closeButton);

            customLayout.SetOnClickListener(this);
            linearLayout.SetOnClickListener(this);
        }

        //Picker usage
        public RSPopupRenderer(string title, string message)
        {
            this.Title = title;
            this.Message = message;

            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as global::Android.Widget.RelativeLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
            linearLayout = customLayout.FindViewById<CustomLinearLayout>(Resource.Id.linearLayout);
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
            closeButton = customLayout.FindViewById<global::Android.Widget.ImageButton>(Resource.Id.closeButton);

            customLayout.SetOnClickListener(this);
            linearLayout.SetOnClickListener(this);
        }

        public RSPopupRenderer(System.IntPtr intPtr, global::Android.Runtime.JniHandleOwnership jniHandleOwnership) : base(intPtr, jniHandleOwnership)
        {
            //Inflate custom layout
            this.customLayout = LayoutInflater.From(((AppCompatActivity)RSAppContext.RSContext)).Inflate(Resource.Layout.rs_dialog_view, null) as global::Android.Widget.RelativeLayout;
            this.contentView = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.contentView);
            linearLayout = customLayout.FindViewById<CustomLinearLayout>(Resource.Id.linearLayout);
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
            closeButton = customLayout.FindViewById<global::Android.Widget.ImageButton>(Resource.Id.closeButton);

            customLayout.SetOnClickListener(this);
            linearLayout.SetOnClickListener(this);
        }

        public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            if (CustomView != null)
            {
                SetCustomView();
            }

            //global::Android.App.AlertDialog.Builder builder = new global::Android.App.AlertDialog.Builder(Context);
            global::Android.App.AlertDialog.Builder builder = new global::Android.App.AlertDialog.Builder(Context, Resource.Style.RSDialogAnimationTheme);
            return builder.Create();
        }

        public override void OnStart()
        {
            base.OnStart();

            Orientation = Context.Resources.Configuration.Orientation;

            //Fix for Control that call keyboard like entry (keyboard not showing up)
            Dialog.Window.ClearFlags(WindowManagerFlags.NotFocusable | WindowManagerFlags.AltFocusableIm);
            //Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);

            if (!backFromSleep) //Dont want to reset layoutparameters when back from sleep
                SetDialog();

            //linearLayout.Post(() =>
            //{
            //    TranslateAnimation translateAnimation = new TranslateAnimation(0, 0, customLayout.Height - PositionY, 0);
            //    translateAnimation.Duration = 330;
            //    translateAnimation.FillAfter = true;
            //    linearLayout.StartAnimation(translateAnimation);
            //}
            //);
        }

        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            ////Manipulate color and roundness of border
            //GradientDrawable gradientDrawable = new GradientDrawable();
            //gradientDrawable.SetColor(global::Android.Graphics.Color.White);
            //gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));
            //gradientDrawable.SetStroke(1, BorderFillColor.ToAndroid());
            //linearLayout.SetBackground(gradientDrawable);

            RSDrawable rSDrawable = new RSDrawable();
            rSDrawable.ArrowSide = this.RSPopupPositionSideEnum;
            rSDrawable.ArrowSize = this.arrowSize;
            rSDrawable.rSPopupRenderer = this;
            rSDrawable.linearLayout = this.linearLayout;
            rSDrawable.BorderRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics);
            linearLayout.SetBackground(rSDrawable);
            linearLayout.OutlineProvider = new RSViewOutlineProvider(this, TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics), arrowSize)
            {
                ArrowSide = this.RSPopupPositionSideEnum
            };
            // Background
            GradientDrawable transparentDrawable = new GradientDrawable();
            transparentDrawable.SetColor(global::Android.Graphics.Color.Transparent);
            Dialog.Window.SetBackgroundDrawable(transparentDrawable);
        }

        // Needed so we can create custom path shape which tnah will be used for shadow casting
        private class RSViewOutlineProvider : ViewOutlineProvider
        {
            private RSPopupRenderer rSPopupRenderer;
            private float borderRadius;
            public float xConstrainConstant;
            public float yConstrainConstant;
            private float arrowSize;
            public RSPopupPositionSideEnum ArrowSide;
            public Outline Outline { get; set; }

            public RSViewOutlineProvider(RSPopupRenderer rSPopupRenderer, float borderRadius, float arrowSize)
            {
                this.rSPopupRenderer = rSPopupRenderer;
                this.borderRadius = borderRadius;
                this.arrowSize = arrowSize;
            }

            public override void GetOutline(global::Android.Views.View view, Outline outline)
            {
                Outline = outline;
                Path path;

                if (global::Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.R)
                    path = new ForcedConvexPath();
                else
                    path = new Path();

                path.SetFillType(Path.FillType.EvenOdd);

                path.MoveTo(0, 0);

                if (ArrowSide == RSPopupPositionSideEnum.Right && rSPopupRenderer.HasArrow)
                {
                    var posY = (view.Height / 2) - yConstrainConstant;
                    path.AddRoundRect(new RectF(arrowSize, 0, view.Width, view.Height), borderRadius, borderRadius, Path.Direction.Cw);

                    path.MoveTo(arrowSize, posY - arrowSize);
                    path.LineTo(0, posY);
                    path.LineTo(arrowSize, posY + arrowSize);
                }
                else if (ArrowSide == RSPopupPositionSideEnum.Left && rSPopupRenderer.HasArrow)
                {
                    var posY = (view.Height / 2) - yConstrainConstant;
                    path.AddRoundRect(new RectF(0, 0, view.Width - arrowSize, view.Height), borderRadius, borderRadius, Path.Direction.Cw);

                    path.MoveTo(view.Width - arrowSize, posY - arrowSize);
                    path.LineTo(view.Width, posY);
                    path.LineTo(view.Width - arrowSize, posY + arrowSize);
                }
                else if (ArrowSide == RSPopupPositionSideEnum.Bottom && rSPopupRenderer.HasArrow)
                {
                    path.AddRoundRect(new RectF(0, arrowSize, view.Width, view.Height), borderRadius, borderRadius, Path.Direction.Cw);
                    var posX = (view.Width / 2) - xConstrainConstant;


                    path.MoveTo(posX + arrowSize, arrowSize);
                    path.LineTo(posX, 0);
                    path.LineTo(posX - arrowSize, arrowSize);
                }
                else if (ArrowSide == RSPopupPositionSideEnum.Top && rSPopupRenderer.HasArrow)
                {
                    path.AddRoundRect(new RectF(0, 0, view.Width, view.Height - arrowSize), borderRadius, borderRadius, Path.Direction.Cw);
                    var posX = (view.Width / 2) - xConstrainConstant;

                    path.MoveTo(posX + arrowSize, view.Height - arrowSize);
                    path.LineTo(posX, view.Height);
                    path.LineTo(posX - arrowSize, view.Height - arrowSize);
                }
                else
                    path.AddRoundRect(new RectF(0, 0, view.Width, view.Height), borderRadius, borderRadius, Path.Direction.Cw);


                if (global::Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.R)
                    outline.SetConvexPath(path);
                else
                    outline.SetPath(path);
            }
        }

        // Hack so non convex path will still be accepted in outline.SetConvexPath(path). for api < 30
        private class ForcedConvexPath : Path
        {
            public override bool IsConvex => true;
        }

        public void setPadding()
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                linearLayout.SetPadding((int)arrowSize, linearLayout.PaddingTop, linearLayout.PaddingRight, linearLayout.PaddingBottom);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, (int)arrowSize, linearLayout.PaddingBottom);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                linearLayout.SetPadding(linearLayout.PaddingLeft, linearLayout.PaddingTop, linearLayout.PaddingRight, (int)arrowSize);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                linearLayout.SetPadding(linearLayout.PaddingLeft, (int)arrowSize, linearLayout.PaddingRight, linearLayout.PaddingBottom);
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

        //Set dialog properties
        private void SetDialog()
        {
            arrowSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);

            if (RelativeView != null)
                shouldShowArrow();

            if(HasArrow)
                setPadding();

            ////Default value can be changed by user
            //if (!UserSetMargin && RelativeView == null)
            //{
            //    LeftMargin = 20;
            //    TopMargin = 30;
            //    RightMargin = 20;
            //    BottomMargin = 30;
            //}


            Orientation = Resources.Configuration.Orientation;
            linearLayout.rSPopupRenderer = this;
            linearLayout.BorderRadius = TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics);


            LeftMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, LeftMargin, Context.Resources.DisplayMetrics));
            TopMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, TopMargin, Context.Resources.DisplayMetrics));
            RightMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, RightMargin, Context.Resources.DisplayMetrics));
            BottomMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, BottomMargin, Context.Resources.DisplayMetrics));
            customLayout.SetPadding(LeftMargin, TopMargin, RightMargin, BottomMargin);


            var attrs = this.Dialog.Window.Attributes;
            var metrics = Resources.DisplayMetrics;

            SetBackground();
            SetCustomLayout();


            //Popup size
            SetSize(metrics);

            //Apply size
            attrs.Width = ViewGroup.LayoutParams.MatchParent;
            attrs.Height = ViewGroup.LayoutParams.MatchParent;


            //Position
            //Set the gravity top and left so it starts at real 0 coordinates than aply position
            Dialog.Window.SetGravity(GravityFlags.Left | GravityFlags.Top);

            //Set dim amount
            attrs.DimAmount = this.DimAmount;


            //Set new attributes
            this.Dialog.Window.Attributes = attrs;


            if (HasCloseButton)
                setCloseButon();

            if (HasCloseButton)
                closeButton.SetOnClickListener(this);


            customLayout.LayoutChange += CustomLayout_LayoutChange;
        }

        // Set arrow true or false
        private void shouldShowArrow()
        {
            // Width
            if (this.Width == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                    HasArrow = false;
                else
                    HasArrow = true;
            }
            // Height
            else if (this.Height == -1)//Match Parent
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                    HasArrow = false;
                else
                    HasArrow = true;
            }
            else
                HasArrow = true;
        }

        //Set and add custom view 
        private void SetCustomView()
        {
            //ContentPage customViewContentPage = new ContentPage();
            //customViewContentPage.BackgroundColor = Forms.Color.Green;
            //customViewContentPage.Content = CustomView;

            var renderer = Platform.CreateRendererWithContext(CustomView, Context);
            Platform.SetRenderer(CustomView, renderer);
            var sizeRequest = CustomView.Measure(double.PositiveInfinity, double.PositiveInfinity, Forms.MeasureFlags.IncludeMargins);
            var sizeW = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)sizeRequest.Request.Width, ((AppCompatActivity)RSAppContext.RSContext).Resources.DisplayMetrics);
            var sizeH = TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)sizeRequest.Request.Height, ((AppCompatActivity)RSAppContext.RSContext).Resources.DisplayMetrics);
            renderer.View.LayoutParameters = new LinearLayout.LayoutParams((int)sizeW, (int)sizeH);
            renderer.Element.Layout(new Rectangle(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
            this.contentView.AddView(renderer.View);
            renderer.View.Enabled = true;



            //convertView = new Extensions.ViewCellContainer(Context, CustomView, null, 0, this.contentView);

            //var convertView = Extensions.ViewExtensions.ConvertFormsToNative(Context, CustomView, 0, 0, 0, 0);

            //this.contentView.AddView(convertView);
            //convertView.SetBackgroundColor(global::Android.Graphics.Color.Pink);
            // convertView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        }

        //Set native view 
        public void SetNativeView(global::Android.Views.View nativeView)
        {
            this.contentView.AddView(nativeView);
        }

        //Set PopupSize
        private void SetSize(DisplayMetrics metrics)
        {
            // Width
            if (this.Width == -1) //Match Parent
            {
                Width = ViewGroup.LayoutParams.MatchParent;
            }
            else if (this.Width == -2) //Wrap Content
            {
                Width = ViewGroup.LayoutParams.WrapContent;
            }
            else //Raw value user input
            {
                Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Width, Context.Resources.DisplayMetrics);

                ////Fix if width user inputs greater than creen width
                //if (Width > metrics.WidthPixels)
                //    Width = metrics.WidthPixels;
            }


            // Height
            if (this.Height == -1) //Match Parent
            {
                Height = ViewGroup.LayoutParams.MatchParent;
            }
            else if (this.Height == -2) //Wrap Content
            {
                Height = ViewGroup.LayoutParams.WrapContent;
            }
            else //Raw value user input
            {
                Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Height, Context.Resources.DisplayMetrics);

                ////Fix if height user inputs greater than creen height
                //if (Height > metrics.HeightPixels)
                //    Height = metrics.HeightPixels;
            }


            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = Width;
            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;
        }

        //Set Popup position relative to view
        int relativeViewWidth;
        int relativeViewHeight;
        public void SetPopupPositionRelativeTo(Forms.View formsView, DisplayMetrics metrics)
        {
            if (formsView != null)
            {
                int x = 0;
                int y = 0;

                var relativeViewAsNativeRenderer = Xamarin.Forms.Platform.Android.Platform.GetRenderer(formsView);
                relativeViewAsNativeRenderer.UpdateLayout();
                relativeViewAsNativeView = relativeViewAsNativeRenderer.View;
                global::Android.Graphics.Rect rectf = new global::Android.Graphics.Rect();
                relativeViewAsNativeView.GetWindowVisibleDisplayFrame(rectf);
                int[] locationScreen = new int[2];
                relativeViewAsNativeView.GetLocationOnScreen(locationScreen);

                relativeViewWidth = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Width, Context.Resources.DisplayMetrics));
                relativeViewHeight = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Height, Context.Resources.DisplayMetrics));


                //X
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    x = locationScreen[0] - rectf.Left;
                    y = locationScreen[1] - rectf.Top + relativeViewHeight / 2;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    x = locationScreen[0] - rectf.Left + relativeViewWidth;
                    y = locationScreen[1] - rectf.Top + relativeViewHeight / 2;
                }


                //Y
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    x = locationScreen[0] - rectf.Left + relativeViewWidth / 2;
                    y = locationScreen[1] - rectf.Top;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                {
                    x = locationScreen[0] - rectf.Left + relativeViewWidth / 2;
                    y = locationScreen[1] + relativeViewHeight - rectf.Top;
                }

                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
                {
                    y = locationScreen[1] - rectf.Top;
                    x = locationScreen[0] - rectf.Left;
                }

                screenUsableWidth = Resources.DisplayMetrics.WidthPixels - (Resources.DisplayMetrics.WidthPixels - (rectf.Right - rectf.Left));
                screenUsableHeight = Resources.DisplayMetrics.HeightPixels - (Resources.DisplayMetrics.HeightPixels - (rectf.Bottom - rectf.Top));

                //Set position
                PositionX = x + (int)RSPopupOffsetX;
                PositionY = y + (int)RSPopupOffsetY;
            }
        }

        // Set dialog position
        private void setDialogPosition()
        {
            if(RelativeView != null)
            {
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    linearLayout.SetX(PositionX - linearLayout.MeasuredWidth / 2);
                    linearLayout.SetY(PositionY - linearLayout.MeasuredHeight);
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                {
                    linearLayout.SetX(PositionX - linearLayout.MeasuredWidth / 2);
                    linearLayout.SetY(PositionY);
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    linearLayout.SetX(PositionX);
                    linearLayout.SetY(PositionY - linearLayout.Height / 2);
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    linearLayout.SetX(PositionX - linearLayout.MeasuredWidth);
                    linearLayout.SetY(PositionY - linearLayout.Height / 2);
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
                {
                    linearLayout.SetX(PositionX);
                    linearLayout.SetY(PositionY);
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Center)
                {
                    linearLayout.SetX(customLayout.Width / 2 - linearLayout.Width / 2);
                    linearLayout.SetY(customLayout.Height / 2 - linearLayout.Height / 2);
                }
            }
            else
            {
                if (RSPopupPositionEnum == RSPopupPositionEnum.Center)
                {
                    linearLayout.SetX(customLayout.Width / 2 - linearLayout.Width / 2);
                    linearLayout.SetY(customLayout.Height / 2 - linearLayout.Height / 2);
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Bottom)
                {
                    linearLayout.SetX(customLayout.Width / 2 - linearLayout.Width / 2);
                    linearLayout.SetY(customLayout.Height - linearLayout.Height);
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Top)
                {
                    linearLayout.SetX(customLayout.Width / 2 - linearLayout.Width / 2);
                    linearLayout.SetY(customLayout.Top);
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Left)
                {
                    linearLayout.SetX(customLayout.Left);
                    linearLayout.SetY(customLayout.Height / 2 - linearLayout.Height / 2);
                }
                else if (RSPopupPositionEnum == RSPopupPositionEnum.Right)
                {
                    linearLayout.SetX(customLayout.Width - linearLayout.Width);
                    linearLayout.SetY(customLayout.Height / 2 - linearLayout.Height / 2);
                }
            }
        }

        // Update dialog position
        private void updateDialogPosition()
        {
            var minXPositionAllowed = customLayout.Left + LeftMargin;
            var maxXPositionAllowed = customLayout.Right - RightMargin;
            var minYPositionAllowed = customLayout.Top + TopMargin;
            var maxYPositionAllowed = customLayout.Bottom - BottomMargin;

            double projectedPositionLeft;
            double projectedPositionRight;
            double projectedPositionTop;
            double projectedPositionBottom;

            double constantX = 0;
            double constantY = 0;



            // Over left top corner
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                // If on right side move left
                if ( (PositionX + relativeViewWidth / 2) >= customLayout.Width / 2)
                {
                    // first set to correct x position
                    projectedPositionLeft = PositionX + relativeViewWidth - linearLayout.Width;
                    linearLayout.SetX((int)projectedPositionLeft);

                    // check if it fits
                    if (projectedPositionLeft < minXPositionAllowed)
                    {
                        constantX = minXPositionAllowed - projectedPositionLeft;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                }
                // If on left side move right
                else if ( (PositionX + relativeViewWidth / 2) < customLayout.Width / 2)
                {
                    projectedPositionRight = PositionX + linearLayout.Width;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        constantX =  maxXPositionAllowed - projectedPositionRight;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                }
            }

            // Left Trailing to Leading anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                projectedPositionLeft = linearLayout.GetX();
                projectedPositionRight = PositionX + relativeViewWidth + linearLayout.Width;

                // If left space not available check if right is ok if not just move it to the right
                if (projectedPositionLeft < minXPositionAllowed)
                {
                    // Check if right space enough, if ok than place it at right side
                    if (projectedPositionRight <= maxXPositionAllowed)
                    {
                        // Switch side
                        if (HasArrow)
                        {
                            (linearLayout.OutlineProvider as RSViewOutlineProvider).ArrowSide = RSPopupPositionSideEnum.Right;
                            (linearLayout.Background as RSDrawable).ArrowSide = RSPopupPositionSideEnum.Right;
                            linearLayout2.SetX(arrowSize);
                        }

                        constantX = relativeViewWidth + linearLayout.Width;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);

                    }
                    // Just move it to the right so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            linearLayout2.SetX(arrowSize / 2);
                        }

                        constantX = minXPositionAllowed - projectedPositionLeft;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                }
            }

            // Right Leading to Trailing anchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                projectedPositionLeft = linearLayout.GetX() - relativeViewWidth - linearLayout.Width;
                projectedPositionRight = linearLayout.GetX() + linearLayout.Width;

                // If right space not available check if left is ok if not just move it to the left
                if (projectedPositionRight > maxXPositionAllowed)
                {
                    // Check if left space enough, if ok than place it at left side
                    if (projectedPositionLeft >= minXPositionAllowed)
                    {
                        // Switch side
                        if (HasArrow)
                        {
                            (linearLayout.OutlineProvider as RSViewOutlineProvider).ArrowSide = RSPopupPositionSideEnum.Left;
                            (linearLayout.Background as RSDrawable).ArrowSide = RSPopupPositionSideEnum.Left;
                            linearLayout2.SetX(0);
                        }

                        constantX = -relativeViewWidth - linearLayout.Width;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                    // Just move it to the left so it ends within screen bounds if widht not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            linearLayout2.SetX(linearLayout.PaddingLeft / 2);
                        }

                        constantX = maxXPositionAllowed - projectedPositionRight;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);

                        System.Diagnostics.Debug.WriteLine("X " + linearLayout.GetX());

                    }
                }
            }

            // Bottom and Top CenterAnchor
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                //// X position
                // If on right side move left if needed else keep position
                if ( (PositionX) >= customLayout.Width / 2)
                {
                    projectedPositionRight = PositionX + linearLayout.Width / 2;

                    if (projectedPositionRight > maxXPositionAllowed)
                    {
                        constantX =  maxXPositionAllowed - projectedPositionRight;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                }
                // If on left side move right if needed else keep position
                else if ((PositionX) < customLayout.Width / 2)
                {
                    projectedPositionLeft = PositionX - linearLayout.Width / 2;

                    if (projectedPositionLeft < minXPositionAllowed)
                    {
                        constantX = minXPositionAllowed - projectedPositionLeft;
                        linearLayout.SetX(linearLayout.GetX() + (int)constantX);
                    }
                }
            }



            // Y Position for Bottom
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                projectedPositionTop = PositionY - relativeViewHeight - linearLayout.Height;
                projectedPositionBottom = PositionY + linearLayout.Height;
                

                // If bottom space not available check if top is ok if not just move it to the top
                if (projectedPositionBottom > maxYPositionAllowed && (projectedPositionTop + linearLayout.Height) < customLayout.Height)
                {
                    // Check if top space enough, if ok than place it at top side
                    if (projectedPositionTop >= minYPositionAllowed)
                    {
                        // Switch side
                        if (HasArrow)
                        {
                            (linearLayout.OutlineProvider as RSViewOutlineProvider).ArrowSide = RSPopupPositionSideEnum.Top;
                            (linearLayout.Background as RSDrawable).ArrowSide = RSPopupPositionSideEnum.Top;
                            linearLayout2.SetY(linearLayout2.GetY() - arrowSize);
                        }

                        constantY = -relativeViewHeight - linearLayout.Height;
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                    // Just move it to the top so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            linearLayout2.SetY(linearLayout2.GetY() - arrowSize / 2);
                        }

                        constantY = maxYPositionAllowed - projectedPositionBottom;
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                }
                else if ((linearLayout.GetY() + linearLayout.Height) > maxYPositionAllowed) // when keyboard pops
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                        linearLayout2.SetY(linearLayout2.GetY() - arrowSize / 2);
                    }

                    var offsetY = (float)(projectedPositionTop + linearLayout.Height) - (float)customLayout.Height;
                    linearLayout.SetY(linearLayout.GetY() - linearLayout.Height - relativeViewHeight - offsetY);
                }
            }

            // Y Position for Top
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                projectedPositionTop = PositionY - linearLayout.Height;
                projectedPositionBottom = PositionY + relativeViewHeight + linearLayout.Height;

                // If top space not available check if bottom is ok if not just move it to the bottom
                if (projectedPositionTop < minYPositionAllowed)
                {
                    // Check if bottom space enough, if ok than place it at bottom side
                    if (projectedPositionBottom <= maxYPositionAllowed)
                    {
                        // Switch side
                        if (HasArrow)
                        {
                            (linearLayout.OutlineProvider as RSViewOutlineProvider).ArrowSide = RSPopupPositionSideEnum.Bottom;
                            (linearLayout.Background as RSDrawable).ArrowSide = RSPopupPositionSideEnum.Bottom;
                            linearLayout2.SetY(linearLayout2.GetY() + arrowSize);
                        }

                        constantY = relativeViewHeight + linearLayout.Height;
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                    // Just move it to the bottom so it ends within screen bounds if height not bigger than that
                    else
                    {
                        // Hide arrow since there is no enough space on screen
                        if (HasArrow)
                        {
                            HasArrow = false;
                            linearLayout2.SetY(linearLayout2.GetY() + arrowSize / 2);
                        }

                        constantY = linearLayout.Height + maxYPositionAllowed - (projectedPositionBottom - relativeViewHeight);
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                }
                else if ((linearLayout.GetY() + linearLayout.Height) > maxYPositionAllowed) // when keyboard pops
                {
                    // Hide arrow since there is no enough space on screen
                    if (HasArrow)
                    {
                        HasArrow = false;
                        linearLayout2.SetY(linearLayout2.GetY() + arrowSize / 2);
                    }

                    constantY = maxYPositionAllowed - (linearLayout.GetY() + linearLayout.Height);
                    linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                }
            }

            // Y Position for Left Right
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left ||
            RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                // If on bottom side move to top if needed else keep position
                if (PositionY >= customLayout.Height / 2)
                {
                    projectedPositionBottom = PositionY + linearLayout.Height / 2;

                    if (projectedPositionBottom > maxYPositionAllowed)
                    {
                        constantY =  maxYPositionAllowed - projectedPositionBottom;
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                }
                // If on top side move to bottom if needed else keep position
                else if (PositionY < linearLayout.Height / 2)
                {
                    projectedPositionTop = PositionY - linearLayout.Height / 2;

                    if (projectedPositionTop < minYPositionAllowed)
                    {
                        constantY = minYPositionAllowed -projectedPositionTop;
                        linearLayout.SetY(linearLayout.GetY() + (int)constantY);
                    }
                }
            }

            // Y Position for Over
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                projectedPositionBottom = PositionY + linearLayout.Height;

                if (projectedPositionBottom > maxYPositionAllowed)
                {
                    constantY = projectedPositionBottom - maxYPositionAllowed;
                    linearLayout.SetY(linearLayout.GetY() - (int)constantY);
                }
            }



            (linearLayout.Background as RSDrawable).xConstrainConstant = (float)constantX;
            (linearLayout.OutlineProvider as RSViewOutlineProvider).xConstrainConstant = (float)constantX;
            (linearLayout.Background as RSDrawable).yConstrainConstant = (float)constantY;
            (linearLayout.OutlineProvider as RSViewOutlineProvider).yConstrainConstant = (float)constantY;
        }

        private void CustomLayout_LayoutChange(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("measured w " + linearLayout.MeasuredWidth + "  width " + linearLayout.Width);
            if (RelativeView != null)
            {
                shouldShowArrow();
                (linearLayout.OutlineProvider as RSViewOutlineProvider).ArrowSide = this.RSPopupPositionSideEnum;
                (linearLayout.Background as RSDrawable).ArrowSide = this.RSPopupPositionSideEnum;

                // Set positionX and positionY 
                SetPopupPositionRelativeTo(RelativeView, Resources.DisplayMetrics);
            }

            resetContentPosition();

            setDialogPosition();
            updateDialogPosition();

            // Update outline 
            linearLayout.InvalidateOutline();


            Console.WriteLine(linearLayout.GetY());
        }

        // Moves linearlayout2 on x or y axis so it compensates for the padding
        private void resetContentPosition()
        {
            if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                linearLayout2.SetX(0);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                linearLayout2.SetX(linearLayout.PaddingLeft);

            if(RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                linearLayout2.SetY(0);
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                linearLayout2.SetY(linearLayout.PaddingTop);
        }

        //Orientation change
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            //In order to force request layout to update size based on new position
            //Position of relative view is^given after this method
            if (Orientation != newConfig.Orientation && UserSetPosition)
            {
                Orientation = newConfig.Orientation;
                canRequestLayout = true;
            }
        }

        //Show popup
        public void ShowPopup()
        {
            //Check if it is already added & ANDROID context not null
            // if (!this.IsAdded && RSAppContext.RSContext != null)
            this.Show(((AppCompatActivity)RSAppContext.RSContext).SupportFragmentManager, "sc");
        }

        //Set closeButton position
        public void setCloseButon()
        {
            if (HasCloseButton)
                closeButton.Visibility = ViewStates.Visible;

            if (RightMargin < closeButton.MeasuredWidth / 2)
                RightMargin = closeButton.MeasuredWidth / 2;
            if (TopMargin < closeButton.MeasuredHeight / 2)
                TopMargin = closeButton.MeasuredHeight / 2;


            closeButton.SetX(linearLayout.GetX() + linearLayout.MeasuredWidth - closeButton.MeasuredWidth / 2);
            closeButton.SetY(linearLayout.GetY() - closeButton.MeasuredHeight / 2);
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


            this.linearLayoutMinWidth += (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, ((AppCompatActivity)RSAppContext.RSContext).Resources.DisplayMetrics);
            linearLayout.SetMinimumWidth(this.linearLayoutMinWidth);
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

            this.linearLayoutMinWidth += (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, ((AppCompatActivity)RSAppContext.RSContext).Resources.DisplayMetrics);
            linearLayout.SetMinimumWidth(this.linearLayoutMinWidth);
        }

        public Forms.View GetRelativeView()
        {
            return this.RelativeView;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            backFromSleep = true;
        }

        public void OnClick(global::Android.Views.View v)
        {
            if (v.Id == customLayout.Id && !IsModal)
            {
                this.Arguments = null;
                this.Dismiss();
            }
            else if (v.Id == closeButton.Id)
            {
                this.Dismiss();
            }
        }

        //Dismiss
        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            OnDismissed();
            this.Dispose();
        }

        //Dismiss event
        public delegate void DismissEventHandler(object source, EventArgs args);
        public event EventHandler DismissEvent;
        protected virtual void OnDismissed()
        {
            if (DismissEvent != null)
                DismissEvent(this, EventArgs.Empty);
        }

        //Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //Force dispose so we can remove CanExecuteChanged event on command in buttons dispose method
            positiveButton?.Dispose();
            neutralButton?.Dispose();
            destructiveButton?.Dispose();

            customLayout.LayoutChange -= CustomLayout_LayoutChange;
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

                if (command != null)
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

        public global::AndroidX.Fragment.App.DialogFragment dialog { get; set; }
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
                if (this.Command != null)
                    this.Command.CanExecuteChanged -= Command_CanExecuteChanged;
            }
        }
    }

    public class CustomLinearLayout : LinearLayout
    {
        public RSPopupRenderer rSPopupRenderer { get; set; }
        public MeasureSpecMode SpecMode { get; set; }

        public float BorderRadius { get; set; }


        public CustomLinearLayout(Context context) : base(context)
        {

        }

        public CustomLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public CustomLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {

        }

        public CustomLinearLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }




        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);


            ////request second layout pass so it updates when orientation changed "Only when popup set to relative view"
            //if (rSPopupRenderer.canRequestLayout)
            //{
            //    this.RequestLayout();
            //    rSPopupRenderer.canRequestLayout = false;
            //}


            //var filledPaint = new Paint();
            //filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            //filledPaint.Color = global::Android.Graphics.Color.White;
            //filledPaint.AntiAlias = true;

            //float arrowSize = 30;


            //Path path = new Path();
            //path.SetFillType(Path.FillType.EvenOdd);
            //path.MoveTo(0, 0);
            //path.AddRoundRect(new RectF(0, 0, canvas.ClipBounds.Width(), canvas.ClipBounds.Height() - arrowSize), BorderRadius, BorderRadius, Path.Direction.Cw);

            //path.MoveTo(canvas.ClipBounds.Width() / 2 + arrowSize, canvas.ClipBounds.Height() - arrowSize);
            //path.LineTo(canvas.ClipBounds.Width() / 2, canvas.ClipBounds.Height());
            //path.LineTo(canvas.ClipBounds.Width() / 2 - arrowSize, canvas.ClipBounds.Height() - arrowSize);
            //path.Close();
            //canvas.DrawPath(path, filledPaint);
        }


        //public override void OnWindowFocusChanged(bool hasWindowFocus)
        //{
        //    base.OnWindowFocusChanged(hasWindowFocus);



        //    TranslateAnimation translateAnimation = new TranslateAnimation(this.GetX(), this.GetX(), this.GetY(), this.GetY() + 300);
        //    translateAnimation.Duration = 400;
        //    translateAnimation.FillAfter = true;
        //    this.StartAnimation(translateAnimation);
        //}
    }

    public class RSDrawable : Drawable
    {
        public float BorderRadius { get; set; }
        public RSPopupPositionSideEnum ArrowSide;
        public RSPopupRenderer rSPopupRenderer;
        public CustomLinearLayout linearLayout;
        public float xConstrainConstant;
        public float yConstrainConstant;
        public float ArrowSize { get; set; }

        public override int Opacity => 1;

        public override void Draw(Canvas canvas)
        {
            var filledPaint = new Paint();
            filledPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
            filledPaint.Color = global::Android.Graphics.Color.White;
            filledPaint.AntiAlias = true;
             

            Path path = new Path();
            path.SetFillType(Path.FillType.EvenOdd);
            path.MoveTo(0, 0);

            if (ArrowSide == RSPopupPositionSideEnum.Right && rSPopupRenderer.HasArrow)
            {
                var posY = (canvas.ClipBounds.Height() / 2) - yConstrainConstant;
                path.AddRoundRect(new RectF(ArrowSize, 0, canvas.ClipBounds.Width(), canvas.ClipBounds.Height()), BorderRadius, BorderRadius, Path.Direction.Cw);

                path.MoveTo(ArrowSize, posY - ArrowSize);
                path.LineTo(0, posY);
                path.LineTo(ArrowSize, posY + ArrowSize);
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Left && rSPopupRenderer.HasArrow)
            {
                var posY = (canvas.ClipBounds.Height() / 2) - yConstrainConstant;
                path.AddRoundRect(new RectF(0, 0, canvas.ClipBounds.Width() - ArrowSize, canvas.ClipBounds.Height()), BorderRadius, BorderRadius, Path.Direction.Cw);

                path.MoveTo(canvas.ClipBounds.Width() - ArrowSize, posY - ArrowSize);
                path.LineTo(canvas.ClipBounds.Width(),posY);
                path.LineTo(canvas.ClipBounds.Width() - ArrowSize, posY + ArrowSize);
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Bottom && rSPopupRenderer.HasArrow)
            {
                path.AddRoundRect(new RectF(0, ArrowSize, canvas.ClipBounds.Width(), canvas.ClipBounds.Height()), BorderRadius, BorderRadius, Path.Direction.Cw);
                var posX = (canvas.ClipBounds.Width() / 2) - xConstrainConstant;

                path.MoveTo(posX + ArrowSize, ArrowSize);
                path.LineTo(posX, 0);
                path.LineTo(posX - ArrowSize, ArrowSize);
            }
            else if (ArrowSide == RSPopupPositionSideEnum.Top && rSPopupRenderer.HasArrow)
            {
                path.AddRoundRect(new RectF(0, 0, canvas.ClipBounds.Width(), canvas.ClipBounds.Height() - ArrowSize), BorderRadius, BorderRadius, Path.Direction.Cw);
                var posX = (canvas.ClipBounds.Width() / 2) - xConstrainConstant;

                path.MoveTo(posX + ArrowSize, canvas.ClipBounds.Height() - ArrowSize);
                path.LineTo(posX, canvas.ClipBounds.Height());
                path.LineTo(posX - ArrowSize, canvas.ClipBounds.Height() - ArrowSize);
            }
            else
                path.AddRoundRect(new RectF(0, 0, canvas.ClipBounds.Width(), canvas.ClipBounds.Height()), BorderRadius, BorderRadius, Path.Direction.Cw);


            path.Close();
            canvas.DrawPath(path, filledPaint);
        }

        public override void SetAlpha(int alpha)
        {
            
        }

        public override void SetColorFilter(ColorFilter colorFilter)
        {
            
        }
    }
}
