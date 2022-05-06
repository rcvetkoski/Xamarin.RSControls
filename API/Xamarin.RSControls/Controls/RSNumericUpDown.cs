using System;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;

namespace Xamarin.RSControls.Controls
{
    public class RSNumericUpDown : RSNumericEntry
    {
        public RSNumericUpDown()
        {

        }

        public static readonly BindableProperty IncrementValueProperty = BindableProperty.Create("IncrementValue", typeof(double), typeof(RSNumericUpDown), (double)1);
        public double IncrementValue
        {
            get { return (double)GetValue(IncrementValueProperty); }
            set { SetValue(IncrementValueProperty, value); }
        }

        public static readonly BindableProperty RSNumericUpDownStyleProperty = BindableProperty.Create("RSNumericUpDownStyle", typeof(RSNumericUpDownStyleEnum), typeof(RSNumericUpDown), RSNumericUpDownStyleEnum.Right);
        public RSNumericUpDownStyleEnum RSNumericUpDownStyle
        {
            get { return (RSNumericUpDownStyleEnum)GetValue(RSNumericUpDownStyleProperty); }
            set { SetValue(RSNumericUpDownStyleProperty, value); }
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
