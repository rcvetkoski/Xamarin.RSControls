using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Validators
{
    public class RequiredValidation : IValidation
    {
        public string Message => "This field is required !";

        public bool Validate(object value)
        {
            if (value == null)
                return false;
            else if (value is string)
            {
                return string.IsNullOrWhiteSpace(value as string) ? false : true;
            }
            else
                return true;
        }
    }
}
