using System;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;

namespace Xamarin.RSControls.Controls
{
    public class RSDatePicker : DatePicker
    {
        public RSDatePicker()
        {
            this.TextColor = Color.Black;
            this.Format = "dd/MM/yyyy";
        }

        //Icon
        public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(string), typeof(RSDatePicker), null);
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        //Icon Color
        public static readonly BindableProperty IconColorProperty = BindableProperty.Create("IconColor", typeof(Color), typeof(RSDatePicker), Color.Transparent);
        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        //Has Border
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSDatePicker), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        //Icon Width
        public static readonly BindableProperty IconWidthProperty = BindableProperty.Create("IconWidth", typeof(double), typeof(RSDatePicker), 15.0);
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        //Icon Height
        public static readonly BindableProperty IconHeightProperty = BindableProperty.Create("IconHeight", typeof(double), typeof(RSDatePicker), 15.0);
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        //Placeholder
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create("Placeholder", typeof(string), typeof(RSDatePicker), null);
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        //Placeholder color
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(RSDatePicker), Color.Gray);
        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        //Is Clearable
        public static readonly BindableProperty IsClearableProperty = BindableProperty.Create("IsClearable", typeof(bool), typeof(RSDatePicker), true);
        public bool IsClearable
        {
            get { return (bool)GetValue(IsClearableProperty); }
            set { SetValue(IsClearableProperty, value); }
        }

        //Is NullableDate
        public static readonly BindableProperty NullableDateProperty = BindableProperty.Create(nameof(NullableDate), typeof(DateTime?), typeof(RSDatePicker), null, defaultBindingMode: BindingMode.TwoWay);
        public DateTime? NullableDate
        {
            get { return (DateTime?)GetValue(NullableDateProperty); }
            set { SetValue(NullableDateProperty, value); }
        }

        //Date Selection mode
        public static readonly BindableProperty DateSelectionModeProperty = BindableProperty.Create("DateSelectionMode", typeof(DateSelectionModeEnum), typeof(RSDatePicker), DateSelectionModeEnum.Default);
        public DateSelectionModeEnum DateSelectionMode
        {
            get { return (DateSelectionModeEnum)GetValue(DateSelectionModeProperty); }
            set { SetValue(DateSelectionModeProperty, value); }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            //Update or set NullableDate when Date binded instead of NullableDate
            //if (propertyName == DateProperty.PropertyName)
            //{
            //    AssignValue();
            //}

            //if (propertyName == NullableDateProperty.PropertyName && NullableDate.HasValue)
            //{
            //    Date = NullableDate.Value;
            //    if (Date.ToString(this.Format) == DateTime.Now.ToString(this.Format))
            //    {
            //        //this code was done because when date selected is the actual date the"DateProperty" does not raise  
            //        UpdateDate();
            //    }
            //}
        }

        public void CleanDate()
        {
            NullableDate = null;
        }
        public void AssignValue()
        {
            NullableDate = Date;
        }

        //Resize control when width or height set to auto and text changes
        public void DoInvalidate()
        {
            this.InvalidateMeasure();
        }
    }
}

