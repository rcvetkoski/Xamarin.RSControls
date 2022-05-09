using System;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;

namespace Xamarin.RSControls.Controls
{
    public class RSNumericUpDown : RSNumericEntry
    {
        public RSNumericUpDown()
        {
            RightIcon = new Helpers.RSEntryIcon()
            {
                View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/plus.svg" },
                Command = "Increase",
                Source = this
            };
            RightHelpingIcon = new Helpers.RSEntryIcon()
            {
                View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/minus.svg" },
                Command = "Decrease",
                Source = this
            };
        }

        public static readonly BindableProperty IncrementValueProperty = BindableProperty.Create("IncrementValue", typeof(double), typeof(RSNumericUpDown), (double)1);
        public double IncrementValue
        {
            get { return (double)GetValue(IncrementValueProperty); }
            set { SetValue(IncrementValueProperty, value); }
        }

        public static readonly BindableProperty RSNumericUpDownStyleProperty = BindableProperty.Create("RSNumericUpDownStyle", typeof(RSNumericUpDownStyleEnum), typeof(RSNumericUpDown), RSNumericUpDownStyleEnum.Right,
            BindingMode.OneWay, null, propertyChanged: OnRSNumericUpDownStyleChanged);
        public RSNumericUpDownStyleEnum RSNumericUpDownStyle
        {
            get { return (RSNumericUpDownStyleEnum)GetValue(RSNumericUpDownStyleProperty); }
            set { SetValue(RSNumericUpDownStyleProperty, value); }
        }
        static void OnRSNumericUpDownStyleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Set default icons
            RSNumericUpDownStyleEnum value = (RSNumericUpDownStyleEnum)newValue;
            RSNumericUpDown rSNumericUpDown = (RSNumericUpDown)bindable;

            if (value == Enums.RSNumericUpDownStyleEnum.Right)
            {
                if (rSNumericUpDown.RightIcon == null)
                {
                    rSNumericUpDown.RightIcon = new Helpers.RSEntryIcon()
                    {
                        View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/plus.svg" },
                        Command = "Increase",
                        Source = rSNumericUpDown
                    };
                }

                if (rSNumericUpDown.RightIcon != null && rSNumericUpDown.RightHelpingIcon == null)
                {
                    rSNumericUpDown.RightHelpingIcon = new Helpers.RSEntryIcon()
                    {
                        View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/minus.svg" },
                        Command = "Decrease",
                        Source = rSNumericUpDown
                    };
                }
            }
            else if (value == Enums.RSNumericUpDownStyleEnum.Split)
            {
                rSNumericUpDown.RightHelpingIcon = null;
                rSNumericUpDown.LeftHelpingIcon = null;

                rSNumericUpDown.LeftIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/minus.svg" },
                    Command = "Decrease",
                    Source = rSNumericUpDown
                };

                rSNumericUpDown.RightIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/plus.svg" },
                    Command = "Increase",
                    Source = rSNumericUpDown
                };
            }
            else
            {
                rSNumericUpDown.RightIcon = null;
                rSNumericUpDown.RightHelpingIcon = null;

                rSNumericUpDown.LeftIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/minus.svg" },
                    Command = "Decrease",
                    Source = rSNumericUpDown
                };

                rSNumericUpDown.LeftHelpingIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/plus.svg" },
                    Command = "Increase",
                    Source = rSNumericUpDown
                };
            }
        }

        public void Increase()
        {
            this.Unfocus();
            double number;

            if (Value == null)
                number = Minimum > 0 ? Minimum : 0;
            else
                number = Convert.ToDouble(Value.ToString());


            number += IncrementValue;

            if (number > Maximum)
                number -= IncrementValue;

            Value = number;
        }

        public void Decrease()
        {
            this.Unfocus();

            double number;

            if (Value == null)
                number = Minimum > 0 ? Minimum : 0;
            else
                number = Convert.ToDouble(Value.ToString());

            number -= IncrementValue;

            if (number < Minimum)
                number += IncrementValue;

            Value = number;
        }
    }
}
