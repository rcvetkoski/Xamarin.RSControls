using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ViewPager.Widget;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using static Android.Views.View;

[assembly: ExportRenderer(typeof(RSDatePicker), typeof(RSDatePickerRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSDatePickerRenderer : DatePickerRenderer, IOnClickListener, AdapterView.IOnItemClickListener
    {
        DatePickerDialog _dialog;
        private bool isTextInputLayout;

        //For custom dialogs
        private AlertDialog alert;
        AlertDialog.Builder dialog;
        private RSDatePicker element;

        public RSDatePickerRenderer(Context context) : base(context)
        {

        }

        protected override EditText CreateNativeControl()
        {
            if(Element != null)
            {
                element = Element as RSDatePicker;

                //Set placeholder text
                SetPlaceHolderText();

                //Set date format
                if (!element.HasCustomFormat())
                    SetDateFormat();
            }

            return new CustomEditText(Context, this.Element as IRSControl);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Forms.DatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;

            

            //Show datepicker
            this.Control.Click += OnPickerClick;
            this.Control.FocusChange += OnPickerFocusChange;

            //Set correct value for nulabledate if greater than max date or smaller than min date
            if (element.NullableDate.HasValue)
                element.NullableDate = CorrectMinMaxDateSelectedValue(element.NullableDate.Value);
            //else
            //    element.NullableDate = CorrectMinMaxDateSelectedValue(DateTime.Now);

            //Set picker text
            SetText();

            this.Control.KeyListener = null;
            this.Control.Enabled = Element.IsEnabled;
            this.Control.SetTextSize(ComplexUnitType.Dip, (float)Element.FontSize);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Update picker text if binding value changes
            if (e.PropertyName == "NullableDate")
            {
                //Reset value if not in allowed range
                if (element.NullableDate.HasValue)
                {
                    if (!IsCorrectMinMaxDateSelectedValue(element.NullableDate.Value))
                        element.NullableDate = CorrectMinMaxDateSelectedValue(element.NullableDate.Value);

                }

                SetText();
            }

            if (e.PropertyName == "Error" && !isTextInputLayout)
                this.Control.Error = (element as RSDatePicker).Error;

            base.OnElementPropertyChanged(sender, e);
        }

        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
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

        private void OnPickerClick(object sender, EventArgs e)
        {
            if(this.Control.HasFocus)
                ShowDatePicker();
        }

        private void OnPickerFocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                ShowDatePicker();
        }

        void SetText()
        {
            if (!element.NullableDate.HasValue)
            {
                this.Control.Text = "";
                if(!isTextInputLayout)
                {
                    this.Control.Hint = element.Placeholder;
                    this.Control.SetHintTextColor(element.PlaceholderColor.ToAndroid());
                }
            }
            else
            {
                this.Control.Text = element.NullableDate.Value.ToString(Element.Format);
                this.Control.SetTextColor(Element.TextColor.ToAndroid());
            }
        }

        void SetDate(DateTime date)
        {
            element.NullableDate = date;
            SetText();
            element.DoInvalidate(); // TODO resize if set to auto
        }

        DateTime CorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (element.MinimumDate <= date && element.MaximumDate >= date)
                return date;
            else
            {
                if (element.MinimumDate > date)
                    return element.MinimumDate;
                else if (element.MaximumDate < date)
                    return element.MaximumDate;
                else
                    return DateTime.Now;
            }
        }

        public bool IsCorrectMinMaxDateSelectedValue(DateTime date)
        {
            if (element.MinimumDate <= date && element.MaximumDate >= date)
                return true;
            else
                return false;
        }

        private void SetDateFormat()
        {
            if (element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                element.Format = "MMMM yyyy";
            }
            else if (element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                element.Format = "yyyy";

            }
            else if (element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                element.Format = "MMMM";
            }
        }

        private void ShowDatePicker(DateTime? dateTime = null)
        {
            if (element.DateSelectionMode == DateSelectionModeEnum.Default)
            {
                if (dateTime.HasValue)
                    CreateDatePickerDialog(dateTime.Value.Year, dateTime.Value.Month, dateTime.Value.Day);
                else if (element.NullableDate.HasValue)
                {
                    var tempDate = CorrectMinMaxDateSelectedValue(element.NullableDate.Value);
                    CreateDatePickerDialog(tempDate.Year, tempDate.Month, tempDate.Day);
                }
                else
                {
                    var tempDate = CorrectMinMaxDateSelectedValue(DateTime.Now);
                    CreateDatePickerDialog(tempDate.Year, tempDate.Month, tempDate.Day);
                }

                _dialog.Show();
            }
            else if (element.DateSelectionMode == DateSelectionModeEnum.Month ||
                    element.DateSelectionMode == DateSelectionModeEnum.MonthYear ||
                    element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                if (dateTime.HasValue)
                    CreateMothYearPickerDialog(dateTime.Value);
                else if (element.NullableDate.HasValue)
                    CreateMothYearPickerDialog(CorrectMinMaxDateSelectedValue(element.NullableDate.Value));
                else
                    CreateMothYearPickerDialog(CorrectMinMaxDateSelectedValue(DateTime.Now));
            }
        }


        #region DatePickerDialog

        void CreateDatePickerDialog(int year, int month, int day)
        {
            RSDatePicker view = element;
            _dialog = new DatePickerDialog(Context, (o, e) =>
            {
                view.Date = e.Date;
                ((IElementController)view).SetValueFromRenderer(VisualElement.IsFocusedProperty, false);

                _dialog = null;
            }, year, month - 1, day);


            _dialog.DatePicker.MaxDate = (long)view.MaximumDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            _dialog.DatePicker.MinDate = (long)view.MinimumDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            _dialog.SetButton(Context.GetString(global::Android.Resource.String.Ok), (sender, e) =>
            {
                SetDate(_dialog.DatePicker.DateTime);
                element.Format = element.Format;
                this.Control.SetTextColor(Element.TextColor.ToAndroid());
            });

            if (element.IsClearable)
            {
                _dialog.SetButton2("Clear", (sender, e) =>
                {
                    element.CleanDate();
                    SetText();
                    element.DoInvalidate(); // TODO resize if set to auto
                });
            }
        }

        #endregion


        #region Month/YearMonth Picker dialog

        // Years used im month and year selection
        private int[] years;
        private int selectedYearIndex = 0;
        private void SetYears(DateTime? intDate)
        {
            years = Enumerable.Range(element.MinimumDate.Year, element.MaximumDate.Year - element.MinimumDate.Year + 1).ToArray();

            if (element != null && element.DateSelectionMode != DateSelectionModeEnum.Month)
            {
                selectedYearIndex = Array.IndexOf(years, intDate.Value.Year);
            }
            else
            {
                years = new int[] { years[Array.IndexOf(years, intDate.Value.Year)] };
                selectedYearIndex = 0;
            }
        }


        // Set when dae is picked before pressing ok button
        private DateTime? internalDate;

        // Month/Year switch
        public void OnClick(global::Android.Views.View v)
        {
            if (v.Id == Resource.Id.headerMonthTitle)
            {
                headerYearTitle.Selected = false;
                headerMonthTitle.Selected = true;
                yearView.Visibility = ViewStates.Gone;
                yearView.Animate().Alpha(0.0f);
                monthView.Visibility = ViewStates.Visible;
                monthView.Animate().Alpha(1.0f);

            }
            else if (v.Id == Resource.Id.headerYearTitle)
            {
                headerYearTitle.Selected = true;
                headerMonthTitle.Selected = false;
                monthView.Visibility = ViewStates.Gone;
                monthView.Animate().Alpha(0.0f);
                yearView.Visibility = ViewStates.Visible;
                yearView.Animate().Alpha(1.0f);
                yearView.SetSelection(Array.IndexOf(years, yearListAdapter.GetSelectedYear()));
            }
        }

        #region Month

        private TextView headerMonthTitle;
        private global::Android.Widget.RelativeLayout monthView;
        private ViewPager viewPager;
        /// <summary>
        /// Selected month index
        /// </summary>
        private global::Android.Widget.Button leftButton;
        private global::Android.Widget.Button rightButton;

        // Create month view
        private void CreateMonthView(global::Android.Views.View monthYearPickerDialogView)
        {
            // Create views
            headerMonthTitle = monthYearPickerDialogView.FindViewById<TextView>(Resource.Id.headerMonthTitle);
            monthView = monthYearPickerDialogView.FindViewById<global::Android.Widget.RelativeLayout>(Resource.Id.monthView);
            leftButton = monthYearPickerDialogView.FindViewById<global::Android.Widget.Button>(Resource.Id.leftButton);
            rightButton = monthYearPickerDialogView.FindViewById<global::Android.Widget.Button>(Resource.Id.rightButton);
            viewPager = monthYearPickerDialogView.FindViewById<ViewPager>(Resource.Id.viewPager);
            PagerTitleStrip pagerIndicator = viewPager.FindViewById<PagerTitleStrip>(Resource.Id.pager_title_strip);

            leftButton.Click += leftButton_Click;
            rightButton.Click += rightButton_Click;

            pagerIndicator.SetNonPrimaryAlpha(0f); // Previous and next item set to invisible

            var adapter = new MonthViewPagerAdapter(Context, years, this);
            viewPager.Adapter = adapter;
            viewPager.AddOnPageChangeListener(adapter);
            
            viewPager.CurrentItem = selectedYearIndex;
        }

        private void leftButton_Click(object sender, EventArgs e)
        {
            if (viewPager.CurrentItem - 1 <= years.Count())
                viewPager.SetCurrentItem(viewPager.CurrentItem - 1, true);
        }
        private void rightButton_Click(object sender, EventArgs e)
        {
            if (viewPager.CurrentItem + 1 >= 0)
                viewPager.SetCurrentItem(viewPager.CurrentItem + 1, true);
        }

        // Manth viewPager adapter
        private class MonthViewPagerAdapter : PagerAdapter, IOnClickListener, ViewPager.IOnPageChangeListener
        {
            private Context context;
            private int[] years;
            private string[] invariantMonths;
            private string[] months;
            private RSDatePickerRenderer picker;
            private Drawable selectedBackgroundDrawable;
            private CustomTextView selectedView;
            private bool initSelectedMonthFinished;

            public MonthViewPagerAdapter(Context context, int[] years, RSDatePickerRenderer picker)
            {
                this.context = context;
                this.years = years;
                this.invariantMonths = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
                this.months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                this.picker = picker;
                this.selectedBackgroundDrawable = this.context.GetDrawable(Resource.Drawable.RSOvalShapeDrawable);
                initSelectedMonthFinished = false;
            }

            public override int Count => years.Count();

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                global::Android.Widget.RelativeLayout monthContainer = LayoutInflater.From(context).Inflate(Resource.Layout.RSMonthViewItem, null) as global::Android.Widget.RelativeLayout;
                global::Android.Widget.GridLayout month = monthContainer.FindViewById<global::Android.Widget.GridLayout>(Resource.Id.month);

                for (int i = 0; i < this.invariantMonths.Length - 1; i++)
                {
                    CreateMonthTextView(month, years[position], i);
                }

                container.AddView(monthContainer);
                monthContainer.Tag = position;

                initSelectedMonthFinished = true;

                return monthContainer;
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
            {
                container.RemoveView(@object as global::Android.Views.View);
            }

            private CustomTextView CreateMonthTextView(GridLayout monthLayout, int year, int monthPosition)
            {
                CustomTextView textView = new CustomTextView(this.context);
                textView.Text = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(this.invariantMonths[monthPosition].Substring(0, 3));
                textView.Position = monthPosition;

                int month = monthPosition + 1;
                DateTime date = new DateTime(year, monthPosition + 1, DateTime.DaysInMonth(year, month));
                DateTime min = new DateTime(picker.Element.MinimumDate.Year, picker.Element.MinimumDate.Month,
                                   DateTime.DaysInMonth(picker.Element.MinimumDate.Year, picker.Element.MinimumDate.Month));
                DateTime max = new DateTime(picker.Element.MaximumDate.Year, picker.Element.MaximumDate.Month,
                                   DateTime.DaysInMonth(picker.Element.MaximumDate.Year, picker.Element.MaximumDate.Month));

                // Disabled or not
                if (date >= min && date <= max)
                {
                    textView.SetTextColor(global::Android.Graphics.Color.Black);
                    textView.SetOnClickListener(this);
                }
                else
                    textView.SetTextColor(global::Android.Graphics.Color.Gray);

                // Selected or not only fired once
                if (!initSelectedMonthFinished)
                {
                    if (year == picker.internalDate.Value.Year && month == picker.internalDate.Value.Month)
                    {
                        Select(textView, year, monthPosition);
                    }
                }

                monthLayout.AddView(textView);
                return textView;
            }

            private void Select(CustomTextView month, int year, int monthPosition)
            {
                if (picker.headerYearTitle != null)
                    picker.headerYearTitle.Text = year.ToString();

                if (selectedView != null)
                {
                    selectedView.Background = new ColorDrawable(global::Android.Graphics.Color.Transparent);
                    selectedView.SetTextColor(global::Android.Graphics.Color.Black);
                }

                int day = 1;
                if (DateTime.DaysInMonth(year, monthPosition + 1) >= picker.internalDate.Value.Day)
                    day = picker.internalDate.Value.Day;

                picker.internalDate = new DateTime(year, monthPosition + 1, day);

                selectedView = month;

                month.Background = selectedBackgroundDrawable;
                month.SetTextColor(global::Android.Graphics.Color.White);
                picker.headerMonthTitle.Text = this.months[monthPosition];
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(years[position].ToString());
            }

            public void OnClick(global::Android.Views.View v)
            {
                CustomTextView textView = (CustomTextView)v;
                int year = years[picker.viewPager.CurrentItem];
                int month = textView.Position;

                //Set Year index if year selection possible
                if (this.picker.yearListAdapter != null)
                {
                    this.picker.yearListAdapter.SetSelectedYearIndex(picker.viewPager.CurrentItem);
                    this.picker.yearListAdapter.NotifyDataSetChanged();
                    //Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {this.picker.yearListAdapter.NotifyDataSetChanged();});
                }

                Select(textView, year, month);
            }

            public override bool IsViewFromObject(global::Android.Views.View view, Java.Lang.Object @object)
            {
                return view == @object;
            }

            public void OnPageSelected(int position)
            {
                if (initSelectedMonthFinished)
                {
                    var relLayout = (global::Android.Widget.RelativeLayout)picker.viewPager.FindViewWithTag(position);
                    var gridLayout = (global::Android.Widget.GridLayout)relLayout.GetChildAt(0);
                    var month = gridLayout.GetChildAt(picker.internalDate.Value.Month - 1) as CustomTextView;
                    if (years[position] == picker.internalDate.Value.Year && month.Position == picker.internalDate.Value.Month - 1)
                        Select(month, years[position], picker.internalDate.Value.Month - 1);
                }
            }

            //Not Used
            public void OnPageScrollStateChanged(int state)
            {

            }

            //Not Used
            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {

            }
        }

        // Helper for month Selection
        private class CustomTextView : TextView
        {
            public int Position { get; set; }

            public CustomTextView(Context context) : base(context)
            {
                //TextAlignment = Android.Views.TextAlignment.Center;
                SetTextSize(ComplexUnitType.Sp, 18);
                Gravity = GravityFlags.Center;
                GridLayout.LayoutParams param = new GridLayout.LayoutParams();
                param.Height = Resources.GetDimensionPixelSize(Resource.Dimension.text_view_width);
                param.Width = Resources.GetDimensionPixelSize(Resource.Dimension.text_view_width);
                //param.SetMargins(10, 15, 10, 15);
                // param.SetGravity(GravityFlags.Center);
                //param.RowSpec = GridLayout.InvokeSpec(GridLayout.Undefined, 1f);
                //param.ColumnSpec = GridLayout.InvokeSpec(GridLayout.Undefined, 1f);
                LayoutParameters = param;
                SetTextColor(global::Android.Graphics.Color.Black);
            }
        }

        #endregion

        #region Year
        private TextView headerYearTitle;
        private global::Android.Widget.ListView yearView;
        private YearListAdapter yearListAdapter;

        // Create year view
        private void CreateYearView(global::Android.Views.View monthYearPickerDialogView)
        {
            headerYearTitle = monthYearPickerDialogView.FindViewById<TextView>(Resource.Id.headerYearTitle);
            yearView = monthYearPickerDialogView.FindViewById<global::Android.Widget.ListView>(Resource.Id.yearView);

            selectedYearIndex = Array.IndexOf(years, internalDate.Value.Year);

            if (selectedYearIndex != -1)
                headerYearTitle.Text = years[selectedYearIndex].ToString();

            yearListAdapter = new YearListAdapter(Context, years, selectedYearIndex);
            yearView.Adapter = yearListAdapter;

            yearView.OnItemClickListener = this;

            // Set scroll to selected item position 
            yearView.SetSelection(selectedYearIndex);
        }

        // Year list adapter
        private class YearListAdapter : BaseAdapter<int>
        {
            private int[] items;
            private int selectedYearIndex;
            private TextView selectedView;
            private Context context;

            public YearListAdapter(Context cntx, int[] items, int selectedYearIndex) : base()
            {
                this.context = cntx;
                this.items = items;

                this.selectedYearIndex = selectedYearIndex;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override int this[int position]
            {
                get { return items[position]; }
            }

            public override int Count
            {
                get { return items.Length; }
            }

            public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
            {
                global::Android.Views.View view = null;

                view = convertView; // re-use an existing view, if one is supplied

                if (view == null) // otherwise create a new one
                    view = LayoutInflater.From(context).Inflate(Resource.Layout.RSYearItemView, null);

                // set view properties to reflect data for the given row
                (view as TextView).Text = items[position].ToString();

                if (this.selectedYearIndex == position)
                {
                    (view as TextView).SetTextColor(global::Android.Graphics.Color.Red);
                    (view as TextView).SetTextSize(ComplexUnitType.Sp, 22);

                    selectedView = view as TextView;
                }
                else
                {
                    (view as TextView).SetTextColor(global::Android.Graphics.Color.Black);
                    (view as TextView).SetTextSize(ComplexUnitType.Sp, 16);
                }

                // return the view, populated with data, for display
                return view;
            }

            public int GetSelectedYear()
            {
                return this.items[this.selectedYearIndex];
            }

            public void SetSelectedYearIndex(int value)
            {
                this.selectedYearIndex = value;
            }

            public TextView GetSelectedView()
            {
                return this.selectedView;
            }

            public void SetSelectedView(TextView view)
            {
                this.selectedView = view;
            }
        }

        // ListView Selected item event
        public void OnItemClick(AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            TextView selected = view as TextView;
            TextView selectedView = yearListAdapter.GetSelectedView();

            if (selectedView != null)
            {
                selectedView.SetTextColor(global::Android.Graphics.Color.Black);
                selectedView.SetTextSize(ComplexUnitType.Sp, 16);
            }

            selected.SetTextColor(global::Android.Graphics.Color.Red);
            selected.SetTextSize(ComplexUnitType.Sp, 22);

            yearListAdapter.SetSelectedYearIndex(position);
            yearListAdapter.SetSelectedView(selected);
            headerYearTitle.Text = selected.Text;

            int day = 1;
            if (DateTime.DaysInMonth(yearListAdapter.GetSelectedYear(), this.internalDate.Value.Month) >= this.internalDate.Value.Day)
                day = this.internalDate.Value.Day;

            //Set internal date to allowed date
            if (yearListAdapter.GetSelectedYear() == element.MinimumDate.Year && element.MinimumDate.Month > internalDate.Value.Month)
                internalDate = new DateTime(yearListAdapter.GetSelectedYear(), element.MinimumDate.Month, day);
            else if (yearListAdapter.GetSelectedYear() == element.MaximumDate.Year && element.MaximumDate.Month < internalDate.Value.Month)
                internalDate = new DateTime(yearListAdapter.GetSelectedYear(), element.MaximumDate.Month, day);
            else
                internalDate = new DateTime(yearListAdapter.GetSelectedYear(), internalDate.Value.Month, day);

            if (viewPager != null)
                viewPager.CurrentItem = position; // Set Month viewPager current year (position)

            if (monthView != null && monthView.Visibility != ViewStates.Visible)
            {
                headerYearTitle.Selected = false;
                headerMonthTitle.Selected = true;
                yearView.Visibility = ViewStates.Gone;
                yearView.Animate().Alpha(0.0f);
                monthView.Visibility = ViewStates.Visible;
                monthView.Animate().Alpha(1.0f);
            }
        }

        #endregion

        #region CREATE PICKER DIALOG

        public void CreateMothYearPickerDialog(DateTime? intDate = null)
        {
            // Get dialog
            var monthYearPickerDialogView = LayoutInflater.From(Context).Inflate(Resource.Layout.RSMonthYearPickerDialog, null);

            // Set years selection array
            SetYears(intDate);

            //Set internal date
            internalDate = intDate;

            // Visibilites and header clicks enable disable
            if (element.DateSelectionMode == DateSelectionModeEnum.MonthYear)
            {
                // Year
                CreateYearView(monthYearPickerDialogView);
                yearView.Visibility = ViewStates.Gone;
                yearView.Selected = false;
                yearView.Alpha = 0.0f;
                headerYearTitle.Visibility = ViewStates.Visible;
                headerYearTitle.SetOnClickListener(this);

                // Month
                CreateMonthView(monthYearPickerDialogView);
                monthView.Visibility = ViewStates.Visible;
                headerMonthTitle.Visibility = ViewStates.Visible;
                headerMonthTitle.Selected = true;
                headerMonthTitle.SetOnClickListener(this);
            }
            else if (element.DateSelectionMode == DateSelectionModeEnum.Month)
            {
                // Month
                CreateMonthView(monthYearPickerDialogView);
                monthView.Visibility = ViewStates.Visible;
                headerMonthTitle.Selected = true;
                headerMonthTitle.Visibility = ViewStates.Visible;

                global::Android.Widget.Button leftButton = monthView.FindViewById<global::Android.Widget.Button>(Resource.Id.leftButton);
                global::Android.Widget.Button rightButton = monthView.FindViewById<global::Android.Widget.Button>(Resource.Id.rightButton);
                leftButton.Visibility = ViewStates.Gone;
                rightButton.Visibility = ViewStates.Gone;
            }
            else if (element.DateSelectionMode == DateSelectionModeEnum.Year)
            {
                // Year
                CreateYearView(monthYearPickerDialogView);
                yearView.Visibility = ViewStates.Visible;
                yearView.Selected = true;
                headerYearTitle.Visibility = ViewStates.Visible;
            }

            // Create dialog
            dialog = new AlertDialog.Builder(Context, Resource.Style.AppCompatDialogStyle);
            dialog.SetPositiveButton(Context.GetString(global::Android.Resource.String.Ok), (senderAlert, args) =>
            {
                int y = internalDate.Value.Year;
                int m = internalDate.Value.Month;
                int d = internalDate.Value.Day;

                var date = new DateTime(y, m, d);

                SetDate(date);
            });
            if (element.IsClearable)
            {
                dialog.SetNegativeButton("Clear", (sender, e) =>
                {
                    element.CleanDate();
                    SetText();
                    element.DoInvalidate(); // TODO resize if set to auto
                });
            }
            else
            {
                dialog.SetNegativeButton(Context.GetString(global::Android.Resource.String.Cancel), (senderAlert, args) =>
                {

                });
            }

            // Set and show monthYearPickerDialogView to dialog popup
            dialog.SetView(monthYearPickerDialogView);
            alert = dialog.Show();
        }

        #endregion

        #endregion


        //Recreate dialog when orientation changed
        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            // Default picker
            if (_dialog != null && _dialog.IsShowing)
            {
                var selectedDate = _dialog.DatePicker.DateTime;
                _dialog.Dismiss();
                _dialog.Dispose();
                _dialog = null;
                ShowDatePicker(selectedDate);
            }

            // Month/Year picker
            if (alert != null && alert.IsShowing)
            {
                var selectedDate = internalDate.Value;
                alert.Dismiss();
                alert.Dispose();
                alert = null;
                ShowDatePicker(selectedDate);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null)
            {
                this.Control.Click -= OnPickerClick;
                this.Control.FocusChange -= OnPickerFocusChange;

                if (_dialog != null)
                {
                    _dialog.Hide();
                    _dialog.Dispose();
                    _dialog = null;
                }
            }

            if(leftButton != null)
                leftButton.Click -= leftButton_Click;

            if(leftButton != null)
                rightButton.Click -= rightButton_Click;

            base.Dispose(disposing);
        }
    }
}