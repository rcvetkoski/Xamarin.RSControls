
using Xamarin.Forms;

namespace Xamarin.RSControls.Controls
{
    public class RSEntryInputLayout: RSEntry
    {
        public RSEntryInputLayout()
        {
        }


        public static readonly BindableProperty ActivePlaceholderColorProperty = BindableProperty.Create(nameof(ActivePlaceholderColor),
            typeof(Color),
            typeof(RSEntry),
            Color.Accent);

        public static readonly BindableProperty FloatingHintEnabledProperty = BindableProperty.Create(nameof(FloatingHintEnabled),
            typeof(bool),
            typeof(RSEntry),
            true);

        /// <summary>
        ///     ActivePlaceholderColor summary. This is a bindable property.
        /// </summary>
        public Color ActivePlaceholderColor
        {
            get => (Color)GetValue(ActivePlaceholderColorProperty);
            set => SetValue(ActivePlaceholderColorProperty, value);
        }



        /// <summary>
        ///     <c>true</c> to float the hint into a label, otherwise <c>false</c>. This is a bindable property.
        /// </summary>
        public bool FloatingHintEnabled
        {
            get => (bool)GetValue(FloatingHintEnabledProperty);
            set => SetValue(FloatingHintEnabledProperty, value);
        }
    }
}
