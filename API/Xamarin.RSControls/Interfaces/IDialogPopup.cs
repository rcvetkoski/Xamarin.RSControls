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
        RSPopupAnimationEnum RSPopupAnimationEnum { get; set; }
        RSPopupPositionEnum RSPopupPositionEnum { get; set; }
        RSPopupPositionSideEnum RSPopupPositionSideEnum { get; set; }
        RSPopupStyleEnum RSPopupStyleEnum { get; set; }
        bool UserSetSize { get; set; }
        bool UserSetMargin { get; set; }
        bool HasCloseButton { get; set; }
        int RightMargin { get; set; }
        int LeftMargin { get; set; }
        int TopMargin { get; set; }
        int BottomMargin { get; set; }
        float BorderRadius { get; set; }
        Forms.Color BorderFillColor { get; set; }
        float DimAmount { get; set; }
        bool ShadowEnabled { get; set; }
        bool IsModal { get; set; }
        Forms.View RelativeView { get; set; }
        float RSPopupOffsetX { get; set; }
        float RSPopupOffsetY { get; set; }
        Forms.View CustomView { get; set; }
        void AddAction(string title, RSPopupButtonTypeEnum rSPopupButtonType, Command command, object commandParameter);
        void ShowPopup();
    }

    public interface IHaveDialogPopup
    {
        string RSPopupTitle { get; set; }
        string RSPopupMessage { get; set; }
        Forms.Color RSPopupBackgroundColor { get; set; }
        RSPopupStyleEnum RSPopupStyleEnum { get; set; }
        bool IsSearchEnabled { get; set; }
        bool RsPopupSeparatorsEnabled { get; set; }
    }
}
