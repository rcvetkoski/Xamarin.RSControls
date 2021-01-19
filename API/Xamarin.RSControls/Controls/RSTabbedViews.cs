using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;

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

        //Bar placement
        public static readonly BindableProperty RSTabPlacementProperty = BindableProperty.CreateAttached("RSTabPlacement", typeof(RSTabPlacementEnum), typeof(RSTabbedViews), RSTabPlacementEnum.Top);
        public RSTabPlacementEnum RSTabPlacement
        {
            get { return (RSTabPlacementEnum)GetValue(RSTabPlacementProperty); }
            set { SetValue(RSTabPlacementProperty, value); }
        }

        //Bar Text Color
        public static readonly BindableProperty BarTextColorProperty = BindableProperty.Create("BarTextColor", typeof(Color), typeof(RSTabbedViews), Color.SlateGray);
        public Color BarTextColor
        {
            get { return (Color)GetValue(BarTextColorProperty); }
            set { SetValue(BarTextColorProperty, value); }
        }

        //Bar Text Color Selected
        public static readonly BindableProperty BarTextColorSelectedProperty = BindableProperty.Create("BarTextColorSelected", typeof(Color), typeof(RSTabbedViews), Color.Black);
        public Color BarTextColorSelected
        {
            get { return (Color)GetValue(BarTextColorSelectedProperty); }
            set { SetValue(BarTextColorSelectedProperty, value); }
        }

        //Menu Color
        public static readonly BindableProperty BarColorProperty = BindableProperty.Create("BarColor", typeof(Color), typeof(RSTabbedViews), Color.Transparent);
        public Color BarColor
        {
            get { return (Color)GetValue(BarColorProperty); }
            set { SetValue(BarColorProperty, value); }
        }

        //Slider Color
        public static readonly BindableProperty SliderColorProperty = BindableProperty.Create("SliderColor", typeof(Color), typeof(RSTabbedViews), Color.Gray);
        public Color SliderColor
        {
            get { return (Color)GetValue(SliderColorProperty); }
            set { SetValue(SliderColorProperty, value); }
        }

        //Title
        public static readonly BindableProperty TitleProperty = BindableProperty.CreateAttached("Title", typeof(string), typeof(RSTabbedViews), string.Empty);
        public static string GetTitle(BindableObject view)
        {
            return (string)view.GetValue(TitleProperty);
        }

        public static void SetTitle(BindableObject view, string value)
        {
            view.SetValue(TitleProperty, value);
        }

        //Icon
        public static readonly BindableProperty IconProperty = BindableProperty.CreateAttached("Icon", typeof(string), typeof(RSTabbedViews), string.Empty);
        public static string GetIcon(BindableObject view)
        {
            return (string)view.GetValue(IconProperty);
        }

        public static void SetIcon(BindableObject view, string value)
        {
            view.SetValue(IconProperty, value);
        }
    }
}
