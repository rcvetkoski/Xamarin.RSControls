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
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSDatePicker), typeof(RSDatePickerRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSDatePickerRenderer : ViewRenderer<RSDatePicker, UITextField>
    {
        private UIDatePicker uIDatePicker;
        private UIPickerView uIPickerViewYears;
        private UIPickerView uIPickerViewMonths;
        private UITextField entry;

        protected override void OnElementChanged(ElementChangedEventArgs<RSDatePicker> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            if (Control == null)
            {
                entry = CreateNativeControl();

                SetPlaceHolderText(entry);

                //Set icon
                SetIcon(entry);

                if (this.Element.DateSelectionMode == DateSelectionModeEnum.Default)
                {
                    CreateDatePicker(entry);
                }
                else if (this.Element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
                {
                    CreateMonthYearPicker(entry);
                }
                else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Year)
                {
                    CreateYearPicker(entry);
                }
                else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Month)
                {
                    CreateMonthPicker(entry);
                }

                entry.InputAccessoryView = CreateToolbar(entry);
                entry.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
                entry.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
                entry.InputAssistantItem.LeadingBarButtonGroups = null;
                entry.InputAssistantItem.TrailingBarButtonGroups = null;
                entry.AccessibilityTraits = UIAccessibilityTrait.Button;

                if(!this.Element.HasCustomFormat())
                    SetDateFormat();

                //Set correct value for nulabledate if greater than max date or smaller than min date
                if (this.Element.NullableDate.HasValue)
                    this.Element.NullableDate = CorrectMinMaxDateSelectedValue(this.Element.NullableDate.Value);
                else
                {
                    //if NullableDate HasValue this 2 lines will be executed in OnElementPropertyChanged
                    SetPickerSelectedIndicator(this.Element.NullableDate);
                    SetText(entry);
                }


                SetNativeControl(entry);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //Update picker text if binding value changes
            if (e.PropertyName == "NullableDate")
            {
                //Reset value if not in allowed range
                if(this.Element.NullableDate.HasValue)
                {
                    if (!IsCorrectMinMaxDateSelectedValue(this.Element.NullableDate.Value))
                        this.Element.NullableDate = CorrectMinMaxDateSelectedValue(this.Element.NullableDate.Value);

                    SetPickerSelectedIndicator(this.Element.NullableDate);
                }
                else
                    SetPickerSelectedIndicator(CorrectMinMaxDateSelectedValue(DateTime.Now));

                SetText(entry);
            }
        }

        private UIToolbar CreateToolbar(UITextField entry)
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            UIToolbar toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

            //Clear cancel button
            UIBarButtonItem clearCancelButton;
            if (this.Element.IsClearable)
            {
                clearCancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (sender, ev) =>
                {
                    this.Element.CleanDate();
                    this.SetPickerSelectedIndicator();
                    SetText(entry);
                    entry.ResignFirstResponder();
                });

            }
            else
            {
                clearCancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Plain, (sender, ev) =>
                {
                    entry.ResignFirstResponder();
                });
            }

            //Space
            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

            //Done Button
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, ev) =>
            {
                if (!this.Element.NullableDate.HasValue)
                    SetDate(CorrectMinMaxDateSelectedValue(DateTime.Now));

                SetText(entry);
                entry.ResignFirstResponder();
            });

            toolbar.SetItems(new[] { clearCancelButton, spacer, doneButton }, false);

            return toolbar;
        }

        private void CreateDatePicker(UITextField entry)
        {
            uIDatePicker = new UIDatePicker();
            uIDatePicker.Mode = UIDatePickerMode.Date;
            uIDatePicker.Date = this.Element.Date.ToNSDate();
            uIDatePicker.MinimumDate = this.Element.MinimumDate.ToNSDate();
            uIDatePicker.MaximumDate = this.Element.MaximumDate.ToNSDate();
            uIDatePicker.ValueChanged += UIDatePicker_ValueChanged;

            entry.InputView = uIDatePicker;
        }

        private void UIDatePicker_ValueChanged(object sender, EventArgs e)
        {
            SetDate(uIDatePicker.Date.ToDateTime());
            SetText(entry);
        }

        private void CreateMonthYearPicker(UITextField entry)
        {
            //Years
            var years = Enumerable.Range(this.Element.MinimumDate.Year, this.Element.MaximumDate.Year - this.Element.MinimumDate.Year + 1);
            var yearsList = years.ToList();
            uIPickerViewYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, this.Element, this, DateModeSelector.Year),
                BackgroundColor = UIColor.Clear
            };

            //Months
            string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Where(x => x.ToString() != string.Empty).ToArray();
            uIPickerViewMonths = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(months, this.Element, this, DateModeSelector.Month),
                BackgroundColor = UIColor.Clear
            };


            UIStackView uIStackView = new UIStackView();
            uIStackView.Axis = UILayoutConstraintAxis.Horizontal;
            uIStackView.Distribution = UIStackViewDistribution.FillEqually;
            uIStackView.AddArrangedSubview(uIPickerViewMonths);
            uIStackView.AddArrangedSubview(uIPickerViewYears);


            entry.InputView = uIStackView;
        }

        private void CreateYearPicker(UITextField entry)
        {
            var years = Enumerable.Range(this.Element.MinimumDate.Year, this.Element.MaximumDate.Year - this.Element.MinimumDate.Year + 1);
            var yearsList = years.ToList();
            uIPickerViewYears = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(years, this.Element, this, DateModeSelector.Year),
                BackgroundColor = UIColor.Clear
            };

            entry.InputView = uIPickerViewYears;
        }

        private void CreateMonthPicker(UITextField entry)
        {
            string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames.Where(x=> x.ToString() != string.Empty).ToArray();
            uIPickerViewMonths = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIDatePickerViewModel(months, this.Element, this, DateModeSelector.Month),
                BackgroundColor = UIColor.Clear
            };

            entry.InputView = uIPickerViewMonths;
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

        private void SetPlaceHolderText(UITextField entry)
        {
            entry.AttributedPlaceholder = new NSAttributedString(this.Element.Placeholder, null, this.Element.PlaceholderColor.ToUIColor());
        }

        public void SetText(UITextField entry)
        {
            if (!this.Element.NullableDate.HasValue)
            {
                entry.Text = "";
            }
            else
            {
                entry.Text = this.Element.NullableDate.Value.ToString(this.Element.Format);
            }
        }

        public void SetDate(DateTime date)
        {
            this.Element.NullableDate = date;
        }

        private void SetDateFormat()
        {
            if (this.Element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                this.Element.Format = "MMMM yyyy";
            }
            else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                this.Element.Format = "yyyy";

            }
            else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                this.Element.Format = "MMMM";
            }
        }

        private void SetIcon(UITextField entry)
        {
            string rightPath = string.Empty;
            string leftPath = string.Empty;


            //Right Icon
            if (Element.RightIcon == null)
                rightPath = "Samples/Data/SVG/calendar.svg";
            else
                rightPath = Element.RightIcon;

            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = rightPath, HeightRequest = Element.IconHeight, WidthRequest = Element.IconHeight, Color = Element.IconColor };
            var convertedRightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
            var outerView = new UIView(new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
            outerView.AddSubview(convertedRightView);
            entry.RightView = outerView;
            entry.RightViewMode = UITextFieldViewMode.Always;


            //Left Icon
            if (Element.LeftIcon != null)
            {
                RSSvgImage leftSvgIcon = new RSSvgImage() { Source = leftPath, HeightRequest = Element.IconHeight, WidthRequest = Element.IconHeight, Color = Element.IconColor };
                var convertedLeftView = Extensions.ViewExtensions.ConvertFormsToNative(leftSvgIcon, new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
                var outerView2 = new UIView(new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
                outerView2.AddSubview(convertedLeftView);
                entry.LeftView = outerView;
                entry.LeftViewMode = UITextFieldViewMode.Always;
            }
        }

        public void SetPickerSelectedIndicator(DateTime? dateTime = null)
        {
            if (this.Element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                if(dateTime != null)
                {
                    uIPickerViewYears.Select((uIPickerViewYears.Model as CustomUIDatePickerViewModel).PickerItems.IndexOf(dateTime.Value.Year), 0, true);
                    uIPickerViewMonths.Select(dateTime.Value.Month - 1, 0, true);
                }
                else
                {
                    uIPickerViewYears.Select((uIPickerViewYears.Model as CustomUIDatePickerViewModel).PickerItems.IndexOf(DateTime.Now.Year), 0, true);
                    uIPickerViewMonths.Select(DateTime.Now.Month - 1, 0, true);
                }
            }
            else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                if (dateTime != null)
                {
                    var index = (uIPickerViewYears.Model as CustomUIDatePickerViewModel).PickerItems.IndexOf(dateTime.Value.Year);
                    uIPickerViewYears.Select(index, 0, true);
                }
                else
                {
                    uIPickerViewYears.Select((uIPickerViewYears.Model as CustomUIDatePickerViewModel).PickerItems.IndexOf(DateTime.Now.Year), 0, true);
                }
            }
            else if (this.Element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                if (dateTime != null)
                {
                    uIPickerViewMonths.Select(dateTime.Value.Month - 1, 0, true);
                }
                else
                {
                    uIPickerViewMonths.Select(DateTime.Now.Month - 1, 0, true);
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
            if (this.Element.MinimumDate <= date && this.Element.MaximumDate >= date)
                return date;
            else
            {
                if (this.Element.MinimumDate > date)
                    return this.Element.MinimumDate;
                else if (this.Element.MaximumDate < date)
                    return this.Element.MaximumDate;
                else
                    return DateTime.Now;
            }
        }

        public bool IsCorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (this.Element.MinimumDate <= date && this.Element.MaximumDate >= date)
                return true;
            else
                return false;
        }

        protected override UITextField CreateNativeControl()
        {
            return new UITextField { BorderStyle = UITextBorderStyle.RoundedRect };
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                uIDatePicker.ValueChanged -= UIDatePicker_ValueChanged;
            }

            base.Dispose(disposing);
        }

    }

    public class CustomUIDatePickerViewModel : UIPickerViewModel
    {
        public IList<object> PickerItems;
        protected int selectedIndex = 0;
        private RSDatePicker rsDatePicker;
        private RSDatePickerRenderer renderer;
        private DateModeSelector dateModeSelector;
        private DateTime dateTime;

        public CustomUIDatePickerViewModel(System.Collections.IEnumerable items, RSDatePicker rsDatePicker, RSDatePickerRenderer renderer, DateModeSelector dateModeSelector)
        {
            this.rsDatePicker = rsDatePicker;
            this.renderer = renderer;
            this.dateModeSelector = dateModeSelector;

            if (PickerItems == null)
                PickerItems = new List<object>();

            PickerItems.Clear();

            foreach (object item in items)
                PickerItems.Add(item);
        }

        public object SelectedItem
        {
            get { return PickerItems[selectedIndex]; }
        }

        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            return PickerItems.Count;
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            return this.PickerItems[(int)row].ToString();
        }

        public override NSAttributedString GetAttributedTitle(UIPickerView pickerView, nint row, nint component)
        {
            if (rsDatePicker.NullableDate.HasValue)
                dateTime = rsDatePicker.NullableDate.Value;
            else
                dateTime = DateTime.Now;

            if (this.dateModeSelector == DateModeSelector.Month)
            {
                dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);

                if (!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
                {
                    return new NSAttributedString(this.PickerItems[(int)row].ToString(), null, UIColor.Gray);
                }
                else
                    return new NSAttributedString(this.PickerItems[(int)row].ToString(), null, UIColor.Black);
            }
            else
            {
                dateTime = new DateTime((int)PickerItems[(int)row], dateTime.Month, dateTime.Day);

                if (!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
                {
                    return new NSAttributedString(this.PickerItems[(int)row].ToString(), null, UIColor.Gray);
                }
                else
                    return new NSAttributedString(this.PickerItems[(int)row].ToString(), null, UIColor.Black);
            }
        }

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            selectedIndex = (int)row;

            if (rsDatePicker.NullableDate.HasValue)
                dateTime = rsDatePicker.NullableDate.Value;
            else
                dateTime = DateTime.Now;


            if (this.dateModeSelector == DateModeSelector.Year)
            {
                dateTime = new DateTime((int)PickerItems[(int)row], dateTime.Month, dateTime.Day);
            }
            else if (this.dateModeSelector == DateModeSelector.Month)
            {
                dateTime = new DateTime(dateTime.Year, (int)row + 1, dateTime.Day);
            }
            else
            {
                dateTime = new DateTime((int)PickerItems[(int)row], (int)row, dateTime.Day);
            }

            if(!this.renderer.IsCorrectMinMaxDateSelectedValue(dateTime))
            {
                dateTime = this.renderer.CorrectMinMaxDateSelectedValue(dateTime);
                this.renderer.SetPickerSelectedIndicator(dateTime);
                this.renderer.SetDate(this.renderer.CorrectMinMaxDateSelectedValue(dateTime));
            }
            else
                this.renderer.SetDate(dateTime);

            this.renderer.SetText(renderer.Control);
        }
    }

    public enum DateModeSelector
    {
        Year,
        Month,
    }
}