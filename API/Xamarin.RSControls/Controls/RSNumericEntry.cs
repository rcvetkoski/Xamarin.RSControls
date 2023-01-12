using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.RSControls.Helpers;

namespace Xamarin.RSControls.Controls
{
    public class RSNumericEntry : RSEntry
    {
        public RSNumericEntry()
        {
            this.Keyboard = Keyboard.Numeric;
        }

        //https://bugzilla.xamarin.com/show_bug.cgi?id=52708
        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(object), typeof(RSNumericEntry), default, BindingMode.TwoWay);
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly BindableProperty CustomUnitProperty = BindableProperty.Create("CustomUnit", typeof(string), typeof(RSNumericEntry), default(string));
        public string CustomUnit
        {
            get { return (string)GetValue(CustomUnitProperty); }
            set { SetValue(CustomUnitProperty, value); }
        }

        public static readonly BindableProperty NumberDecimalDigitsProperty = BindableProperty.Create("NumberDecimalDigits", typeof(int), typeof(RSNumericEntry), 0);
        public int NumberDecimalDigits
        {
            get { return (int)GetValue(NumberDecimalDigitsProperty); }
            set { SetValue(NumberDecimalDigitsProperty, value); }
        }

        public static readonly BindableProperty MinimumProperty = BindableProperty.Create("Minimum", typeof(double), typeof(RSNumericEntry), -double.MaxValue);
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly BindableProperty MaximumProperty = BindableProperty.Create("Maximum", typeof(double), typeof(RSNumericEntry), double.MaxValue);
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly BindableProperty HideTrailingZerosProperty = BindableProperty.Create("HideTrailingZeros", typeof(bool), typeof(RSNumericEntry), false);
        public bool HideTrailingZeros
        {
            get { return (bool)GetValue(HideTrailingZerosProperty); }
            set { SetValue(HideTrailingZerosProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == "IsFocused"
                || propertyName == "Text"
                || propertyName == "Value"
                || propertyName == "NumberDecimalDigits"
                || propertyName == "CustomUnit"
                || propertyName == "Minimum"
                || propertyName == "Maximum"
                || propertyName == "HideTrailingZeros")
            {
                UpdateTextProperty(propertyName);
            }

            base.OnPropertyChanged(propertyName);
        }

        string tempText = string.Empty;
        bool canUpdateProperty = true;

        private void UpdateTextProperty(string propertyName)
        {
            if (!canUpdateProperty)
                return;

            canUpdateProperty = false;

            if (propertyName == "IsFocused")
            {
                if (IsFocused)
                {
                    if (Value != null)
                    {
                        if (Value.ToString() == "0")
                        {
                            tempText = Value.ToString();
                            this.Text = "";
                        }
                        else
                        {
                            if (NumberDecimalDigits != 0)
                                this.Text = Value != null ? Math.Round(Convert.ToDouble(Value.ToString()), NumberDecimalDigits).ToString() : "";
                            else
                                this.Text = Value.ToString();
                        }
                    }
                    else
                        this.Text = "";
                }
                else
                {
                    if (tempText == "0" && this.Text.ToString() == "")
                        this.Value = tempText.ToNullableDouble();
                    else
                    {
                        if (this.Text.ToNullableDouble() < Minimum)
                            Value = Minimum;
                        else if (this.Text.ToNullableDouble() > Maximum)
                            Value = Maximum;
                        else
                        {
                            if (NumberDecimalDigits != 0 && this.Text != "")
                                this.Value = Math.Round(Convert.ToDouble(this.Text), NumberDecimalDigits);
                            else
                                this.Value = this.Text.ToNullableDouble();
                        }
                    }

                    SetTextUnfocused();
                }
            }
            else if (propertyName == "Text")
            {
                if (UpdateSourceTrigger == Enums.UpdateSourceTriggerEnum.Default)
                    this.Value = this.Text.ToNullableDouble();
            }
            else if (propertyName == "Value")
            {
                if (IsFocused)
                {
                    if (Value == null || Value.ToString() == "0")
                        this.Text = "";
                    else
                    {
                        this.Text = Value.ToString();
                    }
                }
                else
                {
                    SetTextUnfocused();
                }
            }
            else if (propertyName == "CustomUnit" || propertyName == "NumberDecimalDigits")
            {
                SetTextUnfocused();
            }
            else if (propertyName == "HideTrailingZeros")
            {
                SetTextUnfocused();
            }

            canUpdateProperty = true;
        }

        private void SetTextUnfocused()
        {
            if (Value != null)
            {
                string str = string.Empty;
                if (NumberDecimalDigits != 0)
                    str = string.Format("{0:N" + this.NumberDecimalDigits + "}", this.Value);
                else
                    str = this.Value.ToString();

                if (IsFocused)
                    this.Value = str.ToNullableDouble();

                str = str.TrimEnd(' ');
                if (HideTrailingZeros && NumberDecimalDigits > 0)
                {
                    str = str.TrimEnd('0');
                    str = str.TrimEnd('.');
                    str = str.TrimEnd(',');
                }

                if (!string.IsNullOrEmpty(this.CustomUnit))
                    str = string.Format("{0} {1}", str, this.CustomUnit);

                this.Text = str;
            }
        }
    }
}
