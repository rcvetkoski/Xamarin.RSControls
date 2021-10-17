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
        private RSDatePicker element;

        protected override UITextField CreateNativeControl()
        {
            if(Element != null)
            {
                element = Element as RSDatePicker;

                //Set default icon
                if ((this.Element as IRSControl).RightIcon == null)
                {
                    (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon()
                    {
                        View = new RSSvgImage() { Source = "Samples/Data/SVG/calendar.svg" },
                        Source = this.Element
                    };
                }

                //Set placeholder text
                SetPlaceHolderText();
            }

            return new RSUITextField(this.Element as IRSControl);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;

            //element = this.Element as RSDatePicker;

            if (this.element.DateSelectionMode == DateSelectionModeEnum.Default)
            {
                CreateDatePicker();
            }
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                CreateMonthYearPicker();
            }
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                CreateYearPicker();
            }
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                CreateMonthPicker();
            }

            this.Control.InputAccessoryView = CreateToolbar();
            this.Control.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.Control.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            this.Control.InputAssistantItem.LeadingBarButtonGroups = null;
            this.Control.InputAssistantItem.TrailingBarButtonGroups = null;
            this.Control.AccessibilityTraits = UIAccessibilityTrait.Button;
            Control.SpellCheckingType = UITextSpellCheckingType.No;
            Control.AutocorrectionType = UITextAutocorrectionType.No;
            Control.TintColor = UIColor.Clear;



            if (!this.element.HasCustomFormat())
                SetDateFormat();

            //Set correct value for nullabledate if greater than max date or smaller than min date
            if (this.element.NullableDate.HasValue && !IsCorrectMinMaxDateSelectedValue(this.element.NullableDate.Value))
                this.element.NullableDate = CorrectMinMaxDateSelectedValue(this.element.NullableDate.Value);
            else
            {
                //if NullableDate HasValue this 2 lines will be executed in OnElementPropertyChanged
                SetPickerSelectedIndicator(this.element.NullableDate);
                SetText();
            }


            //Delete placeholder as we use floating hint instead
            (Element as RSDatePicker).Placeholder = "";

            //Set horizontal text alignement
            if ((element as IRSControl).HorizontalTextAlignment == Forms.TextAlignment.Center)
                Control.TextAlignment = UITextAlignment.Center;
            else if ((element as IRSControl).HorizontalTextAlignment == Forms.TextAlignment.End)
                Control.TextAlignment = UITextAlignment.Right;


            this.Control.EditingDidBegin += Control_EditingDidBegin;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //Update picker text if binding value changes
            if (e.PropertyName == "NullableDate")
            {
                //Update only if changed by external factor
                if(!(sender as Forms.View).IsFocused)
                {
                    //Reset value if not in allowed range
                    if (this.element.NullableDate.HasValue)
                    {
                        if (!IsCorrectMinMaxDateSelectedValue(this.element.NullableDate.Value))
                            this.element.NullableDate = CorrectMinMaxDateSelectedValue(this.element.NullableDate.Value);

                        SetPickerSelectedIndicator(this.element.NullableDate);
                    }
                    else
                        SetPickerSelectedIndicator(CorrectMinMaxDateSelectedValue(DateTime.Now));

                    SetText();

                    (this.Control as RSUITextField).UpdateView();
                }
            }

            if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = (this.Element as RSDatePicker).Error;
            }
        }

        private void Control_EditingDidBegin(object sender, EventArgs e)
        {
            //var rSPopup = new RSPopupRenderer();
            //rSPopup.Title = "";
            //rSPopup.Message = "";
            //rSPopup.Width = -2;
            //rSPopup.Height = -2;
            //rSPopup.TopMargin = 0;
            //rSPopup.BottomMargin = 0;
            //rSPopup.LeftMargin = 20;
            //rSPopup.RightMargin = 20;
            //rSPopup.BorderRadius = 16;
            //rSPopup.BorderFillColor = UIColor.White.ToColor();
            //rSPopup.DimAmount = 0.8f;
            //rSPopup.AddAction("Done", RSPopupButtonTypeEnum.Positive, new Command(() => { if (this.Control != null) { this.Control.ResignFirstResponder(); } }));
            //rSPopup.AddAction("Clear", RSPopupButtonTypeEnum.Destructive, new Command(() => { }));
            //rSPopup.SetNativeView(uIPickerViewMonthsYears);
            //rSPopup.ShowPopup();
        }

        private UIToolbar CreateToolbar()
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            UIToolbar toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

            //Clear cancel button
            UIBarButtonItem clearCancelButton;
            if (this.element.IsClearable)
            {
                clearCancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (sender, ev) =>
                {
                    this.element.CleanDate();
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
                if (!this.element.NullableDate.HasValue)
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
            uIDatePicker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
            uIDatePicker.Mode = UIDatePickerMode.Date;
            uIDatePicker.Date = this.element.Date.ToNSDate();
            uIDatePicker.MinimumDate = this.element.MinimumDate.ToNSDate();
            uIDatePicker.MaximumDate = this.element.MaximumDate.ToNSDate();
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
            var years = Enumerable.Range(this.element.MinimumDate.Year, this.element.MaximumDate.Year - this.element.MinimumDate.Year + 1).ToArray();
            //Months
            string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Where(x => x.ToString() != string.Empty).ToArray();

            uIPickerViewMonthsYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, months, this.element, this),
                BackgroundColor = UIColor.Clear
            };

            this.Control.InputView = uIPickerViewMonthsYears;
        }

        private void CreateYearPicker()
        {
            var years = Enumerable.Range(this.element.MinimumDate.Year, this.element.MaximumDate.Year - this.element.MinimumDate.Year + 1).ToArray();
            uIPickerViewMonthsYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, new string[0], this.element, this),
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
                Model = new CustomUIDatePickerViewModel(new int[0], months, this.element, this),
                BackgroundColor = UIColor.Clear
            };

            this.Control.InputView = uIPickerViewMonthsYears;
        }

        private void SetPlaceHolderText()
        {
            if (element.Placeholder == null)
            {
                if (element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
                {
                    element.Placeholder = "Month, Year";
                }
                else if (element.DateSelectionMode == DateSelectionModeEnum.Month)
                {
                    element.Placeholder = "Select Month";
                }
                else if (element.DateSelectionMode == DateSelectionModeEnum.Year)
                {
                    element.Placeholder = "Select Year";
                }
                else
                {
                    element.Placeholder = "Select Date";
                }
            }
        }

        public void SetText()
        {
            if (!this.element.NullableDate.HasValue)
            {
                this.Control.Text = "";
            }
            else
            {
                this.Control.Text = this.element.NullableDate.Value.ToString(this.element.Format);
            }
        }

        public void SetDate(DateTime date)
        {
            this.element.NullableDate = date;
        }

        private void SetDateFormat()
        {
            if (this.element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                this.element.Format = "MMMM yyyy";
            }
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                this.element.Format = "yyyy";

            }
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                this.element.Format = "MMMM";
            }
        }

        public void SetPickerSelectedIndicator(DateTime? dateTime = null)
        {
            if (this.element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
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
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Year)
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
            else if (this.element.DateSelectionMode == DateSelectionModeEnum.Month)
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
            if (this.element.MinimumDate <= date && this.element.MaximumDate >= date)
                return date;
            else
            {
                if (this.element.MinimumDate > date)
                    return this.element.MinimumDate;
                else if (this.element.MaximumDate < date)
                    return this.element.MaximumDate;
                else
                    return DateTime.Now;
            }
        }

        public bool IsCorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (this.element.MinimumDate <= date && this.element.MaximumDate >= date)
                return true;
            else
                return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                if(uIDatePicker != null)
                    uIDatePicker.ValueChanged -= UIDatePicker_ValueChanged;

                this.Control.EditingDidBegin -= Control_EditingDidBegin;
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

            if (component == 0 && (rsDatePicker.DateSelectionMode == DateSelectionModeEnum.Month || rsDatePicker.DateSelectionMode == DateSelectionModeEnum.MonthYear))
            {
                try
                {
                    rsDatePicker.NullableDate.ToString();
                    dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);

                }
                catch(Exception e)
                {

                }

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