using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Xamarin.RSControls.Helpers
{
    public class TypeExtensions
    {
        public static string GetPropValue(object src, string propName)
        {
            var val = src.GetType().GetRuntimeProperty(propName).GetValue(src, null);
            return val != null ? val.ToString() : string.Empty;
        }

        //Return page of element
        public static Page GetParentPage(VisualElement element)
        {
            if (element != null)
            {
                var parent = element.Parent;
                while (parent != null)
                {
                    if (parent is Page)
                    {
                        return parent as Page;
                    }
                    parent = parent.Parent;
                }
            }
            return null;
        }
    }
}
