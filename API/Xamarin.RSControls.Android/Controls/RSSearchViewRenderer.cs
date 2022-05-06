using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSSearchView), typeof(RSSearchViewRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSSearchViewRenderer : SearchBarRenderer, global::Android.Widget.AdapterView.IOnItemClickListener
    {
        private List<object> objectList;
        private List<string> arrayList;
        AutoCompleteTextView searchBox;
        private RSSearchView rSSearchView;

        public RSSearchViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
                return;

            //Fix dropdown not showing or hiding behind softkeyboard
            var window = ((global::Android.App.Activity)Context).Window;
            window.SetSoftInputMode(SoftInput.AdjustResize);

            rSSearchView = this.Element as RSSearchView;

            if (rSSearchView.ItemsSource == null)
                return;

            objectList = new List<object>();
            arrayList = new List<string>();

            foreach (var item in rSSearchView.ItemsSource)
            {
                if (!string.IsNullOrEmpty(rSSearchView.DisplayMemberPath))
                    arrayList.Add(Helpers.TypeExtensions.GetPropValue(item, rSSearchView.DisplayMemberPath).ToString());
                else
                    arrayList.Add(item.ToString());

                objectList.Add(item);
            }

            AutoCompleteTextViewAdapter<string> dataAdapter = new AutoCompleteTextViewAdapter<string>(Context,
                                                                                                      Resource.Layout.RSAutoCompleteListItem,
                                                                                                      Resource.Id.auto_complete_textView, arrayList.ToArray(),
                                                                                                      rSSearchView);
            
            int searchTextId = this.Control.Resources.GetIdentifier("android:id/search_src_text", null, null);
            searchBox = ((AutoCompleteTextView)this.Control.FindViewById(searchTextId));
            searchBox.Adapter = dataAdapter;
            searchBox.OnItemClickListener = this;
            searchBox.Threshold = 1; //Start searching after how many character typed
            searchBox.ImeOptions = (global::Android.Views.InputMethods.ImeAction)global::Android.Views.InputMethods.ImeFlags.NoExtractUi | global::Android.Views.InputMethods.ImeAction.Search;


            //Eliminate extra space on left and right sides
            int searchEditFrameId = this.Control.Resources.GetIdentifier("android:id/search_edit_frame", null, null);
            LinearLayout searchEditFrame = (LinearLayout)this.Control.FindViewById(searchEditFrameId);
            LinearLayout.LayoutParams p = (LinearLayout.LayoutParams)searchEditFrame.LayoutParameters;
            p.LeftMargin = 0;
            p.RightMargin = 0;


            // Draw border for simple SearchView
            //if(rSSearchView.HasBorder)
            //{
            //    int searchPlateId = this.Control.Resources.GetIdentifier("android:id/search_plate", null, null);
            //    var searchPlate = this.Control.FindViewById(searchPlateId);
            //    searchPlate.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            //    this.Control.SetBackgroundResource(Resource.Drawable.RSRectangleBorderShape);
            //    //Extensions.ViewExtensions.DrawBorder(this.Control, Context, global::Android.Graphics.Color.Black);
            //}


            SetText(rSSearchView);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Text")
            {
                if (this.Control.Query == "")
                    rSSearchView.SelectedItem = null;
            }
            //else if (rSSearchView.HasBorder && e.PropertyName == "IsFocused")
            //{
            //    if (this.Element.IsFocused)
            //        this.Control.SetBackgroundResource(Resource.Drawable.RSRectangleBorderShapeFocused);
            //    else
            //        this.Control.SetBackgroundResource(Resource.Drawable.RSRectangleBorderShape);
            //}
        }

        private void SetText(RSSearchView rSSearchView)
        {
            if (rSSearchView.SelectedItem != null)
            {
                if (!string.IsNullOrEmpty(rSSearchView.DisplayMemberPath))
                    rSSearchView.Text = Helpers.TypeExtensions.GetPropValue(rSSearchView.SelectedItem, rSSearchView.DisplayMemberPath).ToString();
                else
                    rSSearchView.Text = rSSearchView.SelectedItem.ToString();
            }
            else
            {
                rSSearchView.Text = "";
                //if (!isTextInputLayout)
                //{
                //    this.Control.Hint = this.Element.Placeholder;
                //    this.Control.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
                //}
            }
        }

        public void OnItemClick(global::Android.Widget.AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            searchBox.Text = parent.GetItemAtPosition(position).ToString();
            int originalIndex = Array.IndexOf(arrayList.ToArray(), searchBox.Text);
            rSSearchView.SelectedItem = objectList[originalIndex];
            rSSearchView.Unfocus();
        }

        protected override global::Android.Widget.SearchView CreateNativeControl()
        {
            return base.CreateNativeControl();
        }
    }

    public class AutoCompleteTextViewAdapter<T> : global::Android.Widget.ArrayAdapter<T>
    {
        private T[] objects;
        private RSSearchView rSSearchView;

        public AutoCompleteTextViewAdapter(Context context, int resource, int textViewResourceId, T[] objects, RSSearchView rSSearchView) : base(context, resource, textViewResourceId, objects)
        {
            this.objects = objects;
            this.rSSearchView = rSSearchView;
        }

        //public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
        //{
        //    if(convertView == null)
        //        convertView = LayoutInflater.From(Context).Inflate(Resource.Layout.RSAutoCompleteListItem, null);

        //    TextView textView = convertView.FindViewById(Resource.Id.auto_complete_textView) as TextView;

        //    if (!string.IsNullOrEmpty(this.rSSearchView.DisplayMemberPath))
        //        textView.Text = Helpers.TypeExtensions.GetPropValue(this.objects[position], this.rSSearchView.DisplayMemberPath).ToString();
        //    else
        //        textView.Text = this.objects[position].ToString();

        //    return convertView;
        //}
    }
}