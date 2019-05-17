using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Validators
{
    public interface IValidation
    {
        bool Validate(string value);

        string Message { get; }
    }
}
