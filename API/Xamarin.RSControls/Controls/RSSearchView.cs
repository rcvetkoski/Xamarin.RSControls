using System.Collections;
using Xamarin.Forms;

namespace Xamarin.RSControls.Controls
{
    public class RSSearchView : SearchBar
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(RSSearchView), null);
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem", typeof(object), typeof(RSSearchView), null, BindingMode.TwoWay);
        public object SelectedItem
        {
            get => (object)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private string displayMemberPath;
        public string DisplayMemberPath
        {
            get
            {
                return displayMemberPath;
            }
            set
            {
                this.displayMemberPath = value;
            }
        }

        //Has Border
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSSearchView), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }
    }
}
