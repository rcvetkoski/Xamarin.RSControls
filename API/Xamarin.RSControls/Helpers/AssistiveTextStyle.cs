using Xamarin.Forms;

namespace Xamarin.RSControls.Helpers
{
    public class AssistiveTextStyle : BindableObject
    {
        //Shadow Color
        public static readonly BindableProperty FontColorProperty = BindableProperty.Create("FontColor", typeof(Color), typeof(AssistiveTextStyle), Color.Gray);
        public Color FontColor
        {
            get { return (Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }

        //Font Familly
        public static readonly BindableProperty FontFamillyProperty = BindableProperty.Create("FontFamily", typeof(string), typeof(AssistiveTextStyle), null);
        public string FontFamily
        {
            get { return (string)GetValue(FontFamillyProperty); }
            set { SetValue(FontFamillyProperty, value); }
        }

        //Font Weight
        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(AssistiveTextStyle), FontAttributes.None);
        public FontAttributes FontAttributes
        {
            get { return (FontAttributes)GetValue(FontAttributesProperty); }
            set { SetValue(FontAttributesProperty, value); }
        }
    }
}
