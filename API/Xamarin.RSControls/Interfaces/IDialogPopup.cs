using System;
namespace Xamarin.RSControls.Interfaces
{
    public interface IDialogPopup
    {
        float PositionX { get; set; }
        float PositionY { get; set; }

        void ShowPopup();
    }
}
