using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.Interfaces;

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


            IList<object> tempItemsSource = new List<object>();
            foreach (var item in this.ItemsSource)
            {
                if (!string.IsNullOrEmpty(DisplayMemberPath))
                {
                    ItemDisplayBinding = new Binding(DisplayMemberPath);
                }

                tempItemsSource.Add(item);
            }

            if (this.OrderBy == OrderByEnum.Default)
            {
                foreach (var item in tempItemsSource)
                {
                    if (!string.IsNullOrEmpty(DisplayMemberPath))
                        ItemDisplayBinding = new Binding(DisplayMemberPath);

                    itemsSource.Add(item);
                }
            }
            else
            {
                if (this.OrderBy == OrderByEnum.Ascending)
                {
                    if (!string.IsNullOrEmpty(DisplayMemberPath))
                    {
                        foreach (var item in tempItemsSource.OrderBy(p => p.GetType().GetProperty(DisplayMemberPath).GetValue(p, null)))
                        {
                            ItemDisplayBinding = new Binding(DisplayMemberPath);
                            itemsSource.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in tempItemsSource.OrderBy(p => p.ToString()))
                        {
                            itemsSource.Add(item);
                        }
                    }
                }

                else if (this.OrderBy == OrderByEnum.Descending)
                {
                    if (!string.IsNullOrEmpty(DisplayMemberPath))
                    {
                        foreach (var item in tempItemsSource.OrderByDescending(p => p.GetType().GetProperty(DisplayMemberPath).GetValue(p, null)))
                        {
                            ItemDisplayBinding = new Binding(DisplayMemberPath);
                            itemsSource.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in tempItemsSource.OrderByDescending(p => p.ToString()))
                        {
                            itemsSource.Add(item);
                        }
                    }
                }
            }


            if (this.ItemsSource is INotifyCollectionChanged)
                base.ItemsSource = itemsSource as ObservableCollection<object>;
            else
                base.ItemsSource = itemsSource as List<object>;
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

            if(this.OrderBy == OrderByEnum.Ascending)
                foreach (T value in items.OrderBy(i => i.ToString()))
                    itemsSource.Add(value);
            else if(this.OrderBy == OrderByEnum.Descending)
                foreach (T value in items.OrderByDescending(i => i.ToString()))
                    itemsSource.Add(value);
            else
                foreach (T value in items)
                    itemsSource.Add(value);

            ItemsSource = itemsSource;
        }
    }

    public class RSPickerBase : Picker, IHaveError, IRSControl
    {
        public RSPickerBase()
        {
            this.TextColor = Color.Black;
        }

        public static readonly BindableProperty IsPlaceholderAlwaysFloatingProperty = BindableProperty.Create("IsPlaceholderAlwaysFloating", typeof(bool), typeof(RSPickerBase), false);
        public bool IsPlaceholderAlwaysFloating
        {
            get { return (bool)GetValue(IsPlaceholderAlwaysFloatingProperty); }
            set { SetValue(IsPlaceholderAlwaysFloatingProperty, value); }
        }

        //Placeholder
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(RSPickerBase), null);
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

        public static readonly BindableProperty UpdateSourceTriggerProperty = BindableProperty.Create("UpdateSourceTrigger", typeof(UpdateSourceTriggerEnum), typeof(RSPickerBase), null);
        public UpdateSourceTriggerEnum UpdateSourceTrigger
        {
            get { return (UpdateSourceTriggerEnum)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly BindableProperty RSEntryStyleProperty = BindableProperty.Create("RSEntryStyle", typeof(RSEntryStyleSelectionEnum), typeof(RSPickerBase), RSEntryStyleSelectionEnum.OutlinedBorder);
        public RSEntryStyleSelectionEnum RSEntryStyle
        {
            get { return (RSEntryStyleSelectionEnum)GetValue(RSEntryStyleProperty); }
            set { SetValue(RSEntryStyleProperty, value); }
        }

        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSPickerBase), null);
        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public bool HasError
        {
            get
            {
                if (this.Behaviors.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSPickerBase), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        public static readonly BindableProperty HelperProperty = BindableProperty.Create("Helper", typeof(string), typeof(RSPickerBase), string.Empty);
        public string Helper
        {
            get { return (string)GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }

        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter", typeof(int), typeof(RSPickerBase), 0);
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public static readonly BindableProperty CounterMaxLengthProperty = BindableProperty.Create("CounterMaxLength", typeof(int), typeof(RSPickerBase), -1);
        public int CounterMaxLength
        {
            get { return (int)GetValue(CounterMaxLengthProperty); }
            set { SetValue(CounterMaxLengthProperty, value); }
        }

        //Icon
        public static readonly BindableProperty LeadingIconProperty = BindableProperty.Create("LeadingIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon LeadingIcon
        {
            get { return (RSEntryIcon)GetValue(LeadingIconProperty); }
            set { SetValue(LeadingIconProperty, value); }
        }

        public static readonly BindableProperty TrailingIconProperty = BindableProperty.Create("TrailingIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon TrailingIcon
        {
            get { return (RSEntryIcon)GetValue(TrailingIconProperty); }
            set { SetValue(TrailingIconProperty, value); }
        }

        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon LeftIcon
        {
            get { return (RSEntryIcon)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon RightIcon
        {
            get { return (RSEntryIcon)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }

        public static readonly BindableProperty LeftHelpingIconProperty = BindableProperty.Create("LeftHelpingIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon LeftHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(LeftHelpingIconProperty); }
            set { SetValue(LeftHelpingIconProperty, value); }
        }

        public static readonly BindableProperty RightHelpingIconProperty = BindableProperty.Create("RightHelpingIcon", typeof(RSEntryIcon), typeof(RSPickerBase), null);
        public RSEntryIcon RightHelpingIcon
        {
            get { return (RSEntryIcon)GetValue(RightHelpingIconProperty); }
            set { SetValue(RightHelpingIconProperty, value); }
        }

       
        public static readonly BindableProperty BorderRadiusProperty = BindableProperty.Create("BorderRadius", typeof(float), typeof(RSPickerBase), 16f);
        public float BorderRadius
        {
            get { return (float)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty = BindableProperty.Create("Padding", typeof(Thickness), typeof(RSPickerBase), null);
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        //Border Color
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create("BorderColor", typeof(Color), typeof(RSPickerBase), Color.DimGray);
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        //Border Fill Color
        public static readonly BindableProperty BorderFillColorProperty = BindableProperty.Create("BorderFillColor", typeof(Color), typeof(RSPickerBase), Color.FromHex("#OA000000"));
        public Color BorderFillColor
        {
            get { return (Color)GetValue(BorderFillColorProperty); }
            set { SetValue(BorderFillColorProperty, value); }
        }
        //Active Color
        public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create("ActiveColor", typeof(Color), typeof(RSPickerBase), Color.FromHex("#3F51B5"));
        public Color ActiveColor
        {
            get { return (Color)GetValue(ActiveColorProperty); }
            set { SetValue(ActiveColorProperty, value); }
        }
        //Error Color
        public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create("ErrorColor", typeof(Color), typeof(RSPickerBase), Color.FromHex("#f44336"));
        public Color ErrorColor
        {
            get { return (Color)GetValue(ErrorColorProperty); }
            set { SetValue(ErrorColorProperty, value); }
        }

        public static readonly BindableProperty LeftIconsProperty = BindableProperty.Create("LeftIcons", typeof(IList<RSEntryIcon>), typeof(RSPickerBase), new List<RSEntryIcon>());
        public IList<RSEntryIcon> LeftIcons
        {
            get { return (IList<RSEntryIcon>)GetValue(LeftIconsProperty); }
            set { SetValue(LeftIconsProperty, value); }
        }

        public bool HasRighIconSeparator { get; set; }
        public bool HasLeftIconSeparator { get; set; }








        //Order
        public static readonly BindableProperty OrderByProperty = BindableProperty.Create("OrderBy", typeof(OrderByEnum), typeof(RSPickerBase), OrderByEnum.Default);
        public OrderByEnum OrderBy
        {
            get { return (OrderByEnum)GetValue(OrderByProperty); }
            set { SetValue(OrderByProperty, value); }
        }


        //Selection mode
        public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create("SelectionMode", typeof(PickerSelectionModeEnum), typeof(RSPickerBase), PickerSelectionModeEnum.Single);
        public PickerSelectionModeEnum SelectionMode
        {
            get { return (PickerSelectionModeEnum)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }


        //SelectedItems
        public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create("SelectedItems", typeof(IList), typeof(RSPickerBase), null, propertyChanged: OnSelectedItemsChanged);
        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }


        //Items Template
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(RSPickerBase), null);
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
        public bool IsPassword { get; set; } = false;

        private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var list = (RSPickerBase)bindable;

            list.OnSelectedItemsChanged();
        }

        internal void OnSelectedItemsChanged()
        {
            if (this.SelectedItems == null)
                return;


            if (this.SelectedItems is INotifyCollectionChanged observableDataSource)
            {
                observableDataSource.CollectionChanged -= ObservableDataSource_CollectionChanged;
                observableDataSource.CollectionChanged += ObservableDataSource_CollectionChanged;
            }
        }

        private void ObservableDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnPropertyChanged("SelectedItems");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnPropertyChanged("SelectedItems");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged("SelectedItems");
                    break;
                default:
                    break;
            }
        }
    }
}
