using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Interfaces
{
    public interface IHaveError
    {
        string Error { get; set; }

        bool InValid { get; set; }

        bool CheckIsValid();
    }
}
