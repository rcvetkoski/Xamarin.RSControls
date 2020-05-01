using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;

namespace Xamarin.RSControls.Interfaces
{
    public interface IDialogPopup
    {
        string Title { get; set; }
        string Message { get; set; }
        int PositionX { get; set; }
        int PositionY { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        bool UserSetPosition { get; set; }
        bool UserSetSize { get; set; }
        float BorderRadius { get; set; }
        Forms.Color BorderFillColor { get; set; }
        float DimAmount { get; set; }
        bool ShadowEnabled { get; set; }
        Forms.View RelativeView { get; set; }
        Forms.View CustomView { get; set; }
        void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter);
        void ShowPopup();
    }
}
