using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSPopup : BindableObject
    {
        public IDialogPopup service;

        public event EventHandler Dismissed;

        // Title
        public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(RSPopup), string.Empty);
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Message
        public static readonly BindableProperty MessageProperty = BindableProperty.Create("Message", typeof(string), typeof(RSPopup), string.Empty);
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // BackgroundColor
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create("BackgroundColor", typeof(Color), typeof(RSPopup), Color.White);
        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        // BorderRadius
        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSPopup), 16f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        // DimAmount
        public static readonly BindableProperty DimAmountProperty = BindableProperty.Create("DimAmount", typeof(float), typeof(RSPopup), 0.7f);
        public float DimAmount
        {
            get { return (float)GetValue(DimAmountProperty); }
            set { SetValue(DimAmountProperty, value); }
        }


        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == "BackgroundColor")
                SetBackgroundColor(BackgroundColor);
            else if (propertyName == "BorderRadius")
                SetBorderRadius(BorderRadius);
            else if (propertyName == "DimAmount")
                SetDimAmount(DimAmount);
            else if (propertyName == "Title")
                SetTitle(Title);
            else if (propertyName == "Message")
                SetMessage(Message);
        }


        public RSPopup(RSPopupPositionEnum rSPopupPosition = RSPopupPositionEnum.Center)
        {
            Init("", "", rSPopupPosition);
        }

        public RSPopup(string title, string message, RSPopupPositionEnum rSPopupPosition = RSPopupPositionEnum.Center)
        {
            Init(title, message, rSPopupPosition);
        }

        private void Init(string title, string message, RSPopupPositionEnum rSPopupPosition)
        {
            service = DependencyService.Get<IDialogPopup>(DependencyFetchTarget.NewInstance);
            service.rSPopup = this;
            SetTitle(title);
            SetMessage(message);
            SetBackgroundColor(BackgroundColor);
            SetBorderRadius(BorderRadius);
            SetDimAmount(DimAmount);
            SetShadowEnabled(true);
            SetRSPopupPosition(rSPopupPosition);

            // Set default size to wrap parent if not set by user
            service.Width = -2;
            service.Height = -2;
        }

        public void Show()
        {
            service.ShowPopup();
        }

        public void SetPopupPositionRelativeTo(View view)
        {
            service.RelativeView = view;
            service.UserSetPosition = true;
            service.RSPopupPositionSideEnum = RSPopupPositionSideEnum.Bottom; //default
        }

        public void SetPopupPositionRelativeTo(View view, RSPopupPositionSideEnum rSPopupPositionSideEnum, float offsetX = 0, float offsetY = 0)
        {
            service.RelativeView = view;
            service.UserSetPosition = true;
            service.RSPopupPositionSideEnum = rSPopupPositionSideEnum;
            service.RSPopupOffsetX = offsetX;
            service.RSPopupOffsetY = offsetY;
        }

        public void SetPopupSize(int width, int height)
        {
            service.Width = width;
            service.Height = height;
        }

        public void SetPopupSize(int width, RSPopupSizeEnum height)
        {
            service.Width = width;
            service.Height = (int)height;
        }

        public void SetPopupSize(RSPopupSizeEnum width, int height)
        {
            service.Width = (int)width;
            service.Height = height;
        }

        public void SetPopupSize(RSPopupSizeEnum width, RSPopupSizeEnum height)
        {
            service.Width = (int)width;
            service.Height = (int)height;
        }

        public void SetMargin(int left, int top, int right, int bottom)
        {
            service.LeftMargin = left;
            service.TopMargin = top;
            service.RightMargin = right;
            service.BottomMargin = bottom;
            service.UserSetMargin = true;
        }

        public void SetCustomView(Forms.VisualElement customView)
        {
            service.CustomView = customView;
        }

        public void SetShadowEnabled(bool enabled)
        {
            service.ShadowEnabled = enabled;
        }

        private void SetTitle(string title)
        {
            service.Title = title;
        }

        private void SetMessage(string message)
        {
            service.Message = message;
        }

        private void SetBorderRadius(float borderRadius)
        {
            service.BorderRadius = borderRadius;
        }

        private void SetBackgroundColor(Forms.Color color)
        {
            service.BorderFillColor = color;
        }

        private void SetRSPopupPosition(RSPopupPositionEnum rSPopupPosition)
        {
            service.RSPopupPositionEnum = rSPopupPosition;
        }

        private void SetDimAmount(float amount)
        {
            if (amount > 0.7)
                amount = 0.7f;

            service.DimAmount = amount;
        }

        public void SetPopupAnimation(RSPopupAnimationEnum rSPopupAnimationEnum)
        {
            service.RSPopupAnimationEnum = rSPopupAnimationEnum;
        }

        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command = null, object commandParameter = null)
        {
            service.AddAction(title, rSPopupButtonType, command, commandParameter);
        }

        public void Close()
        {
            service.Close();
        }

        public void OnDismissed()
        {
            Dismissed?.Invoke(this, EventArgs.Empty);
        }
    }
}
