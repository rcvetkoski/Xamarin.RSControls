using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSPopup
    {
        private IDialogPopup service;
        public string Title { get; set; }
        public string Message { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float BorderRadius { get; set; }
        public Forms.Color BackgroundColor { get; set; }
        public float DimAmount { get; set; }
        public bool ShadowEnabled { get; set; }


        public RSPopup()
        {
            service = DependencyService.Get<IDialogPopup>();
            SetBackgroundColor(Color.White);
            SetBorderRadius(14);
            SetDimAmount(0.5f);
            SetShadowEnabled(true);
        }

        public RSPopup(string title, string message)
        {
            service = DependencyService.Get<IDialogPopup>();
            SetTitle(title);
            SetMessage(message);
            SetBackgroundColor(Color.White);
            SetBorderRadius(14);
            SetDimAmount(0.5f);
            SetShadowEnabled(true);
        }

        public void Show()
        {
            service.ShowPopup();
        }

        public void SetPopupPosition() //Center of the screen
        {
            
        }

        public void SetPopupPosition(float X, float Y)
        {
            PositionX = X;
            PositionY = Y;

            service.PositionX = X;
            service.PositionY = Y;
        }

        public void SetPopupPosition(View view)
        {
            service.RelativeView = view;
        }

        public void SetTitle(string title)
        {
            Title = title;
            service.Title = this.Title;
        }

        public void SetMessage(string message)
        {
            Message = message;
            service.Message = this.Message;
        }

        public void SetCustomView(Forms.View customView)
        {
            service.CustomView = customView;
        }

        public void SetBorderRadius(float borderRadius)
        {
            BorderRadius = borderRadius;
            service.BorderRadius = this.BorderRadius;
        }

        public void SetBackgroundColor(Forms.Color color)
        {
            BackgroundColor = color;
            service.BorderFillColor = BackgroundColor;
        }

        public void SetShadowEnabled(bool enabled)
        {
            ShadowEnabled = enabled;
            service.ShadowEnabled = this.ShadowEnabled;
        }

        public void SetDimAmount(float amount)
        {
            DimAmount = amount;
            service.DimAmount = DimAmount;
        }

        public void SetPopupAnimation(RSPopupAnimationEnum rSPopupAnimationEnum = RSPopupAnimationEnum.Default)
        {
            //TODO
        }

        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType)
        {
            service.AddAction(title, rSPopupButtonType);
        }
    }
}
