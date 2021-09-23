using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Validators
{
    public class CounterValidation : IValidation
    {
        public string Message => "Input too long !";
        public int CounterMaxLength;

        public bool Validate(object value)
        {
            if (value == null)
                return true;

            if (value is string)
            {
                if ((value as string).Length > CounterMaxLength)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }
    }
}
