using System;
namespace Xamarin.RSControls.Interfaces
{
    public interface IDialogPopup
    {
        string Title { get; set; }
        string Message { get; set; }
        float PositionX { get; set; }
        float PositionY { get; set; }
        float BorderRadius { get; set; }
        bool ShadowEnabled { get; set; }
        void ShowPopup();
    }

}
