using System;
using Xamarin.Forms;

namespace Xamarin.RSControls.Controls
{
    public class RSNumericUpDown : RSNumericEntry
    {
        public static readonly BindableProperty IncrementValueProperty = BindableProperty.Create("IncrementValue", typeof(double), typeof(RSNumericUpDown), (double)1);
        public double IncrementValue
        {
            get { return (double)GetValue(IncrementValueProperty); }
            set { SetValue(IncrementValueProperty, value); }
        }

        public void Increase()
        {
            if (Value == null)
                Value = Minimum > 0 ? Minimum : 0;

            var number = Convert.ToDouble(Value.ToString());
            number += IncrementValue;


            if (number > Maximum)
                number -= IncrementValue;

            Value = number;
        }

        public void Decrease()
        {
            if (Value == null)
                Value = Minimum > 0 ? Minimum : 0;

            var number = Convert.ToDouble(Value.ToString());
            number -= IncrementValue;

            if (number < Minimum)
                number += IncrementValue;

            Value = number;
        }
    }
}
