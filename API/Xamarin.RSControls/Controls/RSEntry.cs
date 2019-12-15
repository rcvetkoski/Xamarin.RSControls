using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSEntry : Entry, IHaveError, IRSControl
    {
        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSEntry), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly BindableProperty RSEntryStyleProperty = BindableProperty.Create("RSEntryStyle", typeof(RSEntryStyleSelectionEnum), typeof(RSEntry), RSEntryStyleSelectionEnum.Default);
        public RSEntryStyleSelectionEnum RSEntryStyle
        {
            get { return (RSEntryStyleSelectionEnum)GetValue(RSEntryStyleProperty); }
            set { SetValue(RSEntryStyleProperty, value); }
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSEntry), null);
        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public bool HasError
        {
            get
            {
                if (this.Behaviors.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSEntry), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        public static readonly BindableProperty HelperProperty = BindableProperty.Create("Helper", typeof(string), typeof(RSEntry), string.Empty);
        public string Helper
        {
            get { return (string)GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }

        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter", typeof(int), typeof(RSEntry), 0);
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public static readonly BindableProperty CounterMaxLengthProperty = BindableProperty.Create("CounterMaxLength", typeof(int), typeof(RSEntry), -1);
        public int CounterMaxLength
        {
            get { return (int)GetValue(CounterMaxLengthProperty); }
            set { SetValue(CounterMaxLengthProperty, value); }
        }

        //Icon
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create("LeadingIcon", typeof(string), typeof(RSEntry), null);
        public string LeadingIcon
        {
            get { return (string)GetValue(LeadingIconProperty); }
            set { SetValue(LeadingIconProperty, value); }
        }
        public static readonly BindableProperty TrailingIconProperty = BindableProperty.Create("TrailingIconfwefew", typeof(string), typeof(RSEntry), null);
        public string TrailingIcon
        {
            get { return (string)GetValue(TrailingIconProperty); }
            set { SetValue(TrailingIconProperty, value); }
        }

        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(string), typeof(RSEntry), null);
        public string LeftIcon
        {
            get { return (string)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon RightIcon
        {
            get { return (RSEntryIcon)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        //Icon Color
        public static readonly BindableProperty IconColorProperty = BindableProperty.Create("IconColor", typeof(Color), typeof(RSEntry), Color.DimGray);
        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        //Icon Height
        public static readonly BindableProperty IconSizeProperty = BindableProperty.Create("IconSize", typeof(double), typeof(RSEntry), 22.0);
        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSEntry), 16f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create("Padding", typeof(Thickness), typeof(RSEntry), null);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        //Border Color
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(RSEntry), Color.DimGray);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        //Active Color
        public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create("ActiveColor", typeof(Color), typeof(RSEntry), Color.FromHex("#3F51B5"));
        public Color ActiveColor
        {
            get { return (Color)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }
        //Error Color
        public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create("ErrorColor", typeof(Color), typeof(RSEntry), Color.FromHex("#f44336"));
        public Color ErrorColor
        {
            get { return (Color)GetValue(ErrorColorProperty); }
            set { SetValue(ErrorColorProperty, value); }
        }
    }
}