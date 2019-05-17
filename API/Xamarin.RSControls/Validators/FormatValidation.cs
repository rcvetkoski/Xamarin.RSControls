using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Xamarin.RSControls.Validators
{
    public class FormatValidation : IValidation
    {
        public string Message => "Invalid format !";
        public string Format { get; set; }

        public bool Validate(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Regex format = new Regex(Format);

                return format.IsMatch(value);
            }
            else
            {
                return false;
            }
        }
    }
}
