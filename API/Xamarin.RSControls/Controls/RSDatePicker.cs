﻿using System;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Xamarin.RSControls.Controls
{
    public class RSDatePicker : DatePicker, IHaveError
    {
        public RSDatePicker()
        {
            this.TextColor = Color.Black;
            this.hasCustomFormat = false;
        }

        //Icon
        public static readonly BindableProperty LeftIconProperty = BindableProperty.Create("LeftIcon", typeof(string), typeof(RSDatePicker), null);
        public string LeftIcon
        {
            get { return (string)GetValue(LeftIconProperty); }
            set { SetValue(LeftIconProperty, value); }
        }

        public static readonly BindableProperty RightIconProperty = BindableProperty.Create("RightIcon", typeof(string), typeof(RSDatePicker), null);
        public string RightIcon
        {
            get { return (string)GetValue(RightIconProperty); }
            set { SetValue(RightIconProperty, value); }
        }


        //Has Border
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create("HasBorder", typeof(bool), typeof(RSDatePicker), false);
        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        //Has Custom Format
        private bool hasCustomFormat { get; set; }

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

        //Error
        public static readonly BindableProperty ErrorProperty = BindableProperty.Create("Error", typeof(string), typeof(RSDatePicker), null);
        public string Error
        {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            if( propertyName == FormatProperty.PropertyName)
            {
                hasCustomFormat = true;
            }
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

        public bool HasCustomFormat()
        {
            return hasCustomFormat;
        }

        //Resize control when width or height set to auto and text changes
        public void DoInvalidate()
        {
            this.InvalidateMeasure();
        }
    }
}

