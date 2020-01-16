using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSDatePicker), typeof(RSDatePickerRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSDatePickerRenderer : DatePickerRenderer
    {
        private UIDatePicker uIDatePicker;
        private UIPickerView uIPickerViewMonthsYears;
        private RSDatePicker elementCasted;

        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;

            elementCasted = this.Element as RSDatePicker;


            SetPlaceHolderText();


            if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Default)
            {
                CreateDatePicker();
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                CreateMonthYearPicker();
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                CreateYearPicker();
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                CreateMonthPicker();
            }

            this.Control.InputAccessoryView = CreateToolbar();
            this.Control.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.Control.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.Control.InputAssistantItem.LeadingBarButtonGroups = null;
            this.Control.InputAssistantItem.TrailingBarButtonGroups = null;
            this.Control.AccessibilityTraits = UIAccessibilityTrait.Button;

            if(!this.elementCasted.HasCustomFormat())
                SetDateFormat();

            //Set correct value for nulabledate if greater than max date or smaller than min date
            if (this.elementCasted.NullableDate.HasValue && !IsCorrectMinMaxDateSelectedValue(this.elementCasted.NullableDate.Value))
                this.elementCasted.NullableDate = CorrectMinMaxDateSelectedValue(this.elementCasted.NullableDate.Value);
            else
            {
                //if NullableDate HasValue this 2 lines will be executed in OnElementPropertyChanged
                SetPickerSelectedIndicator(this.elementCasted.NullableDate);
                SetText();
            }

            SetNativeControl(this.Control);

            //Delete placeholder as we use floating hint instead
            elementCasted.Placeholder = "";
    }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //Update picker text if binding value changes
            if (e.PropertyName == "NullableDate")
            {
                //Reset value if not in allowed range
                if(this.elementCasted.NullableDate.HasValue)
                {
                    if (!IsCorrectMinMaxDateSelectedValue(this.elementCasted.NullableDate.Value))
                        this.elementCasted.NullableDate = CorrectMinMaxDateSelectedValue(this.elementCasted.NullableDate.Value);

                    SetPickerSelectedIndicator(this.elementCasted.NullableDate);
                }
                else
                    SetPickerSelectedIndicator(CorrectMinMaxDateSelectedValue(DateTime.Now));

                SetText();

                if(this.Control is RSUITextField)
                    (this.Control as RSUITextField).UpdateFloatingLabel();
            }
        }

        private UIToolbar CreateToolbar()
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            UIToolbar toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

            //Clear cancel button
            UIBarButtonItem clearCancelButton;
            if (this.elementCasted.IsClearable)
            {
                clearCancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (sender, ev) =>
                {
                    this.elementCasted.CleanDate();
                    //this.SetPickerSelectedIndicator();
                    SetText();
                    this.Control.ResignFirstResponder();
                });

            }
            else
            {
                clearCancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Plain, (sender, ev) =>
                {
                    this.Control.ResignFirstResponder();
                });
            }

            //Space
            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

            //Done Button
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, ev) =>
            {
                if (!this.elementCasted.NullableDate.HasValue)
                    SetDate(CorrectMinMaxDateSelectedValue(DateTime.Now));

                SetText();
                this.Control.ResignFirstResponder();
            });

            toolbar.SetItems(new[] { clearCancelButton, spacer, doneButton }, false);

            return toolbar;
        }

        private void CreateDatePicker()
        {
            uIDatePicker = new UIDatePicker();
            uIDatePicker.Mode = UIDatePickerMode.Date;
            uIDatePicker.Date = this.elementCasted.Date.ToNSDate();
            uIDatePicker.MinimumDate = this.elementCasted.MinimumDate.ToNSDate();
            uIDatePicker.MaximumDate = this.elementCasted.MaximumDate.ToNSDate();
            uIDatePicker.ValueChanged += UIDatePicker_ValueChanged;

            this.Control.InputView = uIDatePicker;
        }

        private void UIDatePicker_ValueChanged(object sender, EventArgs e)
        {
            SetDate(uIDatePicker.Date.ToDateTime());
            SetText();
        }

        private void CreateMonthYearPicker()
        {
            //Years
            var years = Enumerable.Range(this.elementCasted.MinimumDate.Year, this.elementCasted.MaximumDate.Year - this.elementCasted.MinimumDate.Year + 1).ToArray();
            //Months
            string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Where(x => x.ToString() != string.Empty).ToArray();

            uIPickerViewMonthsYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, months, this.elementCasted, this),
                BackgroundColor = UIColor.Clear
            };

            this.Control.InputView = uIPickerViewMonthsYears;
        }

        private void CreateYearPicker()
        {
            var years = Enumerable.Range(this.elementCasted.MinimumDate.Year, this.elementCasted.MaximumDate.Year - this.elementCasted.MinimumDate.Year + 1).ToArray();
            uIPickerViewMonthsYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, new string[0], this.elementCasted, this),
                BackgroundColor = UIColor.Clear
            };

            this.Control.InputView = uIPickerViewMonthsYears;
        }

        private void CreateMonthPicker()
        {
            string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Where(x=> x.ToString() != string.Empty).ToArray();
            uIPickerViewMonthsYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(new int[0], months, this.elementCasted, this),
                BackgroundColor = UIColor.Clear
            };

            this.Control.InputView = uIPickerViewMonthsYears;
        }

        private void CreateUISearchBar()
        {
            ////SearchBar
            //UISearchBar uISearchBar;
            //if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.LandscapeLeft && UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.LandscapeRight)
            //    uISearchBar = new UISearchBar(new CGRect(x: 0, y: 0, width: toolbar.Frame.Width - 150, height: 30));
            //else
            //    uISearchBar = new UISearchBar(new CGRect(x: 0, y: 0, width: toolbar.Frame.Width - 250, height: 30));

            //uISearchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            //uISearchBar.ShowsCancelButton = false;
            //uISearchBar.Translucent = false;
            //uISearchBar.SearchBarStyle = UISearchBarStyle.Minimal;
            //uISearchBar.Placeholder = "Search";
            //UIBarButtonItem textfieldBarButton = new UIBarButtonItem(customView: uISearchBar);
        }

        private void SetPlaceHolderText()
        {
            if(this.elementCasted.Placeholder != null)
                this.Control.AttributedPlaceholder = new NSAttributedString(this.elementCasted.Placeholder, null, this.elementCasted.PlaceholderColor.ToUIColor());
        }

        public void SetText()
        {
            if (!this.elementCasted.NullableDate.HasValue)
            {
                this.Control.Text = "";
            }
            else
            {
                this.Control.Text = this.elementCasted.NullableDate.Value.ToString(this.elementCasted.Format);
            }
        }

        public void SetDate(DateTime date)
        {
            this.elementCasted.NullableDate = date;
        }

        private void SetDateFormat()
        {
            if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                this.elementCasted.Format = "MMMM yyyy";
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                this.elementCasted.Format = "yyyy";

            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                this.elementCasted.Format = "MMMM";
            }
        }

        public void SetPickerSelectedIndicator(DateTime? dateTime = null)
        {
            if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                if(dateTime != null)
                {
                    uIPickerViewMonthsYears.Select(dateTime.Value.Month - 1, 0, true);
                    uIPickerViewMonthsYears.Select(Array.IndexOf((uIPickerViewMonthsYears.Model as CustomUIDatePickerViewModel).Years, dateTime.Value.Year), 1, true);
                }
                else
                {
                    uIPickerViewMonthsYears.Select(DateTime.Now.Month - 1, 0, true);
                    uIPickerViewMonthsYears.Select(DateTime.Now.Year, 1, true);
                }
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                if (dateTime != null)
                {
                    uIPickerViewMonthsYears.Select(Array.IndexOf((uIPickerViewMonthsYears.Model as CustomUIDatePickerViewModel).Years, dateTime.Value.Year), 0, true);
                }
                else
                {
                    uIPickerViewMonthsYears.Select(Array.IndexOf((uIPickerViewMonthsYears.Model as CustomUIDatePickerViewModel).Years, DateTime.Now.Year), 0, true);
                }
            }
            else if (this.elementCasted.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                if (dateTime != null)
                {
                    uIPickerViewMonthsYears.Select(dateTime.Value.Month - 1, 0, true);
                }
                else
                {
                    uIPickerViewMonthsYears.Select(DateTime.Now.Month - 1, 0, true);
                }
            }
            else
            {
                if (dateTime != null)
                {
                    uIDatePicker.Date = dateTime.Value.ToNSDate();
                }
                else
                {
                    uIDatePicker.Date = DateTime.Now.ToNSDate();
                }
            }
        }

        public DateTime CorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (this.elementCasted.MinimumDate <= date && this.elementCasted.MaximumDate >= date)
                return date;
            else
            {
                if (this.elementCasted.MinimumDate > date)
                    return this.elementCasted.MinimumDate;
                else if (this.elementCasted.MaximumDate < date)
                    return this.elementCasted.MaximumDate;
                else
                    return DateTime.Now;
            }
        }

        public bool IsCorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (this.elementCasted.MinimumDate <= date && this.elementCasted.MaximumDate >= date)
                return true;
            else
                return false;
        }

        protected override UITextField CreateNativeControl()
        {
            return new RSUITextField(this.Element as IRSControl);
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                if(uIDatePicker != null)
                    uIDatePicker.ValueChanged -= UIDatePicker_ValueChanged;
            }

            base.Dispose(disposing);
        }

    }

    public class CustomUIDatePickerViewModel : UIPickerViewModel
    {
        public int[] Years;
        public string[] Months;
        protected int selectedIndex = 0;
        private RSDatePicker rsDatePicker;
        private RSDatePickerRenderer renderer;
        private DateTime dateTime;

        public CustomUIDatePickerViewModel(int[] years, string[] months, RSDatePicker rsDatePicker, RSDatePickerRenderer renderer)
        {
            this.rsDatePicker = rsDatePicker;
            this.renderer = renderer;

            this.Years = years;
            this.Months = months;
        }

        public object SelectedItem { get; set; }


        public override nint GetComponentCount(UIPickerView picker)
        {
            if(this.rsDatePicker.DateSelectionMode == DateSelectionModeEnum.MonthYear)
                return 2;
            else
                return 1;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            if (component == 0)
            {
                if (Months.Any())
                    return Months.Length;
                else
                    return Years.Length;
            }
            else
                return Years.Length;
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            if(component == 0)
            {
                if(this.Months.Any())
                    return this.Months[(int)row].ToString();
                else
                    return this.Years[(int)row].ToString();
            }
            else
                return this.Years[(int)row].ToString();
        }

        public override NSAttributedString GetAttributedTitle(UIPickerView pickerView, nint row, nint component)
        {
            var selectedRow = pickerView.SelectedRowInComponent(component);

            if (rsDatePicker.NullableDate.HasValue)
                dateTime = rsDatePicker.NullableDate.Value;
            else
                dateTime = DateTime.Now;

            if (component == 0)
            {
                dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);

                if (!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
                {
                    return new NSAttributedString(this.Months[(int)row].ToString(), null, UIColor.Gray);
                }
                else
                    return new NSAttributedString(this.Months[(int)row].ToString(), null, UIColor.Black);
            }
            else
            {
                dateTime = new DateTime((int)Years[(int)row], dateTime.Month, dateTime.Day);

                if (!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
                {
                    return new NSAttributedString(this.Years[(int)row].ToString(), null, UIColor.Gray);
                }
                else
                    return new NSAttributedString(this.Years[(int)row].ToString(), null, UIColor.Black);

            }
        }


        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            var selectedDate = dateTime;

            selectedIndex = (int)row;

            if (rsDatePicker.NullableDate.HasValue)
                dateTime = rsDatePicker.NullableDate.Value;
            else
                dateTime = DateTime.Now;


            if (this.GetComponentCount(picker) > 1)
            {
                if(component == 0)
                    dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);
                else if (component == 1)
                    dateTime = new DateTime((int)Years[(int)row], dateTime.Month, dateTime.Day);
            }
            else 
            {
                if (this.Months.Any())
                    dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);
                else 
                    dateTime = new DateTime((int)Years[(int)row], dateTime.Month, dateTime.Day);
            }


            if (!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
            {
                dateTime = this.renderer.CorrectMinMaxDateSelectedValue(dateTime);

                if (this.rsDatePicker.NullableDate.HasValue && selectedDate != this.rsDatePicker.NullableDate)
                    this.renderer.SetPickerSelectedIndicator(dateTime);

                this.renderer.SetDate(this.renderer.CorrectMinMaxDateSelectedValue(dateTime));
            }
            else
                this.renderer.SetDate(dateTime);

            this.renderer.SetText();
        }
    }
}