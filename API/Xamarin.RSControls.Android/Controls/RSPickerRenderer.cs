using Android.Content;
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
using Xamarin.RSControls.Interfaces;

[assembly: ExportRenderer(typeof(RSPickerBase), typeof(RSPickerRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPickerRenderer : PickerRenderer, AdapterView.IOnItemClickListener, IDisposable
    {
        private RSPopupRenderer sPopupRenderer;
        private AlertDialog alertDialog;
        private bool isTextInputLayout;
        private CustomBaseAdapter<object> adapter;
        private global::Android.Widget.ListView listViewAndroid;
        private global::Android.Widget.SearchView searchView;
        private List<object> originalItemsList;
        private RSPickerBase ElementCasted;

        public RSPickerRenderer(Context context) : base(context)
        {
            originalItemsList = new List<object>();
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;

            ElementCasted = this.Element as RSPickerBase;

            this.Control.SetSingleLine(true);

            //Show datepicker
            this.Control.Click += OnPickerClick;
            this.Control.FocusChange += OnPickerFocusChange;


            //Set picker text
            SetText();

            this.Control.KeyListener = null;
            //this.Control.Enabled = ElementCasted.IsEnabled;
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "SelectedItem" || e.PropertyName == "SelectedItems")
            {
                SetText();
            }

            if (e.PropertyName == "Error" && this.Control is CustomEditText && !isTextInputLayout)
                (this.Control as CustomEditText).ErrorMessage = (this.Element as RSPickerBase).Error;
        }

        protected override EditText CreateNativeControl()
        {
            if ((this.Element as IRSControl).RightIcon == null)
                (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Samples/Data/SVG/arrow.svg" }
                };

            return new CustomEditText(Context, this.Element as IRSControl);
        }


        internal void SetIsTextInputLayout(bool value)
        {
            isTextInputLayout = value;
        }

        private void SetText()
        {
            if (this.ElementCasted is RSPicker)
            {
                if(this.ElementCasted.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (this.ElementCasted.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty((this.ElementCasted as RSPicker).DisplayMemberPath))
                            this.Control.Text = Helpers.TypeExtensions.GetPropValue(this.ElementCasted.SelectedItem, (ElementCasted as RSPicker).DisplayMemberPath).ToString();
                        else
                            this.Control.Text = ElementCasted.SelectedItem.ToString();
                    }
                    else
                    {
                        this.Control.Text = "";
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.ElementCasted.SelectedItems != null && this.ElementCasted.SelectedItems.Count >= 1)
                    {
                        foreach(object item in this.ElementCasted.SelectedItems)
                        {
                            if (!string.IsNullOrEmpty((this.ElementCasted as RSPicker).DisplayMemberPath))
                                this.Control.Text += Helpers.TypeExtensions.GetPropValue(item, (this.ElementCasted as RSPicker).DisplayMemberPath).ToString();
                            else
                                this.Control.Text += item.ToString();

                            if(this.ElementCasted.SelectedItems.IndexOf(item) < this.ElementCasted.SelectedItems.Count - 1)
                                this.Control.Text += ", ";
                        }
                    }
                    else
                    {
                        this.Control.Text = "";
                    }
                }

            }
            else
            {
                if (this.ElementCasted.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (this.ElementCasted.SelectedItem != null)
                        this.Control.Text = ElementCasted.SelectedItem.ToString();
                    else
                    {
                        this.Control.Text = "";
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.ElementCasted.SelectedItems != null && this.ElementCasted.SelectedItems.Count >= 1)
                    {
                        foreach(object item in this.ElementCasted.SelectedItems)
                        {
                            this.Control.Text += item.ToString();

                            if (this.ElementCasted.SelectedItems.IndexOf(item) < this.ElementCasted.SelectedItems.Count - 1)
                                this.Control.Text += ", ";
                        }
                    }

                }
            }
        }

        private global::Android.Views.View CreateDialogLayout()
        {
            //LinearLayout
            LinearLayout layout = new LinearLayout(Context);
            layout.Orientation = Orientation.Vertical;


            //SearchView
            if (this.ElementCasted.IsSearchEnabled && this.ElementCasted.RSPopupStyleEnum == Enums.RSPopupStyleEnum.RsPopopStyle)
            {
                searchView = new global::Android.Widget.SearchView(Context);
                searchView.SetPadding(searchView.PaddingLeft,
                                           searchView.PaddingTop,
                                           searchView.PaddingRight,
                                           (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics));
                searchView.SetQueryHint("Search");
                searchView.SetImeOptions((global::Android.Views.InputMethods.ImeAction)global::Android.Views.InputMethods.ImeFlags.NoExtractUi | global::Android.Views.InputMethods.ImeAction.Search);
                searchView.QueryTextChange -= SearchView_QueryTextChange;
                searchView.QueryTextChange += SearchView_QueryTextChange;
                searchView.SetIconifiedByDefault(false);
            }


            //ListView
            listViewAndroid = new global::Android.Widget.ListView(Context);
            listViewAndroid.SetFooterDividersEnabled(true);
            listViewAndroid.AddFooterView(new global::Android.Views.View(Context), null, true);
            listViewAndroid.OnItemClickListener = this;


            //Listview Adapter
            adapter = new CustomBaseAdapter<object>(Context, this.ElementCasted as RSPickerBase, ElementCasted.ItemsSource, listViewAndroid);
            listViewAndroid.Adapter = adapter;

            //Set selected item's
            if (ElementCasted.SelectionMode == Enums.PickerSelectionModeEnum.Single)
            {
                //Separator visibility
                if(this.ElementCasted.RsPopupSeparatorsUserSet)
                {
                    if (!this.ElementCasted.RsPopupSeparatorsEnabled)
                        listViewAndroid.Divider = null;
                }
                else
                    listViewAndroid.Divider = null;

                listViewAndroid.ChoiceMode = ChoiceMode.Single;
                //listViewAndroid.SetItemChecked(ElementCasted.SelectedIndex, true);
                listViewAndroid.SetSelection(ElementCasted.SelectedIndex);

                if (ElementCasted.SelectedIndex != -1)
                    adapter.CheckedItems.Add(ElementCasted.ItemsSource[ElementCasted.SelectedIndex]);
            }
            else
            {
                listViewAndroid.ChoiceMode = ChoiceMode.Multiple;

                //Separator visibility
                if (this.ElementCasted.RsPopupSeparatorsUserSet)
                    if (!this.ElementCasted.RsPopupSeparatorsEnabled)
                        listViewAndroid.Divider = null;


                if (ElementCasted.SelectedItems != null)
                {
                    foreach (object item in ElementCasted.SelectedItems)
                    {
                        if (ElementCasted.ItemsSource.Contains(item))
                            adapter.CheckedItems.Add(item);
                    }
                }
            }


            //Populate linear layout
            if (this.ElementCasted.IsSearchEnabled && this.ElementCasted.RSPopupStyleEnum == Enums.RSPopupStyleEnum.RsPopopStyle)
                layout.AddView(searchView);

            layout.AddView(listViewAndroid);

            return layout;
        }

        private void CreatePickerDialog()
        {
            sPopupRenderer = new RSPopupRenderer((this.Element as RSPickerBase).RSPopupTitle, (this.Element as RSPickerBase).RSPopupMessage);
            sPopupRenderer.BorderRadius = 14;
            sPopupRenderer.DimAmount = 0.7f;
            sPopupRenderer.BorderFillColor = (this.Element as RSPickerBase).RSPopupBackgroundColor;
            sPopupRenderer.Width = (int)Enums.RSPopupSizeEnum.MatchParent;
            sPopupRenderer.Height = (int)Enums.RSPopupSizeEnum.WrapContent;
            sPopupRenderer.UserSetSize = true;


            //Create layout
            var layout = CreateDialogLayout();

            //SetView to dialog
            sPopupRenderer.SetNativeView(layout);

            sPopupRenderer.AddAction("Done", Enums.RSPopupButtonTypeEnum.Positive, (senderAlert, args) =>
            {
                SetText();
                this.Control.ClearFocus();
            });
            sPopupRenderer.AddAction("Clear Item", Enums.RSPopupButtonTypeEnum.Neutral, (senderAlert, args) =>
            {
                ElementCasted.SelectedItem = null;
                ElementCasted.SelectedIndex = -1;
                adapter.CheckedItems.Clear();
                if (this.ElementCasted.SelectedItems != null)
                    this.ElementCasted.SelectedItems.Clear();

                SetText();
                this.Control.ClearFocus();
                sPopupRenderer.Dismiss();
            });

            sPopupRenderer.ShowPopup();
        }

        private void CreateNativePickerDialog()
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(Context);


            //Create layout
            var layout = CreateDialogLayout();
            //layout.SetPadding(0, (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics), 0, 0);

            ////SetView to dialog
            dialog.SetView(layout);
            //dialog.SetAdapter(adapter, this);
            dialog.SetTitle(ElementCasted.Title);


            dialog.SetPositiveButton("Done", (senderAlert, args) =>
            {
                SetText();
                this.Control.ClearFocus();
            });
            dialog.SetNegativeButton("Clear Item", (senderAlert, args) =>
            {
                ElementCasted.SelectedItem = null;
                ElementCasted.SelectedIndex = -1;
                adapter.CheckedItems.Clear();
                if (this.ElementCasted.SelectedItems != null)
                    this.ElementCasted.SelectedItems.Clear();

                SetText();
                this.Control.ClearFocus();
            });

            alertDialog = dialog.Show();
        }

        private void SearchView_QueryTextChange(object sender, global::Android.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            if(this.ElementCasted.ItemsSource != null)
                adapter.Filter.InvokeFilter(e.NewText);
        }

        public void OnItemClick(AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            var obj = adapter.GetItem(position);
            var instInfo = obj.GetType().GetProperty("Instance");
            var instance = instInfo.GetValue(obj, null);

            if(this.ElementCasted.ItemTemplate != null)
            {
                //Force check when using custom template, when not using custom template CheckedTextView gets checked or not (Android implemantation) since it's the main view for the row
                var item = ((view as LinearLayout).GetChildAt(0) as CheckedTextView);

                if (item.Checked)
                    item.Checked = false;
                else
                    item.Checked = true;
            }

            if (this.ElementCasted.SelectionMode == Enums.PickerSelectionModeEnum.Single)
            {
                ElementCasted.SelectedItem = instance;

                if(alertDialog != null)
                    alertDialog.Dismiss();

                if(sPopupRenderer != null)
                    sPopupRenderer.Dismiss();
            }
            else
            {
                if(this.ElementCasted.SelectedItems != null)
                {
                    if (this.ElementCasted.SelectedItems.Contains(instance))
                    {
                        this.ElementCasted.SelectedItems.Remove(instance);
                        adapter.CheckedItems.Remove(instance);
                    }
                    else
                    {
                        this.ElementCasted.SelectedItems.Add(instance);
                        adapter.CheckedItems.Add(instance);
                    }
                }
            }
        }
        private void OnPickerClick(object sender, EventArgs e)
        {
            if (this.Control.HasFocus)
            {
                if (this.ElementCasted.RSPopupStyleEnum == Enums.RSPopupStyleEnum.RsPopopStyle)
                    CreatePickerDialog();
                else
                    CreateNativePickerDialog();
            }
        }
        private void OnPickerFocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                if (this.ElementCasted.RSPopupStyleEnum == Enums.RSPopupStyleEnum.RsPopopStyle)
                    CreatePickerDialog();
                else
                    CreateNativePickerDialog();
            }
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

            //if(sPopupRenderer != null && sPopupRenderer.Dialog != null)
            //    sPopupRenderer.Dialog.Dismiss();
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
        private Xamarin.Forms.View formsView;


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
                    //Selection box
                    global::Android.Views.View selectionIndicator = new CheckedTextView(context);
                    if (rsPicker.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                        selectionIndicator = LayoutInflater.From(context).Inflate(Resource.Layout.select_dialog_singlechoice_material, null);
                    else
                        selectionIndicator = LayoutInflater.From(context).Inflate(Resource.Layout.select_dialog_multichoice_material, null);

                    (selectionIndicator as CheckedTextView).SetPadding((selectionIndicator as CheckedTextView).PaddingLeft,
                                                                       (selectionIndicator as CheckedTextView).PaddingTop,
                                                                       0,
                                                                       (selectionIndicator as CheckedTextView).PaddingBottom);


                    //Item template
                    formsView = rsPicker.ItemTemplate.CreateContent() as Xamarin.Forms.View;
                    //var nativeView = Extensions.ViewExtensions.ConvertFormsToNative(context, formsView, formsView.X, formsView.Y, 300, selectionIndicator.MinimumHeight);
                    var renderer = Platform.CreateRendererWithContext(formsView, context);
                    var nativeView = new ViewCellContainer(context, formsView, renderer, selectionIndicator.MinimumHeight);
                    convertView = new LinearLayout(this.context);
                    (convertView as LinearLayout).SetGravity(GravityFlags.CenterVertical);
                    (convertView as LinearLayout).AddView(selectionIndicator);
                    (convertView as LinearLayout).AddView(nativeView);
                }

                var obj = this.GetItem(position);
                var instInfo = obj.GetType().GetProperty("Instance");
                var instance = instInfo.GetValue(obj, null);
                //formsView.BindingContext = instance;
                ((convertView as LinearLayout).GetChildAt(1) as ViewCellContainer)._formsView.BindingContext = instance;
                //(convertView as IVisualElementRenderer).Element.BindingContext = instance;
                if (CheckedItems.Any())
                {
                    if (CheckedItems.Contains(instance))
                    {
                        listViewAndroid.SetItemChecked(position, true);
                        ((convertView as LinearLayout).GetChildAt(0) as CheckedTextView).Checked = true;
                    }
                    else
                    {
                        listViewAndroid.SetItemChecked(position, false);
                        ((convertView as LinearLayout).GetChildAt(0) as CheckedTextView).Checked = false;
                    }
                }
            }


            return convertView;
        }

        public class ViewCellContainer : global::Android.Views.ViewGroup
        {
            public  Xamarin.Forms.View _formsView;
            private IVisualElementRenderer _renderer;
            private double minCellHeight;

            public ViewCellContainer(global::Android.Content.Context context, Xamarin.Forms.View formsView, IVisualElementRenderer renderer, double minCellHeight) : base(context)
            {
                _formsView = formsView;
                 _renderer = renderer;
                this.AddView(_renderer.View);
                this.minCellHeight = ContextExtensions.FromPixels(this.Context, minCellHeight);
            }

            // this will layout the cell in xamarin forms and then get the height
            // it means you can variable height cells / wrap to content etc
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);


                double pixelsWidth = MeasureSpec.GetSize(widthMeasureSpec);
                double numWidth = ContextExtensions.FromPixels(this.Context, pixelsWidth);
                SizeRequest sizeRequest = _formsView.Measure(numWidth, double.PositiveInfinity);

                if(this.minCellHeight < sizeRequest.Request.Height)
                    _formsView.Layout(new Rectangle(0.0, 0.0, numWidth, sizeRequest.Request.Height));
                else
                    _formsView.Layout(new Rectangle(0.0, 0.0, numWidth, this.minCellHeight));

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