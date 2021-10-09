using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSEntry : Entry, IHaveError, IRSControl
    {
        public static readonly BindableProperty IsPlaceholderAlwaysFloatingProperty = BindableProperty.Create("IsPlaceholderAlwaysFloating", typeof(bool), typeof(RSEntry), false);
        public bool IsPlaceholderAlwaysFloating
        {
            get { return (bool)GetValue(IsPlaceholderAlwaysFloatingProperty); }
            set { SetValue(IsPlaceholderAlwaysFloatingProperty, value); }
        }
        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSEntry), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly BindableProperty RSEntryStyleProperty = BindableProperty.Create("RSEntryStyle", typeof(RSEntryStyleSelectionEnum), typeof(RSEntry), RSEntryStyleSelectionEnum.OutlinedBorder);
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
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create("LeadingIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon LeadingIcon
        {
            get { return (RSEntryIcon)GetValue(LeadingIconProperty); }
            set { SetValue(LeadingIconProperty, value); }
        }

        public static readonly BindableProperty TrailingIconProperty = BindableProperty.Create("TrailingIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon TrailingIcon
        {
            get { return (RSEntryIcon)GetValue(TrailingIconProperty); }
            set { SetValue(TrailingIconProperty, value); }
        }

        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon LeftIcon
        {
            get { return (RSEntryIcon)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon RightIcon
        {
            get { return (RSEntryIcon)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        public static readonly BindableProperty LeftHelpingIconProperty = BindableProperty.Create("LeftHelpingIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon LeftHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(LeftHelpingIconProperty); }
            set { SetValue(LeftHelpingIconProperty, value); }
        }

        public static readonly BindableProperty RightHelpingIconProperty = BindableProperty.Create("RightHelpingIcon", typeof(RSEntryIcon), typeof(RSEntry), null);
        public RSEntryIcon RightHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(RightHelpingIconProperty); }
            set { SetValue(RightHelpingIconProperty, value); }
        }


        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSEntry), 5f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        //Border width stroke
        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create("BorderWidth", typeof(float), typeof(RSEntry), 1f);
        public float BorderWidth
        {
            get { return (float)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        //Shadow radius
        public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create("ShadowRadius", typeof(float), typeof(RSEntry), 1f);
        public float ShadowRadius
        {
            get { return (float)GetValue(ShadowRadiusProperty); }
            set { SetValue(ShadowRadiusProperty, value); }
        }

        //Shadow Color
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create("ShadowColor", typeof(Color), typeof(RSEntry), Color.Gray);
        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create("Padding", typeof(Thickness), typeof(RSEntry), default);
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
        //Border Fill Color
        public static readonly BindableProperty BorderFillColorProperty = BindableProperty.Create("BorderFillColor", typeof(Color), typeof(RSEntry), null);

        public Color BorderFillColor
        {
            get { return (Color)GetValue(BorderFillColorProperty); }
            set { SetValue(BorderFillColorProperty, value); }
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

        //Shadow Enabled
        public static readonly BindableProperty ShadowEnabledProperty = BindableProperty.Create("ShadowEnabled", typeof(bool), typeof(RSEntry), false);
        public bool ShadowEnabled
        {
            get { return (bool)GetValue(ShadowEnabledProperty); }
            set { SetValue(ShadowEnabledProperty, value); }
        }

        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(RSEntry), Color.DimGray);
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }


        //Placeholder Style
        public static readonly BindableProperty PlaceholderStyleProperty = BindableProperty.Create("PlaceholderStyle", typeof(AssistiveTextStyle), typeof(RSEntry), new AssistiveTextStyle());
        public AssistiveTextStyle PlaceholderStyle
        {
            get { return (AssistiveTextStyle)GetValue(PlaceholderStyleProperty); }
            set { SetValue(PlaceholderStyleProperty, value); }
        }

        //Helper Style
        public static readonly BindableProperty HelperStyleProperty = BindableProperty.Create("HelperStyle", typeof(AssistiveTextStyle), typeof(RSEntry), new AssistiveTextStyle());
        public AssistiveTextStyle HelperStyle
        {
            get { return (AssistiveTextStyle)GetValue(HelperStyleProperty); }
            set { SetValue(HelperStyleProperty, value); }
        }

        //Counter Style
        public static readonly BindableProperty CounterStyleProperty = BindableProperty.Create("CounterStyle", typeof(AssistiveTextStyle), typeof(RSEntry), new AssistiveTextStyle());
        public AssistiveTextStyle CounterStyle
        {
            get { return (AssistiveTextStyle)GetValue(CounterStyleProperty); }
            set { SetValue(CounterStyleProperty, value); }
        }

        //Error Style
        public static readonly BindableProperty ErrorStyleProperty = BindableProperty.Create("ErrorStyle", typeof(AssistiveTextStyle), typeof(RSEntry), new AssistiveTextStyle());
        public AssistiveTextStyle ErrorStyle
        {
            get { return (AssistiveTextStyle)GetValue(ErrorStyleProperty); }
            set { SetValue(ErrorStyleProperty, value); }
        }

        public bool HasRighIconSeparator { get; set; }
        public bool HasLeftIconSeparator { get; set; }


        public static readonly BindableProperty LeftIconsProperty = BindableProperty.Create("LeftIcons", typeof(IList<RSEntryIcon>), typeof(RSEntry), new List<RSEntryIcon>());
        public IList<RSEntryIcon> LeftIcons
        {
            get { return (IList<RSEntryIcon>)GetValue(LeftIconsProperty); }
            set { SetValue(LeftIconsProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if(RightIcon != null)
                RightIcon.BindingContext = this.BindingContext;

            if(LeftIcon != null)
                LeftIcon.BindingContext = this.BindingContext;

            if (TrailingIcon != null)
                TrailingIcon.BindingContext = this.BindingContext;

            if (LeadingIcon != null)
                LeadingIcon.BindingContext = this.BindingContext;

            if (LeftHelpingIcon != null)
                LeftHelpingIcon.BindingContext = this.BindingContext;

            if (RightHelpingIcon != null)
                RightHelpingIcon.BindingContext = this.BindingContext;
        }
    }
}