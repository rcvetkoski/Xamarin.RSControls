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
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Xaml;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using static Android.Views.ViewTreeObserver;

[assembly: Dependency(typeof(RSPopupRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPopupRenderer : global::Android.Support.V4.App.DialogFragment, IDialogPopup, IDisposable, global::Android.Views.View.IOnClickListener
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool UserSetPosition { get; set; }
        public bool UserSetSize { get; set; }
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
        private LinearLayout buttonsLayout;
        public global::Android.Views.View arrow;
        public global::Android.Graphics.Point arrowSize;
        public RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
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
            buttonsLayout = customLayout.FindViewById<global::Android.Widget.LinearLayout>(Resource.Id.buttons);
            arrow = customLayout.FindViewById<global::Android.Views.View>(Resource.Id.arrow);
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
            arrow = customLayout.FindViewById<global::Android.Views.View>(Resource.Id.arrow);
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

            global::Android.Support.V7.App.AlertDialog.Builder builder = new global::Android.Support.V7.App.AlertDialog.Builder(Context, Resource.Style.RSDialogAnimationTheme);
            return builder.Create();
        }

        public override void OnStart()
        {
            base.OnStart();


            Orientation = Context.Resources.Configuration.Orientation;

            //Fix for Control that call keyboard like entry (keyboard not showing up)
            Dialog.Window.ClearFlags(WindowManagerFlags.NotFocusable | WindowManagerFlags.AltFocusableIm);
            //Dialog.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);

            if(!backFromSleep) //Dont want to reset layoutparameters when back from sleep
                SetDialog();
        }

        //Set PopupSize
        private void SetPopupSize(DisplayMetrics metrics)
        {
            if (UserSetSize)
            {
                if (Width != -2 && Width != -1)
                {
                    Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Width, Context.Resources.DisplayMetrics);

                    //Fix if width user inputs greater than creen width
                    if (Width > metrics.WidthPixels)
                        Width = metrics.WidthPixels;
                }

                if (Height != -2 && Height != -1)
                {
                    Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, Height, Context.Resources.DisplayMetrics);

                    //Fix if height user inputs greater than creen height
                    if (Height > metrics.HeightPixels)
                        Height = metrics.HeightPixels;
                }
            }
            else
            {
                Width = ViewGroup.LayoutParams.WrapContent;
                Height = ViewGroup.LayoutParams.WrapContent;
            }
        }

        //Set Popup position relative to view
        public void SetPopupPositionRelativeTo(Forms.View formsView, DisplayMetrics metrics)
        {
            if (formsView != null)
            {
                int x = 0;
                int y = 0;

                var relativeViewAsNativeRenderer = Xamarin.Forms.Platform.Android.Platform.GetRenderer(formsView);
                relativeViewAsNativeRenderer.UpdateLayout();
                relativeViewAsNativeView = relativeViewAsNativeRenderer.View;
                Rect rectf = new Rect();
                relativeViewAsNativeView.GetWindowVisibleDisplayFrame(rectf);
                int[] locationScreen = new int[2];
                relativeViewAsNativeView.GetLocationOnScreen(locationScreen);

                var relativeViewWidth = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Width, Context.Resources.DisplayMetrics));
                var relativeViewHeight = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)formsView.Height, Context.Resources.DisplayMetrics));


                //X
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    x = locationScreen[0] - rectf.Left - arrowSize.Y;
                    y = locationScreen[1] - rectf.Top + relativeViewHeight / 2 - arrowSize.X / 2;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    x = locationScreen[0] - rectf.Left + relativeViewWidth;
                    y = locationScreen[1] - rectf.Top + relativeViewHeight / 2 - arrowSize.X / 2;
                }


                //Y
                if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                {
                    y = locationScreen[1] - rectf.Top - arrowSize.Y;
                    x = locationScreen[0] - rectf.Left + relativeViewWidth / 2 - arrowSize.X / 2;
                }
                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                {
                    y = locationScreen[1] + relativeViewHeight - rectf.Top;
                    x = locationScreen[0] - rectf.Left + relativeViewWidth / 2 - arrowSize.X / 2;
                }

                else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
                {
                    y = locationScreen[1] - rectf.Top;
                    x = locationScreen[0];
                }

                screenUsableWidth = Resources.DisplayMetrics.WidthPixels - (Resources.DisplayMetrics.WidthPixels - (rectf.Right - rectf.Left));
                screenUsableHeight = Resources.DisplayMetrics.HeightPixels - (Resources.DisplayMetrics.HeightPixels - (rectf.Bottom - rectf.Top));

                //Set position
                PositionX = x + (int)RSPopupOffsetX;
                PositionY = y + (int)RSPopupOffsetY;
                customLayout.MeasuredHeight.ToString();
            }
        }

        //Show popup
        public void ShowPopup()
        {
            //Check if it is already added & ANDROID context not null
            // if (!this.IsAdded && RSAppContext.RSContext != null)
            this.Show(((AppCompatActivity)RSAppContext.RSContext).SupportFragmentManager, "sc");
        }

        //Set and add custom view 
        private void SetCustomView()
        {
            var renderer = Platform.CreateRendererWithContext(CustomView, Context);
            Platform.SetRenderer(CustomView, renderer);
            convertView = new Extensions.ViewCellContainer(Context, CustomView, renderer);
            this.contentView.AddView(convertView);
            convertView.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        }

        //Set native view 
        public void SetNativeView(global::Android.Views.View nativeView)
        {
            this.contentView.AddView(nativeView);
        }

        //Set dialog properties
        private void SetDialog()
        {
            //Default value can be changed by user
            if (!UserSetMargin && RelativeView == null)
            {
                LeftMargin = 30;
                TopMargin = 30;
                RightMargin = 30;
                BottomMargin = 30;
            }

            Orientation = Resources.Configuration.Orientation;
            linearLayout.rSPopupRenderer = this;


            LeftMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, LeftMargin, Context.Resources.DisplayMetrics));
            TopMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, TopMargin, Context.Resources.DisplayMetrics));
            RightMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, RightMargin, Context.Resources.DisplayMetrics));
            BottomMargin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, BottomMargin, Context.Resources.DisplayMetrics));

            var attrs = this.Dialog.Window.Attributes;
            var metrics = Resources.DisplayMetrics;

            SetBackground();
            SetCustomLayout();


            //Popup size
            SetPopupSize(metrics);

            //Apply size
            attrs.Width = ViewGroup.LayoutParams.MatchParent;
            attrs.Height = ViewGroup.LayoutParams.MatchParent;

            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = Width;
            (linearLayout.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = Height;


            arrowSize = new global::Android.Graphics.Point(0, 0);
            if (RelativeView != null)
            {
                arrowSize.X = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Context.Resources.DisplayMetrics));
                arrowSize.Y = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 15, Context.Resources.DisplayMetrics));
            }


            //Position
            if (UserSetPosition)
            {
                //Set the gravity top and left so it starts at real 0 coordinates than aply position
                Dialog.Window.SetGravity(GravityFlags.Left | GravityFlags.Top);


                //if (RelativeView != null)
                //    SetPopupPositionRelativeTo(RelativeView, metrics);

                //setArrow();
                //SetLinearLayoutPosition();
                if (HasCloseButton)
                    setCloseButon();
            }
            else
            {
                customLayout.SetGravity(GravityFlags.Center);
            }


            //Set dim amount
            attrs.DimAmount = this.DimAmount;

            //Set new attributes
            this.Dialog.Window.Attributes = attrs;


            if (HasCloseButton)
                closeButton.SetOnClickListener(this);

            linearLayout.LayoutChange += LinearLayout_LayoutChange;
        }

        private void LinearLayout_LayoutChange(object sender, global::Android.Views.View.LayoutChangeEventArgs e)
        {
            if (RelativeView != null)
            {
                linearLayout.LayoutChange -= LinearLayout_LayoutChange;

                SetPopupPositionRelativeTo(RelativeView, Resources.DisplayMetrics);
                setArrow();
                SetLinearLayoutPosition();
            }
        }

        //Set popup position relative view
        public void SetLinearLayoutPosition()
        {
            //Left Right
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                linearLayout.SetX(arrow.GetX() - linearLayout.MeasuredWidth + 1);
                correctY();
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                linearLayout.SetX(arrow.GetX() + arrowSize.Y - 1);
                correctY();
            }

            //Top Bottom
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                correctedX();
                correctY();
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                correctedX();
                correctY();
            }

            //Over
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                correctedX();
                correctY();
                //linearLayout.SetY(arrow.GetY());
            }
        }

        //Correct X if not place to show on screen
        private void correctedX()
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
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
                if (arrow.GetX() > (screenUsableWidth - RightMargin - LeftMargin) / 2) //Right side of screen
                {
                    if ((arrow.GetX() + arrowSize.X / 2 + linearLayout.MeasuredWidth / 2) > screenUsableWidth - RightMargin)
                    {
                        var offset = (arrow.GetX() + arrowSize.X / 2 + linearLayout.MeasuredWidth / 2) - screenUsableWidth;
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2 - offset - RightMargin);
                    }
                    else
                    {
                        linearLayout.SetX(arrow.GetX() + arrowSize.X / 2 - linearLayout.MeasuredWidth / 2);
                    }
                }
                else if (arrow.GetX() < (screenUsableWidth - RightMargin - LeftMargin) / 2) //Left side of screen
                {
                    if ((arrow.GetX() - arrowSize.X / 2 - linearLayout.MeasuredWidth / 2) < (0 + LeftMargin))
                    {
                        var offset = (arrow.GetX() - arrowSize.X / 2 - linearLayout.MeasuredWidth / 2);
                        linearLayout.SetX(arrow.GetX() - arrowSize.X / 2 - linearLayout.MeasuredWidth / 2 - offset + LeftMargin);
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

        //Correct Y if not place to show on screen
        private void correctY()
        {
            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                var pos = arrow.GetY() - linearLayout.MeasuredHeight + 1;
                var posBottom = arrow.GetY() + arrowSize.X + linearLayout.MeasuredHeight + 1;


                if (pos < 0 + TopMargin && pos < (screenUsableHeight + TopMargin) && posBottom < (screenUsableHeight - BottomMargin))
                    RSPopupPositionSideEnum = RSPopupPositionSideEnum.Bottom;
                else if (pos < 0 + TopMargin)
                    linearLayout.SetY((int)(arrow.GetY() + arrowSize.Y + System.Math.Abs(pos + TopMargin)));
                else if(arrow.GetY() > (screenUsableHeight - BottomMargin))
                    linearLayout.SetY((screenUsableHeight - BottomMargin) - linearLayout.MeasuredHeight + 1);
                else
                    linearLayout.SetY(arrow.GetY() - linearLayout.MeasuredHeight + 1);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
            {
                var pos = arrow.GetY() + arrowSize.X + linearLayout.MeasuredHeight + 1;
                var posTop = arrow.GetY() + arrowSize.X - linearLayout.MeasuredHeight - 1;

                if (pos > (screenUsableHeight - BottomMargin) && pos > 0 && posTop > 0 && posTop < (screenUsableHeight - BottomMargin))
                    RSPopupPositionSideEnum = RSPopupPositionSideEnum.Top;
                else if (pos > (screenUsableHeight - BottomMargin))
                    linearLayout.SetY((screenUsableHeight - BottomMargin) - linearLayout.MeasuredHeight + 1);
                else
                    linearLayout.SetY(arrow.GetY() + arrowSize.Y - 1);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right || RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                var linearLayoutHeight = linearLayout.MeasuredHeight;

                if (arrow.GetY() < (screenUsableHeight - TopMargin - BottomMargin) / 2) //Top side of screen
                {
                    var pos = arrow.GetY() - linearLayoutHeight / 2 + arrowSize.X / 2;

                    if (pos < 0 + TopMargin)
                        linearLayout.SetY((int)(arrow.GetY() + arrowSize.X / 2 - linearLayoutHeight / 2 + System.Math.Abs(TopMargin - pos)));
                    else
                        linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayoutHeight / 2);
                }
                else if (arrow.GetY() > (screenUsableHeight - TopMargin - BottomMargin) / 2)// Bottom side of screen
                {
                    var pos = arrow.GetY() + linearLayoutHeight / 2 + arrowSize.X / 2;

                    if (pos > screenUsableHeight - BottomMargin)
                        linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayoutHeight / 2 - (pos - (screenUsableHeight - BottomMargin)));
                    else
                        linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayoutHeight / 2);
                }
                else
                {
                    linearLayout.SetY(arrow.GetY() + arrowSize.X / 2 - linearLayoutHeight / 2);
                }
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                var linearLayoutHeight = linearLayout.MeasuredHeight;
                var pos = arrow.GetY() + linearLayoutHeight;

                if (pos > screenUsableHeight && arrow.GetY() < (screenUsableHeight - TopMargin - BottomMargin) / 2)
                    linearLayout.SetY((int)(arrow.GetY() - (pos - (screenUsableHeight - TopMargin))));
                else if (pos > screenUsableHeight && arrow.GetY() > (screenUsableHeight - TopMargin - BottomMargin) / 2)
                    linearLayout.SetY((int)(arrow.GetY() - (pos - (screenUsableHeight - BottomMargin))));
                else
                    linearLayout.SetY(arrow.GetY());



                //if (arrow.GetY() > (screenUsableHeight - TopMargin - BottomMargin) / 2)
                //{
                //    var pos = arrow.GetY() + linearLayoutHeight;
                //    if (pos > screenUsableHeight - BottomMargin)
                //        linearLayout.SetY((int)(arrow.GetY() - (pos - screenUsableHeight) - BottomMargin));
                //    else
                //        linearLayout.SetY(arrow.GetY());
                //}
                //else if (arrow.GetY() < (screenUsableHeight - TopMargin - BottomMargin) / 2)
                //{
                //    var pos = arrow.GetY() - linearLayoutHeight;
                //    if (pos < 0 + TopMargin)
                //        linearLayout.SetY((int)(arrow.GetY() + System.Math.Abs(pos) + TopMargin));
                //    else
                //        linearLayout.SetY(arrow.GetY());
                //}
                //else
                //linearLayout.SetY(arrow.GetY());
            }
        }

        //Set arrow background size and position
        public void setArrow()
        {
            arrow.SetBackground(new CustomArrow(arrow, RSPopupPositionSideEnum, Context));
            //arrow.Background = new CustomArrow(arrow, RSPopupPositionSideEnum, Context);

            if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
            {
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.X;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.Y;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
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
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.Y;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.X;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
            }
            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.Y;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.X;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
            }

            else if (RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
            {
                //var lp = new global::Android.Widget.RelativeLayout.LayoutParams(arrowSize.Y, arrowSize.X);
                //arrow.LayoutParameters = lp;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Width = arrowSize.Y;
                (arrow.LayoutParameters as global::Android.Widget.RelativeLayout.LayoutParams).Height = arrowSize.X;
                arrow.SetX(PositionX);
                arrow.SetY(PositionY);
            }

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

        //Custom background so we can set border radius shadow ...
        private void SetBackground()
        {
            //Manipulate color and roundness of border
            GradientDrawable gradientDrawable = new GradientDrawable();
            gradientDrawable.SetColor(BorderFillColor.ToAndroid());
            gradientDrawable.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, BorderRadius, Context.Resources.DisplayMetrics));
            gradientDrawable.SetStroke(1, global::Android.Graphics.Color.LightGray);
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
            linearLayout.RequestLayout();
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

            this.Dispose();
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
                if (this.Command != null)
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

            if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
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
            if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Top)
                drawArrowTop(canvas);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Bottom)
                drawArrowBottom(canvas);
            else if (rSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
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

        private void drawArrowTop(Canvas canvas)
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
            path.LineTo(width / 2, height);
            path.LineTo(width, 0);
            path.LineTo(0, 0);
            path.Close();


            //Arrow
            Paint paint2 = new Paint();
            paint2.Color = global::Android.Graphics.Color.White;
            paint2.AntiAlias = true;
            Path path2 = new Path();
            path2.SetFillType(Path.FillType.EvenOdd);


            path2.MoveTo(shadowThikness, 0);
            path2.LineTo(width / 2, height - shadowThikness);
            path2.LineTo(width - shadowThikness, 0);
            path2.Close();

            canvas.DrawPath(path, paint);
            canvas.DrawPath(path2, paint2);
        }

        private void drawArrowBottom(Canvas canvas)
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

    public class CustomLinearLayout : LinearLayout
    {
        public RSPopupRenderer rSPopupRenderer { get; set; }
        public MeasureSpecMode SpecMode { get; set; }


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

            //request second layout pass so it updates when orientation changed "Only when popup set to relative view"
            if (rSPopupRenderer.canRequestLayout)
            {
                this.RequestLayout();
                rSPopupRenderer.canRequestLayout = false;
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            double widthSpecPixel = MeasureSpec.GetSize(widthMeasureSpec);
            double heightSpecPixel = MeasureSpec.GetSize(heightMeasureSpec);

            var screenWidth = rSPopupRenderer.screenUsableWidth;
            var screenHeight = rSPopupRenderer.screenUsableHeight;


            if (rSPopupRenderer.UserSetPosition && rSPopupRenderer.RelativeView != null)
            {
                rSPopupRenderer.SetPopupPositionRelativeTo(rSPopupRenderer.GetRelativeView(), Resources.DisplayMetrics);
                rSPopupRenderer.setArrow();
                rSPopupRenderer.SetLinearLayoutPosition();

                if (rSPopupRenderer.RSPopupPositionSideEnum == RSPopupPositionSideEnum.Left)
                {
                    var x = rSPopupRenderer.arrow.GetX();
                    var maxWidth = System.Math.Abs(x - rSPopupRenderer.LeftMargin);

                    if (maxWidth < widthSpecPixel)
                    {
                        var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                        widthMeasureSpec = MeasureSpec.MakeMeasureSpec((int)maxWidth, widthMode);
                    }
                }
                else if (rSPopupRenderer.RSPopupPositionSideEnum == RSPopupPositionSideEnum.Right)
                {
                    var x = rSPopupRenderer.arrow.GetX() + rSPopupRenderer.arrowSize.Y;
                    var maxWidth = System.Math.Abs(screenWidth - rSPopupRenderer.RightMargin - x);

                    if (maxWidth < widthSpecPixel)
                    {
                        var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                        widthMeasureSpec = MeasureSpec.MakeMeasureSpec((int)maxWidth, widthMode);
                    }
                }
                else if (rSPopupRenderer.RSPopupPositionSideEnum == RSPopupPositionSideEnum.Over)
                {
                    var maxWidth = screenWidth - rSPopupRenderer.LeftMargin - rSPopupRenderer.RightMargin;

                    if (maxWidth < widthSpecPixel)
                    {
                        var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                        widthMeasureSpec = MeasureSpec.MakeMeasureSpec((int)maxWidth, widthMode);
                    }
                }
                else
                {
                    var maxWidth = System.Math.Abs(screenWidth - rSPopupRenderer.LeftMargin - rSPopupRenderer.RightMargin);

                    if (maxWidth < widthSpecPixel)
                    {
                        var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                        widthMeasureSpec = MeasureSpec.MakeMeasureSpec((int)maxWidth, widthMode);
                    }
                }

                //Fix Height
                var maxHeight = screenHeight - rSPopupRenderer.TopMargin - rSPopupRenderer.BottomMargin;
                if (heightSpecPixel > maxHeight)
                {
                    var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
                    heightMeasureSpec = MeasureSpec.MakeMeasureSpec((int)maxHeight, heightMode);
                }

                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

                rSPopupRenderer.SetPopupPositionRelativeTo(rSPopupRenderer.GetRelativeView(), Resources.DisplayMetrics);
                rSPopupRenderer.setArrow();
                rSPopupRenderer.SetLinearLayoutPosition();

                this.MeasuredWidth.ToString();
                this.MeasuredHeight.ToString();
            }
            else
            {
                if (widthSpecPixel > Resources.DisplayMetrics.WidthPixels - rSPopupRenderer.LeftMargin - rSPopupRenderer.RightMargin)
                {
                    var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
                    widthMeasureSpec = MeasureSpec.MakeMeasureSpec(Resources.DisplayMetrics.WidthPixels - rSPopupRenderer.LeftMargin - rSPopupRenderer.RightMargin, widthMode);
                }

                if (heightSpecPixel > heightSpecPixel - rSPopupRenderer.TopMargin - rSPopupRenderer.BottomMargin)
                {
                    var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
                    heightMeasureSpec = MeasureSpec.MakeMeasureSpec((int)heightSpecPixel - rSPopupRenderer.TopMargin - rSPopupRenderer.BottomMargin, heightMode);
                }

                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }

            if (rSPopupRenderer.HasCloseButton)
                rSPopupRenderer.setCloseButon();
        }
    }
}
