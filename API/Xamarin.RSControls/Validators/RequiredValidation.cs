using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Validators
{
    public class RequiredValidation : IValidation
    {
        public string Message => "This field is required !";

        public bool Validate(string value)
        {
            return string.IsNullOrEmpty(value) ? false : true;
        }
    }
}
