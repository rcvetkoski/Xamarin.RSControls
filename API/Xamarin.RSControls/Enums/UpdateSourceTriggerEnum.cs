namespace Xamarin.RSControls.Enums
{
    //
    // Summary:
    //     Describes the timing of binding source updates.
    public enum UpdateSourceTriggerEnum
    {
        // Updates the binding source whenever new character is typed.
        Default = 0,

        // Updates the binding source whenever the binding target element loses focus.
        OnFocusLost = 1,
    }
}
