using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace Xamarin.RSControls.Helpers
{
    [ContentProperty(nameof(Bindings))]
    public class RSEntryIcon : BindableObject
    {
        public RSEntryIcon()
        {
        }

        public string Path { get; set; }
        public string Command { get; set; }


        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(RSEntryIcon), default);
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        public static readonly BindableProperty CommandParametersProperty = BindableProperty.Create("CommandParameters", typeof(List<object>), typeof(RSEntryIcon), new List<object>());
        public List<object> CommandParameters
        {
            get { return (List<object>)GetValue(CommandParametersProperty); }
            set { SetValue(CommandParametersProperty, value); }
        }


        public IList<Binding> Bindings { get; } = new List<Binding>();

    }
}