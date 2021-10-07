using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSDatePicker : DatePicker, IHaveError, IRSControl
    {
        public RSDatePicker()
        {
            this.TextColor = Color.Black;
            this.hasCustomFormat = false;
        }

        //IsPlaceholderAlwaysFloating
        public static readonly BindableProperty IsPlaceholderAlwaysFloatingProperty = BindableProperty.Create("IsPlaceholderAlwaysFloating", typeof(bool), typeof(RSDatePicker), false);
        public bool IsPlaceholderAlwaysFloating
        {
            get { return (bool)GetValue(IsPlaceholderAlwaysFloatingProperty); }
            set { SetValue(IsPlaceholderAlwaysFloatingProperty, value); }
        }

        //Has Custom Format
        private bool hasCustomFormat { get; set; }

        //Placeholder
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(RSDatePicker), null);
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        //Placeholder color
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(RSDatePicker), Color.Gray);
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        //IsPassword
        public bool IsPassword { get; set; } = false;

        //Is Clearable
        public static readonly BindableProperty IsClearableProperty = BindableProperty.Create("IsClearable", typeof(bool), typeof(RSDatePicker), true);
        public bool IsClearable
        {
            get { return (bool)GetValue(IsClearableProperty); }
            set { SetValue(IsClearableProperty, value); }
        }

        //Is NullableDate
        public static readonly BindableProperty NullableDateProperty = BindableProperty.Create(nameof(NullableDate), typeof(DateTime?), typeof(RSDatePicker), null, defaultBindingMode: BindingMode.TwoWay);
        public DateTime? NullableDate
        {
            get { return (DateTime?)GetValue(NullableDateProperty); }
            set { SetValue(NullableDateProperty, value); }
        }

        //Date Selection mode
        public static readonly BindableProperty DateSelectionModeProperty = BindableProperty.Create("DateSelectionMode", typeof(DateSelectionModeEnum), typeof(RSDatePicker), DateSelectionModeEnum.Default);
        public DateSelectionModeEnum DateSelectionMode
        {
            get { return (DateSelectionModeEnum)GetValue(DateSelectionModeProperty); }
            set { SetValue(DateSelectionModeProperty, value); }
        }

        //Entry style
        public static readonly BindableProperty RSEntryStyleProperty = BindableProperty.Create("RSEntryStyle", typeof(RSEntryStyleSelectionEnum), typeof(RSDatePicker), RSEntryStyleSelectionEnum.OutlinedBorder);
        public RSEntryStyleSelectionEnum RSEntryStyle
        {
            get { return (RSEntryStyleSelectionEnum)GetValue(RSEntryStyleProperty); }
            set { SetValue(RSEntryStyleProperty, value); }
        }

        //Error
        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSDatePicker), null);
        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        //Has error
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

        public static readonly BindableProperty HelperProperty = BindableProperty.Create("Helper", typeof(string), typeof(RSDatePicker), string.Empty);
        public string Helper
        {
            get { return (string)GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }

        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter", typeof(int), typeof(RSDatePicker), 0);
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public static readonly BindableProperty CounterMaxLengthProperty = BindableProperty.Create("CounterMaxLength", typeof(int), typeof(RSDatePicker), -1);
        public int CounterMaxLength
        {
            get { return (int)GetValue(CounterMaxLengthProperty); }
            set { SetValue(CounterMaxLengthProperty, value); }
        }

        //Icon
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create("LeadingIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon LeadingIcon
        {
            get { return (RSEntryIcon)GetValue(LeadingIconProperty); }
            set { SetValue(LeadingIconProperty, value); }
        }

        public static readonly BindableProperty TrailingIconProperty = BindableProperty.Create("TrailingIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon TrailingIcon
        {
            get { return (RSEntryIcon)GetValue(TrailingIconProperty); }
            set { SetValue(TrailingIconProperty, value); }
        }

        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon LeftIcon
        {
            get { return (RSEntryIcon)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon RightIcon
        {
            get { return (RSEntryIcon)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        public static readonly BindableProperty LeftHelpingIconProperty = BindableProperty.Create("LeftHelpingIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon LeftHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(LeftHelpingIconProperty); }
            set { SetValue(LeftHelpingIconProperty, value); }
        }

        public static readonly BindableProperty RightHelpingIconProperty = BindableProperty.Create("RightHelpingIcon", typeof(RSEntryIcon), typeof(RSDatePicker), null);
        public RSEntryIcon RightHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(RightHelpingIconProperty); }
            set { SetValue(RightHelpingIconProperty, value); }
        }

        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create("HorizontalTextAlignment", typeof(TextAlignment), typeof(RSDatePicker), TextAlignment.Start);
        public TextAlignment HorizontalTextAlignment
        {
            get { return (TextAlignment)GetValue(HorizontalTextAlignmentProperty); }
            set { SetValue(HorizontalTextAlignmentProperty, value); }
        }

        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSDatePicker), 8f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create("Padding", typeof(Thickness), typeof(RSDatePicker), null);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        //Border Color
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(RSDatePicker), Color.DimGray);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        //Border width stroke
        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create("BorderWidth", typeof(float), typeof(RSDatePicker), 1f);
        public float BorderWidth
        {
            get { return (float)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        //Border Fill Color
        public static readonly BindableProperty BorderFillColorProperty = BindableProperty.Create("BorderFillColor", typeof(Color), typeof(RSDatePicker), null);
        public Color BorderFillColor
        {
            get { return (Color)GetValue(BorderFillColorProperty); }
            set { SetValue(BorderFillColorProperty, value); }
        }
        //Active Color
        public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create("ActiveColor", typeof(Color), typeof(RSDatePicker), Color.FromHex("#3F51B5"));
        public Color ActiveColor
        {
            get { return (Color)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }
        //Error Color
        public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create("ErrorColor", typeof(Color), typeof(RSDatePicker), Color.FromHex("#f44336"));
        public Color ErrorColor
        {
            get { return (Color)GetValue(ErrorColorProperty); }
            set { SetValue(ErrorColorProperty, value); }
        }

        public static readonly BindableProperty LeftIconsProperty = BindableProperty.Create("LeftIcons", typeof(IList<RSEntryIcon>), typeof(RSDatePicker), new List<RSEntryIcon>());
        public IList<RSEntryIcon> LeftIcons
        {
            get { return (IList<RSEntryIcon>)GetValue(LeftIconsProperty); }
            set { SetValue(LeftIconsProperty, value); }
        }

        //Shadow Enabled
        public static readonly BindableProperty ShadowEnabledProperty = BindableProperty.Create("ShadowEnabled", typeof(bool), typeof(RSDatePicker), false);
        public bool ShadowEnabled
        {
            get { return (bool)GetValue(ShadowEnabledProperty); }
            set { SetValue(ShadowEnabledProperty, value); }
        }

        //Shadow radius
        public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create("ShadowRadius", typeof(float), typeof(RSDatePicker), 1f);
        public float ShadowRadius
        {
            get { return (float)GetValue(ShadowRadiusProperty); }
            set { SetValue(ShadowRadiusProperty, value); }
        }

        //Shadow Color
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create("ShadowColor", typeof(Color), typeof(RSDatePicker), Color.Gray);
        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        //Placeholder Style
        public static readonly BindableProperty PlaceholderStyleProperty = BindableProperty.Create("PlaceholderStyle", typeof(AssistiveTextStyle), typeof(RSDatePicker), new AssistiveTextStyle());
        public AssistiveTextStyle PlaceholderStyle
        {
            get { return (AssistiveTextStyle)GetValue(PlaceholderStyleProperty); }
            set { SetValue(PlaceholderStyleProperty, value); }
        }

        //Helper Style
        public static readonly BindableProperty HelperStyleProperty = BindableProperty.Create("HelperStyle", typeof(AssistiveTextStyle), typeof(RSDatePicker), new AssistiveTextStyle());
        public AssistiveTextStyle HelperStyle
        {
            get { return (AssistiveTextStyle)GetValue(HelperStyleProperty); }
            set { SetValue(HelperStyleProperty, value); }
        }

        //Counter Style
        public static readonly BindableProperty CounterStyleProperty = BindableProperty.Create("CounterStyle", typeof(AssistiveTextStyle), typeof(RSDatePicker), new AssistiveTextStyle());
        public AssistiveTextStyle CounterStyle
        {
            get { return (AssistiveTextStyle)GetValue(CounterStyleProperty); }
            set { SetValue(CounterStyleProperty, value); }
        }

        //Error Style
        public static readonly BindableProperty ErrorStyleProperty = BindableProperty.Create("ErrorStyle", typeof(AssistiveTextStyle), typeof(RSDatePicker), new AssistiveTextStyle());
        public AssistiveTextStyle ErrorStyle
        {
            get { return (AssistiveTextStyle)GetValue(ErrorStyleProperty); }
            set { SetValue(ErrorStyleProperty, value); }
        }

        public bool HasRighIconSeparator { get; set; }
        public bool HasLeftIconSeparator { get; set; }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            if( propertyName == FormatProperty.PropertyName)
            {
                hasCustomFormat = true;
            }
            //Update or set NullableDate when Date binded instead of NullableDate
            //if (propertyName == DateProperty.PropertyName)
            //{
            //    AssignValue();
            //}

            //if (propertyName == NullableDateProperty.PropertyName && NullableDate.HasValue)
            //{
            //    Date = NullableDate.Value;
            //    if (Date.ToString(this.Format) == DateTime.Now.ToString(this.Format))
            //    {
            //        //this code was done because when date selected is the actual date the"DateProperty" does not raise  
            //        UpdateDate();
            //    }
            //}
        }

        public void CleanDate()
        {
            NullableDate = null;
        }
        public void AssignValue()
        {
            NullableDate = Date;
        }

        public bool HasCustomFormat()
        {
            return hasCustomFormat;
        }

        //Resize control when width or height set to auto and text changes
        public void DoInvalidate()
        {
            this.InvalidateMeasure();
        }
    }
}

