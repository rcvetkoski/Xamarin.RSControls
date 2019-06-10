using System;
using System.Linq;
using System.Reflection;

namespace Xamarin.RSControls.Helpers
{
    public class TypeExtensions
    {
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetRuntimeProperty(propName).GetValue(src, null);
        }
    }
}
