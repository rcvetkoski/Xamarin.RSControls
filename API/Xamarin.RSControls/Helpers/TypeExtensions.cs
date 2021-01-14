using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Xamarin.RSControls.Helpers
{
    public class TypeExtensions
    {
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetRuntimeProperty(propName).GetValue(src, null);
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
