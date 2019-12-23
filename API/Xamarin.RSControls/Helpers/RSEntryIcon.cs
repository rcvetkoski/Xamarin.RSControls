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
        public object Source { get; set; }

        //Icon Color
        public static readonly BindableProperty IconColorProperty = BindableProperty.Create("IconColor", typeof(Color), typeof(RSEntryIcon), Color.DimGray);
        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        //Icon Height
        public static readonly BindableProperty IconSizeProperty = BindableProperty.Create("IconSize", typeof(double), typeof(RSEntryIcon), 22.0);
        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

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