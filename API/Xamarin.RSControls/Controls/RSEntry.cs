using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSEntry : Entry, IHaveError
    {
        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSEntry), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSEntry), null);
        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSEntry), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }
    }
}
