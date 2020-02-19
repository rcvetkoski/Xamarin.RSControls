namespace Xamarin.RSControls.Interfaces
{
    public interface IDialogPopup
    {
        string Title { get; set; }
        string Message { get; set; }
        float PositionX { get; set; }
        float PositionY { get; set; }
        float BorderRadius { get; set; }
        Forms.Color BackgroundColor { get; set; }
        float DimAmount { get; set; }
        bool ShadowEnabled { get; set; }
        Forms.View RelativeView { get; set; }
        Forms.View CustomView { get; set; }
        void ShowPopup();
    }

}
