using Android.Content;
using Android.Database;
using Android.Graphics.Drawables;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSPickerBase), typeof(RSPickerRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSPickerRenderer : ViewRenderer<RSPickerBase, AppCompatEditText>
    {
        private AlertDialog alertDialog;
        private Xamarin.Forms.ListView listView;
        private bool isTextInputLayout;

        public RSPickerRenderer(Context context) : base(context)
        {
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
            if (Element is RSPicker)
            {
                if (Element.SelectedItem != null)
                {
                    if (!string.IsNullOrEmpty((Element as RSPicker).DisplayMemberPath))
                        this.Control.Text = Helpers.TypeExtensions.GetPropValue(Element.SelectedItem, (Element as RSPicker).DisplayMemberPath).ToString();
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
                if (Element.SelectedItem != null)
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

            if (Element.ItemTemplate != null)
            {
                listView = new Xamarin.Forms.ListView(ListViewCachingStrategy.RecycleElement);
                //listView.SeparatorVisibility = SeparatorVisibility.None;
                listView.ItemSelected += ListView_ItemSelected;
                listView.ItemsSource = Element.ItemsSource;
                var template = Element.ItemTemplate;
                var dataTemplate = new DataTemplate(() =>
                {
                    var view = template.CreateContent() as Xamarin.Forms.View;

                    return new ViewCell { View = view };
                });

                listView.ItemTemplate = dataTemplate;

                //Convert Xamarin forms view to android view
                var convertedList = Extensions.ViewExtensions.ConvertFormsToNative(listView, new Rectangle(), Context);
                
                dialog.SetView(convertedList);
            }
            else
            {
                dialog.SetSingleChoiceItems(Element.Items.ToArray(), Element.SelectedIndex, Selection);
                dialog.SetTitle(Element.Title);
            }

            dialog.SetPositiveButton("Done", (senderAlert, args) =>
            {
            });
            dialog.SetNegativeButton("Clear Item", (senderAlert, args) =>
            {
                Element.SelectedItem = null;
                Element.SelectedIndex = -1;
            });

            alertDialog = dialog.Show();
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Element.SelectedItem = e.SelectedItem;
            alertDialog.Dismiss();
        }

        private void Selection(object sender, DialogClickEventArgs e)
        {
            Element.SelectedItem = Element.ItemsSource[e.Which];
            alertDialog.Dismiss();
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

            if (e.PropertyName == "SelectedItem")
            {
                SetText();
            }

            if (e.PropertyName == "Error" && !isTextInputLayout)
                this.Control.Error = (this.Element as RSPickerBase).Error;
        }
    }
}