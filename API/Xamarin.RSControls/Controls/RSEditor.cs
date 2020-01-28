using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSEditor : Editor, IHaveError, IRSControl, IDisposable
    {
        public RSEditor()
        {
            this.TextChanged += RSEditor_TextChanged;   
        }

        private void RSEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetListMaxHeight();
        }

        public static readonly BindableProperty IsPlaceholderAlwaysFloatingProperty = BindableProperty.Create("IsPlaceholderAlwaysFloating", typeof(bool), typeof(RSEditor), false);
        public bool IsPlaceholderAlwaysFloating
        {
            get { return (bool)GetValue(IsPlaceholderAlwaysFloatingProperty); }
            set { SetValue(IsPlaceholderAlwaysFloatingProperty, value); }
        }
        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSEditor), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly BindableProperty RSEntryStyleProperty = BindableProperty.Create("RSEntryStyle", typeof(RSEntryStyleSelectionEnum), typeof(RSEditor), RSEntryStyleSelectionEnum.OutlinedBorder);
        public RSEntryStyleSelectionEnum RSEntryStyle
        {
            get { return (RSEntryStyleSelectionEnum)GetValue(RSEntryStyleProperty); }
            set { SetValue(RSEntryStyleProperty, value); }
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSEditor), null);
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

        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSEditor), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        public static readonly BindableProperty HelperProperty = BindableProperty.Create("Helper", typeof(string), typeof(RSEditor), string.Empty);
        public string Helper
        {
            get { return (string)GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }

        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter", typeof(int), typeof(RSEditor), 0);
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public static readonly BindableProperty CounterMaxLengthProperty = BindableProperty.Create("CounterMaxLength", typeof(int), typeof(RSEditor), -1);
        public int CounterMaxLength
        {
            get { return (int)GetValue(CounterMaxLengthProperty); }
            set { SetValue(CounterMaxLengthProperty, value); }
        }

        //Icon
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create("LeadingIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon LeadingIcon
        {
            get { return (RSEntryIcon)GetValue(LeadingIconProperty); }
            set { SetValue(LeadingIconProperty, value); }
        }

        public static readonly BindableProperty TrailingIconProperty = BindableProperty.Create("TrailingIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon TrailingIcon
        {
            get { return (RSEntryIcon)GetValue(TrailingIconProperty); }
            set { SetValue(TrailingIconProperty, value); }
        }

        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon LeftIcon
        {
            get { return (RSEntryIcon)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon RightIcon
        {
            get { return (RSEntryIcon)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        public static readonly BindableProperty LeftHelpingIconProperty = BindableProperty.Create("LeftHelpingIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon LeftHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(LeftHelpingIconProperty); }
            set { SetValue(LeftHelpingIconProperty, value); }
        }

        public static readonly BindableProperty RightHelpingIconProperty = BindableProperty.Create("RightHelpingIcon", typeof(RSEntryIcon), typeof(RSEditor), null);
        public RSEntryIcon RightHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(RightHelpingIconProperty); }
            set { SetValue(RightHelpingIconProperty, value); }
        }


        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSEditor), 16f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create("Padding", typeof(Thickness), typeof(RSEditor), null);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        //Border Color
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(RSEditor), Color.DimGray);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        //Border Fill Color
        public static readonly BindableProperty BorderFillColorProperty = BindableProperty.Create("BorderFillColor", typeof(Color), typeof(RSEditor), Color.FromHex("#OA000000"));
        public Color BorderFillColor
        {
            get { return (Color)GetValue(BorderFillColorProperty); }
            set { SetValue(BorderFillColorProperty, value); }
        }
        //Active Color
        public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create("ActiveColor", typeof(Color), typeof(RSEditor), Color.FromHex("#3F51B5"));
        public Color ActiveColor
        {
            get { return (Color)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }
        //Error Color
        public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create("ErrorColor", typeof(Color), typeof(RSEditor), Color.FromHex("#f44336"));
        public Color ErrorColor
        {
            get { return (Color)GetValue(ErrorColorProperty); }
            set { SetValue(ErrorColorProperty, value); }
        }

        public bool IsPassword { get; set; } = false;

        //Shadow Enabled
        public static readonly BindableProperty ShadowEnabledProperty = BindableProperty.Create("ShadowEnabled", typeof(bool), typeof(RSEditor), false);
        public bool ShadowEnabled
        {
            get { return (bool)GetValue(ShadowEnabledProperty); }
            set { SetValue(ShadowEnabledProperty, value); }
        }


        public bool HasRighIconSeparator { get; set; }
        public bool HasLeftIconSeparator { get; set; }


        public static readonly BindableProperty LeftIconsProperty = BindableProperty.Create("LeftIcons", typeof(IList<RSEntryIcon>), typeof(RSEditor), new List<RSEntryIcon>());
        public IList<RSEntryIcon> LeftIcons
        {
            get { return (IList<RSEntryIcon>)GetValue(LeftIconsProperty); }
            set { SetValue(LeftIconsProperty, value); }
        }

        public static readonly BindableProperty MaxHeightProperty = BindableProperty.Create("MaxHeight", typeof(double), typeof(RSEditor), default(double));
        public double MaxHeight
        {
            get { return (double)GetValue(MaxHeightProperty); }
            set { SetValue(MaxHeightProperty, value); }
        }

        private void SetListMaxHeight()
        {
            if (MaxHeight != 0)
            {
                if (Height >= MaxHeight)
                    this.HeightRequest = MaxHeight;
                //else
                //    this.HeightRequest = Listheight;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (RightIcon != null)
                RightIcon.BindingContext = this.BindingContext;

            if (LeftIcon != null)
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

        public void Dispose()
        {
            this.TextChanged -= RSEditor_TextChanged;
        }
    }
}
