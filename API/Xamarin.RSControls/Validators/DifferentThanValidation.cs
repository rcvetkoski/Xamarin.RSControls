using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;

namespace Xamarin.RSControls.Validators
{
    public class DifferentThanValidation<T> : BindableObject, IValidation
    {
        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(T), typeof(DifferentThanValidation<T>), default);
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Message => $"Value must be different than {Value} !";


        public bool Validate(object value)
        {
            if (value?.ToString() == Value?.ToString())
                return false;
            else
                return true;
        }
    }
}
