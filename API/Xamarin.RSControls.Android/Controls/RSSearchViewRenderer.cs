using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSSearchView), typeof(RSSearchViewRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSSearchViewRenderer : SearchBarRenderer, global::Android.Widget.AdapterView.IOnItemClickListener
    {
        global::Android.Support.V7.Widget.SearchView.SearchAutoComplete searchAutoComplete;
        AutoCompleteTextView searchBox;

        public RSSearchViewRenderer(Context context) : base(context)
        {
        }

        //protected override void OnElementChanged(ElementChangedEventArgs<RSSearchView> e)
        //{
        //    base.OnElementChanged(e);

        //    List<string> list = new List<string>(){ "Emmanuel", "Olayemi", "Henrry", "Mark", "Steve", "Ayomide", "David", "Anthony", "Adekola", "Adenuga" };
        //    AutoCompleteTextViewAdapter<string> dataAdapter = new AutoCompleteTextViewAdapter<string>(Context, Resource.Layout.RSAutoCompleteListItem, Resource.Id.auto_complete_textView, list.ToArray());


        //    AppCompatAutoCompleteTextView appCompatMultiAutoCompleteTextView = new AppCompatAutoCompleteTextView(Context);
        //    appCompatMultiAutoCompleteTextView.Adapter = dataAdapter;


        //    SearchView searchView = new SearchView(Context);
        //    //searchView.SetIconifiedByDefault(false); //Force view to show hint at start and no just search icon
        //    searchAutoComplete = (SearchView.SearchAutoComplete)searchView.FindViewById(Resource.Id.search_src_text);
        //    //appCompatMultiAutoCompleteTextView.Hint = this.Element.Placeholder;
        //    //appCompatMultiAutoCompleteTextView.SetHintTextColor(this.Element.PlaceholderColor.ToAndroid());
        //    searchAutoComplete.Adapter = dataAdapter;
        //    searchAutoComplete.OnItemClickListener = this;
        //    this.SetNativeControl(searchView);

        //}

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            List<string> list = new List<string>() { "Emmanuel", "Olayemi", "Henrry", "Mark", "Steve", "Ayomide", "David", "Anthony", "Adekola", "Adenuga", "Adffrff", "Adrthrzhh", "Adrthz" };
            AutoCompleteTextViewAdapter<string> dataAdapter = new AutoCompleteTextViewAdapter<string>(Context, Resource.Layout.RSAutoCompleteListItem, Resource.Id.auto_complete_textView, list.ToArray());

            int searchTextId = this.Control.Resources.GetIdentifier("android:id/search_src_text", null, null);
            searchBox = ((AutoCompleteTextView)this.Control.FindViewById(searchTextId));
            searchBox.ImeOptions = (global::Android.Views.InputMethods.ImeAction)global::Android.Views.InputMethods.ImeFlags.NoExtractUi;
            searchBox.Adapter = dataAdapter;
            searchBox.OnItemClickListener = this;
        }

        public void OnItemClick(global::Android.Widget.AdapterView parent, global::Android.Views.View view, int position, long id)
        {
            searchBox.Text = parent.GetItemAtPosition(position).ToString();
            //searchAutoComplete.Text = parent.GetItemAtPosition(position).ToString();
        }
    }

    public class AutoCompleteTextViewAdapter<T> : global::Android.Widget.ArrayAdapter<T>
    {
        private T[] objects;

        public AutoCompleteTextViewAdapter(Context context, int resource, int textViewResourceId, T[] objects) : base(context, resource, textViewResourceId, objects)
        {
            this.objects = objects;
        }
    }
}