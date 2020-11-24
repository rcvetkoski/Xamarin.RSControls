using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.RSControls.Controls;

namespace Xamarin.RSControls.Controls
{

    public class RSTabbedPage : Forms.TabbedPage
    {
        public RSTabbedPage()
        {
            
        }

        public static readonly BindableProperty TabbarPlacementProperty = BindableProperty.Create("TabbarPlacement", typeof(TabbarPlacementEnum), typeof(RSTabbedPage), TabbarPlacementEnum.Top);
        public TabbarPlacementEnum TabbarPlacement
        {
            get { return (TabbarPlacementEnum)GetValue(TabbarPlacementProperty); }
            set
            {
                SetValue(TabbarPlacementProperty, value);

                if(value == TabbarPlacementEnum.Bottom)
                {
                    On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
                }
            }
        }

        //Icon
        public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(string), typeof(RSTabbedPage), null);
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == "TabbarPlacement")
            {
                if (TabbarPlacement == TabbarPlacementEnum.Bottom)
                {
                    On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
                }
            }
        }
    }

    public enum TabbarPlacementEnum
    {
        Top,
        Bottom
    }
}
