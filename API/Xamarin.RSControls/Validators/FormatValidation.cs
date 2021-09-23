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

        public bool Validate(object value)
        {
            if (value == null)
                return false;

            if (value is string)
            {
                if (!string.IsNullOrEmpty(value as string))
                {
                    Regex format = new Regex(Format);

                    return format.IsMatch(value as string);
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }
    }
}
