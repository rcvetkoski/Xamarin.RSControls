using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.RSControls.Enums;

namespace Xamarin.RSControls.Controls
{
    public class RSPicker : RSPickerBase, IDisposable
    {
        IList<object> itemsSource;

        public new static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(RSPicker), null, propertyChanged: OnItemsChanged);
        public new IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
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
                OnItemsChanged();
            }
        }

        private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var list = (RSPicker)bindable;

            list.OnItemsChanged();
        }

        internal void OnItemsChanged()
        {
            if (this.ItemsSource == null)
                return;


            if (this.ItemsSource is INotifyCollectionChanged observableDataSource)
            {
                itemsSource = new ObservableCollection<object>();

                observableDataSource.CollectionChanged -= ObservableDataSource_CollectionChanged;
                observableDataSource.CollectionChanged += ObservableDataSource_CollectionChanged;
            }
            else
                itemsSource = new List<object>();



            foreach (var item in this.ItemsSource)
            {
                if (!string.IsNullOrEmpty(DisplayMemberPath))
                {
                    ItemDisplayBinding = new Binding(DisplayMemberPath);
                }

                itemsSource.Add(item);
            }


            IOrderedEnumerable<object> orderedDataSource = null;
            if (this.OrderBy == OrderByEnum.Ascending)
                orderedDataSource = itemsSource.OrderBy(p => p.GetType().GetProperty(DisplayMemberPath).GetValue(p, null));
            else if(this.OrderBy == OrderByEnum.Descending)
                orderedDataSource = itemsSource.OrderByDescending(p => p.GetType().GetProperty(DisplayMemberPath).GetValue(p, null));


            if(this.OrderBy == OrderByEnum.Default)
                base.ItemsSource = itemsSource.ToList();
            else
                base.ItemsSource = orderedDataSource.ToList();
        }

        private void ObservableDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                        itemsSource.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    itemsSource.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    itemsSource.Clear();
                    break;
                default:
                    break;
            }
        }


        public void Dispose()
        {
            if (this.ItemsSource is ObservableCollection<object> observableDataSource)
                observableDataSource.CollectionChanged -= ObservableDataSource_CollectionChanged;
        }
    }




    public class RSEnumPicker<T> : RSPickerBase where T : struct
    {
        public RSEnumPicker()
        {
            OnItemsChanged();
        }

        private void OnItemsChanged()
        {
            List<T> itemsSource = new List<T>();
            List<T> items = new List<T>();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                items.Add(value);
            }

            foreach (T value in items.OrderBy(i => i.ToString()))
            {
                itemsSource.Add(value);
            }

            ItemsSource = itemsSource;
        }
    }


    

    public class RSPickerBase : Picker
    {
        public RSPickerBase()
        {
            this.TextColor = Color.Black;
        }

        //Icon
        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(string), typeof(RSPickerBase), null);
        public string LeftIcon
        {
            get { return (string)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(string), typeof(RSPickerBase), null);
        public string RightIcon
        {
            get { return (string)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        //Icon Color
        public static readonly BindableProperty IconColorProperty = BindableProperty.Create("IconColor", typeof(Color), typeof(RSPickerBase), Color.DimGray);
        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        //Icon Width
        public static readonly BindableProperty IconWidthProperty = BindableProperty.Create("IconWidth", typeof(double), typeof(RSPickerBase), 30.0);
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        //Icon Height
        public static readonly BindableProperty IconHeightProperty = BindableProperty.Create("IconHeight", typeof(double), typeof(RSPickerBase), 30.0);
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        //Has Border
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSPickerBase), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        //Placeholder
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(RSPickerBase), "");
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        //Placeholder color
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(RSPickerBase), Color.Gray);
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        //Order
        public static readonly BindableProperty OrderByProperty = BindableProperty.Create("OrderBy", typeof(OrderByEnum), typeof(RSPickerBase), OrderByEnum.Default);
        public OrderByEnum OrderBy
        {
            get { return (OrderByEnum)GetValue(OrderByProperty); }
            set { SetValue(OrderByProperty, value); }
        }

        //Items Template
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(RSPickerBase), null);
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
    }
}
