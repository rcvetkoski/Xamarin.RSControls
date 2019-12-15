using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.RSControls.Helpers
{
    public class RSEntryIcon : BindableObject
    {
        public RSEntryIcon()
        {
            CommandParameters = new List<object>();
        }

        public string Path { get; set; }
        public string Command { get; set; }

        public  BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(RSEntryIcon), default);
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

       // public static readonly BindableProperty CommandParametersProperty = BindableProperty.Create("CommandParameters", typeof(List<Binding>), typeof(RSEntryIcon), default, propertyChanged:OnPropChanged);

        //private static void OnPropChanged(BindableObject bindable, object oldValue, object newValue)
        //{
           
        //}

        public List<object> CommandParameters
        { get; set; }

        //{
        //    get { return (List<Binding>)GetValue(CommandParametersProperty); }
        //    set { SetValue(CommandParametersProperty, value); }
        //}
    }
}
