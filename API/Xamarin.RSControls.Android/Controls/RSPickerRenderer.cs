﻿using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSPickerBase), typeof(RSPickerRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPickerRenderer : ViewRenderer<RSPickerBase, AppCompatEditText>, AdapterView.IOnItemClickListener, IDisposable
    {
        private AlertDialog alertDialog;
        private bool isTextInputLayout;
        private CustomBaseAdapter<object> adapter;
        private global::Android.Widget.ListView listViewAndroid;
        private global::Android.Widget.SearchView searchView;
        private List<object> originalItemsList;

        public RSPickerRenderer(Context context) : base(context)
        {
            originalItemsList = new List<object>();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSPickerBase> e)
        {
            base.OnElementChanged(e);

            var nativeEditText = new AppCompatEditText(Context);
            this.SetNativeControl(nativeEditText);

            if (Control == null || e.NewElement == null)
                return;

            nativeEditText.SetSingleLine(true);


            //Draw border or not
            if (Element.HasBorder)
                Extensions.ViewExtensions.DrawBorder(nativeEditText, Context, global::Android.Graphics.Color.Black);

            //Set icon
            SetIcon(nativeEditText);

            //Set placeholder text
            SetPlaceHolderText();

            //Show datepicker
            this.Control.Click += OnPickerClick;
            this.Control.FocusChange += OnPickerFocusChange;


            //Set picker text
            SetText();

            this.Control.KeyListener = null;
            this.Control.Enabled = Element.IsEnabled;
            this.Control.SetTextSize(ComplexUnitType.Dip, (float)Element.FontSize);
        }

        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
        }

        private void SetPlaceHolderText()
        {
            this.Control.Hint = this.Element.Placeholder;
            this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
        }

        private void SetText()
        {
            if (this.Element is RSPicker)
            {
                if(this.Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (this.Element.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty((this.Element as RSPicker).DisplayMemberPath))
                            this.Control.Text = Helpers.TypeExtensions.GetPropValue(this.Element.SelectedItem, (Element as RSPicker).DisplayMemberPath).ToString();
                        else
                            this.Control.Text = Element.SelectedItem.ToString();
                    }
                    else
                    {
                        this.Control.Text = "";
                        if (!isTextInputLayout)
                        {
                            this.Control.Hint = this.Element.Placeholder;
                            this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
                        }
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.Element.SelectedItems != null && this.Element.SelectedItems.Count >= 1)
                    {
                        foreach(object item in this.Element.SelectedItems)
                        {
                            if (!string.IsNullOrEmpty((this.Element as RSPicker).DisplayMemberPath))
                                this.Control.Text += Helpers.TypeExtensions.GetPropValue(item, (this.Element as RSPicker).DisplayMemberPath).ToString();
                            else
                                this.Control.Text += item.ToString();

                            if(this.Element.SelectedItems.IndexOf(item) < this.Element.SelectedItems.Count - 1)
                                this.Control.Text += ", ";
                        }
                    }
                    else
                    {
                        this.Control.Text = "";
                        if (!isTextInputLayout)
                        {
                            this.Control.Hint = this.Element.Placeholder;
                            this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
                        }
                    }
                }

            }
            else
            {
                if (this.Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (this.Element.SelectedItem != null)
                        this.Control.Text = Element.SelectedItem.ToString();
                    else
                    {
                        this.Control.Text = "";
                        if (!isTextInputLayout)
                        {
                            this.Control.Hint = this.Element.Placeholder;
                            this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
                        }
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.Element.SelectedItems != null && this.Element.SelectedItems.Count >= 1)
                    {
                        foreach(object item in this.Element.SelectedItems)
                        {
                            this.Control.Text += item.ToString();

                            if (this.Element.SelectedItems.IndexOf(item) < this.Element.SelectedItems.Count - 1)
                                this.Control.Text += ", ";
                        }
                    }
                    else
                    {
                        this.Control.Text = "";
                        if (!isTextInputLayout)
                        {
                            this.Control.Hint = this.Element.Placeholder;
                            this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
                        }
                    }
                }
            }
        }

        private void SetIcon(global::Android.Widget.EditText nativeEditText)
        {
            string rightPath = string.Empty;
            string leftPath = string.Empty;
            Drawable rightDrawable = null;
            Drawable leftDrawable = null;

            //Right Icon
            if (Element.RightIcon == null)
                rightPath = "Samples/Data/SVG/arrow.svg";
            else
                rightPath = Element.RightIcon;

            int pixel = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)Element.IconHeight, Context.Resources.DisplayMetrics);
            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = rightPath, HeightRequest = pixel, WidthRequest = pixel, Color = Element.IconColor };
            var convertedRightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new Rectangle(), Context);
            rightDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedRightView, pixel, pixel));


            //Left Icon
            if (Element.LeftIcon != null)
            {
                leftPath = Element.LeftIcon;
                RSSvgImage leftSvgIcon = new RSSvgImage() { Source = leftPath, HeightRequest = pixel, WidthRequest = pixel, Color = Element.IconColor };
                var convertedLeftView = Extensions.ViewExtensions.ConvertFormsToNative(leftSvgIcon, new Rectangle(), Context);
                leftDrawable = new BitmapDrawable(Context.Resources, Extensions.ViewExtensions.CreateBitmapFromView(convertedLeftView, pixel, pixel));
            }


            //Set Drawable to control
            //nativeEditText.CompoundDrawablePadding = 5;
            nativeEditText.SetCompoundDrawablesRelativeWithIntrinsicBounds(leftDrawable, null, rightDrawable, null);
        }

        private void CreatePickerDialog()
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(Context);


            //LinearLayout
            LinearLayout layout = new LinearLayout(Context);
            layout.Orientation = Orientation.Vertical;


            //SearchView
            searchView = new global::Android.Widget.SearchView(Context);
            searchView.SetQueryHint("Search");
            searchView.SetImeOptions((global::Android.Views.InputMethods.ImeAction)global::Android.Views.InputMethods.ImeFlags.NoExtractUi | global::Android.Views.InputMethods.ImeAction.Search);
            searchView.QueryTextChange += SearchView_QueryTextChange;
            searchView.SetIconifiedByDefault(false);


            //ListView
            listViewAndroid = new global::Android.Widget.ListView(Context);
            listViewAndroid.Divider = null;
            listViewAndroid.OnItemClickListener = this;



            //Listview Adapter
            adapter = new CustomBaseAdapter<object>(Context, this.Element as RSPickerBase, Element.ItemsSource, listViewAndroid);
            listViewAndroid.Adapter = adapter;

            //Set selected item's
            if(Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
            {
                listViewAndroid.ChoiceMode = ChoiceMode.Single;
                //listViewAndroid.SetItemChecked(Element.SelectedIndex, true);
                listViewAndroid.SetSelection(Element.SelectedIndex);

                if (Element.SelectedIndex != -1)
                    adapter.CheckedItems.Add(Element.ItemsSource[Element.SelectedIndex]);
            }
            else
            {
                listViewAndroid.ChoiceMode = ChoiceMode.Multiple;

                if (Element.SelectedItems != null)
                {
                    foreach(object item in Element.SelectedItems)
                    {
                        if (Element.ItemsSource.Contains(item))
                            adapter.CheckedItems.Add(item);
                    }
                }
            }


            //Populate linear layout
            layout.AddView(searchView);
            layout.AddView(listViewAndroid);

            //SetView to dialog
            dialog.SetView(layout);
            dialog.SetTitle(Element.Title);



            dialog.SetPositiveButton("Done", (senderAlert, args) =>
            {
                SetText();
            });
            dialog.SetNegativeButton("Clear Item", (senderAlert, args) =>
            {
                Element.SelectedItem = null;
                Element.SelectedIndex = -1;
                adapter.CheckedItems.Clear();
                if (this.Element.SelectedItems != null)
                    this.Element.SelectedItems.Clear();

                SetText();
            });

            alertDialog = dialog.Show();
        }

        private void SearchView_QueryTextChange(object sender, global::Android.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            if(this.Element.ItemsSource != null)
                adapter.Filter.InvokeFilter(e.NewText);
        }

        public void OnItemClick(AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            var obj = adapter.GetItem(position);
            var instInfo = obj.GetType().GetProperty("Instance");
            var instance = instInfo.GetValue(obj, null);


            if(this.Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
            {
                Element.SelectedItem = instance;
                alertDialog.Dismiss();
            }
            else
            {
                if(this.Element.SelectedItems != null)
                {
                    if (this.Element.SelectedItems.Contains(instance))
                    {
                        this.Element.SelectedItems.Remove(instance);
                        adapter.CheckedItems.Remove(instance);
                    }
                    else
                    {
                        this.Element.SelectedItems.Add(instance);
                        adapter.CheckedItems.Add(instance);
                    }
                }
            }
        }

        private void OnPickerClick(object sender, EventArgs e)
        {
            if (this.Control.HasFocus)
                CreatePickerDialog();
        }

        private void OnPickerFocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
                CreatePickerDialog();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "SelectedItem" || e.PropertyName == "SelectedItems")
            {
                SetText();
            }

            if (e.PropertyName == "Error" && !isTextInputLayout)
                this.Control.Error = (this.Element as RSPickerBase).Error;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.Control.Click -= OnPickerClick;
                this.Control.FocusChange -= OnPickerFocusChange;

                if (this.searchView != null)
                    searchView.QueryTextChange -= SearchView_QueryTextChange;
            }

            base.Dispose(disposing);
        }
    }

    public class CustomBaseAdapter<T> : BaseAdapter<T>, IFilterable
    {
        private Context context;
        private RSPickerBase rsPicker;
        private System.Collections.IList itemsSource;
        private global::Android.Widget.ListView listViewAndroid;
        private List<T> originalItemsList;
        public string DisplayMemberPath;
        public List<object> CheckedItems;


        public CustomBaseAdapter(Context context, RSPickerBase rsPicker, System.Collections.IList itemsSource, global::Android.Widget.ListView listViewAndroid)
        {
            this.context = context;
            this.rsPicker = rsPicker;
            this.itemsSource = itemsSource;
            this.listViewAndroid = listViewAndroid;


            if (rsPicker is RSPicker && rsPicker != null)
                this.DisplayMemberPath = (rsPicker as RSPicker).DisplayMemberPath;

            CheckedItems = new List<object>();
        }

        public override T this[int position]
        {
            get { return (T)this.itemsSource[position]; }
        }
        
        public override int Count
        {
            get
            {
                if (this.itemsSource != null)
                    return this.itemsSource.Count;
                else
                    return 0;
            }
        }

        public Filter Filter
        {
            get
            {
                return new CustomFilter(this);
            }
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
        {
            if(rsPicker.ItemTemplate == null)
            {
                if (convertView == null)
                {
                    if (rsPicker.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                        convertView = LayoutInflater.From(context).Inflate(Resource.Layout.select_dialog_singlechoice_material, null);
                    else
                        convertView = LayoutInflater.From(context).Inflate(Resource.Layout.select_dialog_multichoice_material, null);
                }

                var obj = this.GetItem(position);
                var instInfo = obj.GetType().GetProperty("Instance");
                var instance = instInfo.GetValue(obj, null);

                if ((this.rsPicker != null && rsPicker is RSPicker) && !string.IsNullOrEmpty(this.DisplayMemberPath))
                    (convertView as CheckedTextView).Text = Helpers.TypeExtensions.GetPropValue(instance, this.DisplayMemberPath).ToString();
                else
                    (convertView as CheckedTextView).Text = this.GetItem(position).ToString();


                if (CheckedItems.Any())
                {
                    if (CheckedItems.Contains(instance))
                        listViewAndroid.SetItemChecked(position, true);
                    else
                        listViewAndroid.SetItemChecked(position, false);
                }
            }
            else
            {
                if (convertView == null)
                {
                    Xamarin.Forms.View view = rsPicker.ItemTemplate.CreateContent() as Xamarin.Forms.View;
                    var renderer = Platform.CreateRendererWithContext(view, context);
                    //renderer.Element.HeightRequest = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, context.Resources.DisplayMetrics); ;
                    Platform.SetRenderer(view, renderer);
                    convertView = new ViewCellContainer(this.context, view, renderer);
                }

                var obj = this.GetItem(position);
                var instInfo = obj.GetType().GetProperty("Instance");
                var instance = instInfo.GetValue(obj, null);
                (convertView as ViewCellContainer)._formsView.BindingContext = instance;
                //(convertView as IVisualElementRenderer).Element.BindingContext = instance;
                if (CheckedItems.Any())
                {
                    if (CheckedItems.Contains(instance))
                    {
                        listViewAndroid.SetItemChecked(position, true);
                        convertView.SetBackgroundColor(global::Android.Graphics.Color.LightGray);
                    }
                    else
                    {
                        listViewAndroid.SetItemChecked(position, false);
                        convertView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                    }
                }
            }


            return convertView;
        }

        public class ViewCellContainer : global::Android.Views.ViewGroup
        {
            public  Xamarin.Forms.View _formsView;
            private IVisualElementRenderer _renderer;

            public ViewCellContainer(global::Android.Content.Context context, Xamarin.Forms.View formsView, IVisualElementRenderer renderer) : base(context)
            {
                _formsView = formsView;
                 _renderer = renderer;
                this.AddView(_renderer.View);
            }

            // this will layout the cell in xamarin forms and then get the height
            // it means you can variable height cells / wrap to content etc
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                double pixels = MeasureSpec.GetSize(widthMeasureSpec);
                double num = ContextExtensions.FromPixels(this.Context, pixels);
                SizeRequest sizeRequest = _formsView.Measure(num, double.PositiveInfinity);
                _formsView.Layout(new Rectangle(0.0, 0.0, num, sizeRequest.Request.Height));
                double width = _formsView.Width;
                int measuredWidth = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, width), MeasureSpecMode.Exactly);
                double height = _formsView.Height;
                int measuredHeight = MeasureSpec.MakeMeasureSpec((int)ContextExtensions.ToPixels(this.Context, height), MeasureSpecMode.Exactly);
                _renderer.View.Measure(widthMeasureSpec, heightMeasureSpec);
                this.SetMeasuredDimension(measuredWidth, measuredHeight);
            }

            protected override void OnLayout(bool changed, int l, int t, int r, int b)
            {
                _renderer.UpdateLayout();
            }
        }

        //Custom Filter Class
        private class CustomFilter : Filter
        {
            private readonly CustomBaseAdapter<T> adapter;


            public CustomFilter(CustomBaseAdapter<T> adapter)
            {
                this.adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var filterResults = new FilterResults();
                var results = new List<T>();

                if (this.adapter.originalItemsList == null)
                {
                    this.adapter.originalItemsList = new List<T>();

                    foreach (T item in this.adapter.itemsSource)
                        this.adapter.originalItemsList.Add(item);
                }

                if (constraint == null)
                    return filterResults;

                if (this.adapter.originalItemsList != null && this.adapter.originalItemsList.Any())
                {
                    // Compare constraint to all names lowercased. 
                    // It they are contained they are added to results.
                    if(this.adapter.DisplayMemberPath != null)
                        results.AddRange(this.adapter.originalItemsList.Where(x => Helpers.TypeExtensions.GetPropValue(x, this.adapter.DisplayMemberPath).ToString().ToLower().Contains(constraint.ToString())));
                    else
                        results.AddRange(this.adapter.originalItemsList.Where(x => x.ToString().ToLower().Contains(constraint.ToString())));

                }

                filterResults.Values = FromArray(results.Select(r => r.ToJavaObject()).ToArray());

                filterResults.Count = results.Count;

                constraint.Dispose();

                return filterResults;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                using (var values = results.Values)
                {
                    this.adapter.itemsSource = values.ToArray<Java.Lang.Object>().Select(a => a.ToNetObject<T>()).ToArray();
                }

                this.adapter.NotifyDataSetChanged();

                constraint.Dispose();
                results.Dispose();
            }
        }
    }

    public class JavaHolder : Java.Lang.Object
    {
        public readonly object Instance;

        public JavaHolder(object instance)
        {
            Instance = instance;
        }
    }

    public static class ObjectExtensions
    {
        public static TObject ToNetObject<TObject>(this Java.Lang.Object value)
        {
            if (value == null)
                return default(TObject);

            if (!(value is JavaHolder))
                throw new InvalidOperationException("Unable to convert to .NET object. Only Java.Lang.Object created with .ToJavaObject() can be converted.");

            TObject returnVal;
            try
            {
                returnVal = (TObject)((JavaHolder)value).Instance;
            }
            finally
            {
                value.Dispose();
            }

            return returnVal;
        }

        public static Java.Lang.Object ToJavaObject<TObject>(this TObject value)
        {
            if (Equals(value, default(TObject)) && !typeof(TObject).IsValueType)
                return null;

            var holder = new JavaHolder(value);

            return holder;
        }
    }
}