using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;

namespace Xamarin.RSControls.Controls
{
    public class RSEntry : Entry, IRSControl
    {
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(RSEntry), Color.Black);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSEntry), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }
    }
}
