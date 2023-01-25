using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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

        // View
        public static readonly BindableProperty ViewProperty = BindableProperty.Create("View", typeof(View), typeof(RSEntryIcon), null);
        public View View
        {
            get { return (View)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
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

        //Icon Width
        public static readonly BindableProperty IconWidthProperty = BindableProperty.Create("IconWidth", typeof(double), typeof(RSEntryIcon), 22.0);
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        //Icon Height
        public static readonly BindableProperty IconHeightProperty = BindableProperty.Create("IconHeight", typeof(double), typeof(RSEntryIcon), 22.0);
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
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


        public IList<RSCommandParameter> Bindings { get; } = new List<RSCommandParameter>();
    }

    public class RSCommandParameter : BindableObject
    {
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(RSCommandParameter), default);
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value);}
        }

       
    }
}