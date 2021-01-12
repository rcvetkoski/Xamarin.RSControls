using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.RSControls.Controls
{
    public class RSTabbedViews : Forms.StackLayout
    {
        public RSTabbedViews()
        {
            Views = new List<VisualElement>();
            this.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        //Pages
        public static readonly BindableProperty ViewsProperty = BindableProperty.Create("Views", typeof(List<VisualElement>), typeof(RSTabbedViews), null);
        public List<VisualElement> Views
        {
            get { return (List<VisualElement>)GetValue(ViewsProperty); }
            set { SetValue(ViewsProperty, value); }
        }

        //Current Page
        public static readonly BindableProperty CurrentViewProperty = BindableProperty.Create("CurrentView", typeof(VisualElement), typeof(RSTabbedViews), null);
        public VisualElement CurrentView
        {
            get { return (VisualElement)GetValue(CurrentViewProperty); }
            set { SetValue(CurrentViewProperty, value); }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.CreateAttached("Title", typeof(string), typeof(RSTabbedViews), string.Empty);

        public static string GetTitle(BindableObject view)
        {
            return (string)view.GetValue(TitleProperty);
        }

        public static void SetTitle(BindableObject view, string value)
        {
            view.SetValue(TitleProperty, value);
        }
    }
}
