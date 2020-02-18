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
        public bool ShadowEnabled { get; set; }


        public RSPopup()
        {
            service = DependencyService.Get<IDialogPopup>();
            SetBorderRadius(12);
            SetShadowEnabled(true);
        }

        public RSPopup(string title, string message)
        {
            service = DependencyService.Get<IDialogPopup>();
            SetTitle(title);
            SetMessage(message);
            SetBorderRadius(12);
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

        public void SetBorderRadius(float borderRadius)
        {
            BorderRadius = borderRadius;
            service.BorderRadius = this.BorderRadius;
        }

        public void SetShadowEnabled(bool enabled)
        {
            ShadowEnabled = enabled;
            service.ShadowEnabled = this.ShadowEnabled;
        }

        public void SetPopupAnimation(RSPopupAnimationEnum rSPopupAnimationEnum = RSPopupAnimationEnum.Default)
        {
            //TODO
        }

        public void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType)
        {

        }
    }
}
