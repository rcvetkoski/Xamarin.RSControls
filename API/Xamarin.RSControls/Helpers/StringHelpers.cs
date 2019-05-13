using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.RSControls.Helpers
{
    public static class StringHelpers
    {
        public static double? ToNullableDouble(this string s)
        {
            double i;
            if (double.TryParse(s, out i)) return i;
            return null;
        }
    }
}
