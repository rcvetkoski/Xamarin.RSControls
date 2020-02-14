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


        public RSPopup(object obj)
        {
            var lol = obj;
            service = DependencyService.Get<IDialogPopup>();
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
        }

        public void SetMessage(string message)
        {
            Message = message;
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
