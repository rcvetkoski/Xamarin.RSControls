using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSPickerBase), typeof(RSPickerRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPickerRenderer : ViewRenderer<RSPickerBase, UITextField>
    {
        private UIPickerView uIPickerView;
        private UITableView multipleSelectionList;
        CustomUITableView customTable;
        public RSPopupRenderer rSPopup;
        public bool canUpdate = true;

        public RSPickerRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSPickerBase> e)
        {
            base.OnElementChanged(e);


            if (Control != null)
                return;

            if (e.NewElement == null)
                return;


            //Create uitextfield and set it as control
            var entry = CreateNativeControl();
            entry.Font = UIFont.SystemFontOfSize((nfloat)this.Element.FontSize);
            this.SetNativeControl(entry);


            //Set uitextfield style
            //this.Control.BorderStyle = UITextBorderStyle.RoundedRect;
            //Control.Layer.BorderColor = UIColor.LightGray.CGColor;
            //Control.Font = UIFont.SystemFontOfSize((nfloat)this.Element.FontSize);


            //Set Text
            SetText();


            if (this.Element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
            {
                entry.InputAccessoryView = CreateToolbar(entry);
                entry.InputAssistantItem.LeadingBarButtonGroups = new UIBarButtonItemGroup[0];
                entry.InputAssistantItem.TrailingBarButtonGroups = new UIBarButtonItemGroup[0];


                if (this.Element.SelectionMode == PickerSelectionModeEnum.Single)
                    entry.InputView = CreateUIPickerView();
                else
                    entry.InputView = CreateMultipleSelectionPicker();
            }
            else
                entry.InputView = new UIView(); // Hide original keyboard




            entry.EditingDidBegin += Entry_EditingDidBegin;

            //Delete placeholder as we use floating hint instead
            Element.Placeholder = "";
        }

        //If collection has changed meanwhile update the data on click
        private void Entry_EditingDidBegin(object sender, EventArgs e)
        {
            if(canUpdate)
            {
                if (this.Element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
                {
                    if (uIPickerView != null)
                        uIPickerView.Model = new CustomUIPickerViewModel(this.Element.ItemsSource, this.Element, this);

                    if (multipleSelectionList != null)
                    {
                        multipleSelectionList.Source = new CustomTableSource(this.Element.Items.ToArray(), this);
                    }
                }
                else
                    CreateCustomUiPickerPopup();
            }

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "SelectedItem" || e.PropertyName == "SelectedItems")
            {
                SetText();

                (this.Control as RSUITextField).UpdateFloatingLabel();
            }
            else if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = this.Element.Error;
            }
        }

        public void SetText()
        {
            if (this.Element is RSPicker)
            {
                if (this.Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
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
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.Element.SelectedItems != null && this.Element.SelectedItems.Count >= 1)
                    {
                        foreach (object item in this.Element.SelectedItems)
                        {
                            if (!string.IsNullOrEmpty((this.Element as RSPicker).DisplayMemberPath))
                                this.Control.Text += Helpers.TypeExtensions.GetPropValue(item, (this.Element as RSPicker).DisplayMemberPath).ToString();
                            else
                                this.Control.Text += item.ToString();

                            if (this.Element.SelectedItems.IndexOf(item) < this.Element.SelectedItems.Count - 1)
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
                if (this.Element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (this.Element.SelectedItem != null)
                        this.Control.Text = Element.SelectedItem.ToString();
                    else
                    {
                        this.Control.Text = "";
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (this.Element.SelectedItems != null && this.Element.SelectedItems.Count >= 1)
                    {
                        foreach (object item in this.Element.SelectedItems)
                        {
                            this.Control.Text += item.ToString();

                            if (this.Element.SelectedItems.IndexOf(item) < this.Element.SelectedItems.Count - 1)
                                this.Control.Text += ", ";
                        }
                    }
                    else
                    {
                        this.Control.Text = "";
                    }
                }
            }
        }

        private UIToolbar CreateToolbar(UITextField entry)
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            UIToolbar toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
            //UIToolbar toolbar = new UIToolbar() { BarStyle = UIBarStyle.Default, Translucent = true };
            toolbar.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

            //Clear cancel button
            UIBarButtonItem clearCancelButton;

            clearCancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (sender, ev) =>
            {
                if (this.Element.SelectionMode == PickerSelectionModeEnum.Single)
                    this.Element.SelectedItem = null;
                else
                {
                    this.Element.SelectedItems.Clear();
                    multipleSelectionList.ReloadData();
                }

                SetText();
                entry.EndEditing(true);
                //entry.ResignFirstResponder();
            });


            //Space
            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

            //Done Button
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, ev) =>
            {
                if (this.Element.SelectionMode == PickerSelectionModeEnum.Single)
                    this.Element.SelectedItem = (uIPickerView.Model as CustomUIPickerViewModel).SelectedItem;

                SetText();
                entry.ResignFirstResponder();
            });

            toolbar.SetItems(new[] { clearCancelButton, spacer, doneButton }, false);

            return toolbar;
        }

        private UIPickerView CreateUIPickerView()
        {
            uIPickerView = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight,
                Model = new CustomUIPickerViewModel(this.Element.ItemsSource, this.Element, this),
            };

            //Fixes selectedindicator not showing
            if (this.Element.SelectedItem != null)
                uIPickerView.Select(this.Element.SelectedIndex, 0, true);

            return uIPickerView;
        }

        private UITableView CreateMultipleSelectionPicker()
        {
            multipleSelectionList = new UITableView();
            multipleSelectionList.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            multipleSelectionList.BackgroundColor = UIColor.Clear;


            UIBlurEffect uIBlurEffect = new UIBlurEffect();
            UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
            UIVisualEffectView uIVisualEffectView = new UIVisualEffectView(uIBlurEffect);
            uIVisualEffectView.TranslatesAutoresizingMaskIntoConstraints = false;
            multipleSelectionList.AddSubview(uIVisualEffectView);

            multipleSelectionList.AllowsMultipleSelection = true;
            multipleSelectionList.SetEditing(true, true);
            multipleSelectionList.AllowsMultipleSelectionDuringEditing = true;
            multipleSelectionList.Source = new CustomTableSource(this.Element.Items.ToArray(), this);


            return multipleSelectionList;
        }

        private UIView CreateCustomUiPickerPopup()
        {
            customTable = new CustomUITableView();
            customTable.Source = new CustomTableSource(this.Element.Items.ToArray(), this);

            //Scroll to selected item if single selection mode
            if(this.Element.SelectedItem != null)
            {
                var selectedIndex = this.Element.ItemsSource.IndexOf(this.Element.SelectedItem);
                customTable.ScrollToRow(NSIndexPath.FromItemSection(selectedIndex, 0), UITableViewScrollPosition.Top, false);
            }

            customTable.AllowsMultipleSelection = true;
            customTable.SetEditing(true, true);
            customTable.AllowsMultipleSelectionDuringEditing = true;

            customTable.TableFooterView = new UIView(); //Hide empty separators

            //Holder
            UIStackView holder = new UIStackView();
            holder.Axis = UILayoutConstraintAxis.Vertical;

            //Search Bar

            if(this.Element.IsSearchEnabled)
            {
                UISearchBar searchBar = new UISearchBar();
                searchBar.SizeToFit();
                searchBar.SearchButtonClicked += (sender, e) =>
                {
                    //this is the method that is called when the user clicks on search button and its here to close keyboard  
                    searchBar.ResignFirstResponder();
                };
                searchBar.AutocorrectionType = UITextAutocorrectionType.No;
                searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
                searchBar.SearchBarStyle = UISearchBarStyle.Minimal;
                searchBar.Placeholder = "Search";
                searchBar.TextChanged += (sender, e) =>
                {
                    //this is the method that is called when the user searches  
                    searchTable(customTable, customTable.Source as CustomTableSource, searchBar);
                };
                holder.AddArrangedSubview(searchBar);
            }


            holder.AddArrangedSubview(customTable);

            //Create RSPopup
            rSPopup = new RSPopupRenderer();
            //rSPopup.Title = "Custom Picker";
            //rSPopup.Message = "Message";
            rSPopup.Width = -1;
            rSPopup.Height = -2;
            rSPopup.TopMargin = 40;
            rSPopup.BottomMargin = 40;
            rSPopup.LeftMargin = 20;
            rSPopup.RightMargin = 20;
            rSPopup.BorderRadius = 16;
            rSPopup.BorderFillColor = UIColor.White.ToColor();
            rSPopup.DimAmount = 0.6f;
            rSPopup.AddAction("Done", RSPopupButtonTypeEnum.Positive, new Command( () => { this.Control.ResignFirstResponder(); } ));
            rSPopup.AddAction("Clear", RSPopupButtonTypeEnum.Destructive, new Command( () => { clearPicker(); } ));
            rSPopup.SetNativeView(holder);
            rSPopup.ShowPopup();


            return rSPopup;
        }

        private void clearPicker()
        {
            if (this.Element.SelectionMode == PickerSelectionModeEnum.Single)
                this.Element.SelectedItem = null;
            else
            {
                this.Element.SelectedItems.Clear();
                customTable.ReloadData();
            }

            SetText();
            this.Control.EndEditing(true);
            rSPopup.Dismiss(true);
        }

        private void searchTable(UITableView table, CustomTableSource tableSource, UISearchBar searchBar)
        {
            //perform the search, and refresh the table with the results  
            tableSource.PerformSearch(searchBar.Text);
            table.ReloadData();
        }

        protected override UITextField CreateNativeControl()
        {
            return new RSUITextField(this.Element as IRSControl);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.Control != null)
                    this.Control.EditingDidBegin -= Entry_EditingDidBegin;
            }
        }
    }


    //Native picker
    public class CustomUIPickerViewModel : UIPickerViewModel
    {
        private IList<object> myItems;
        protected int selectedIndex = 0;
        private RSPickerBase rsPicker;
        private RSPickerRenderer rSPickerRenderer;

        public CustomUIPickerViewModel(System.Collections.IEnumerable items, RSPickerBase rsPicker, RSPickerRenderer rSPickerRenderer)
        {
            this.rSPickerRenderer = rSPickerRenderer;

            if (this.rSPickerRenderer.Element.ItemsSource == null)
                return;

            this.rsPicker = rsPicker;

            if (myItems == null)
                myItems = new List<object>();

            myItems.Clear();

            foreach (object item in items)
                myItems.Add(item);
        }

        public object SelectedItem
        {
            get { return myItems[selectedIndex]; }
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return myItems.Count;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            if ((this.rsPicker != null && rsPicker is RSPicker) && !string.IsNullOrEmpty((rsPicker as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(myItems[(int)row], (rsPicker as RSPicker).DisplayMemberPath).ToString();
            else
                return this.myItems[(int)row].ToString();
        }

        //View reuse is buged IOS bug
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            if (rsPicker.ItemTemplate == null)
            {
                UILabel label = new UILabel(new CGRect(0.0f, 0.0f, pickerView.RowSizeForComponent(component).Width, pickerView.RowSizeForComponent(component).Height));
                label.TextAlignment = UITextAlignment.Center;
                label.Text = GetTitle(pickerView, row, component);
                label.Font = UIFont.SystemFontOfSize(22);

                return label;
            }
            else
            {
                Xamarin.Forms.View formsView = rsPicker.ItemTemplate.CreateContent() as Xamarin.Forms.View;
                formsView.BindingContext = myItems[(int)row];
                var renderer = Platform.CreateRenderer(formsView);
                renderer.NativeView.Frame = new CGRect(0, 0, pickerView.RowSizeForComponent(component).Width, pickerView.RowSizeForComponent(component).Height);
                renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
                renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;
                renderer.Element.Layout(new CGRect(0, 0, pickerView.RowSizeForComponent(component).Width, pickerView.RowSizeForComponent(component).Height).ToRectangle());
                var nativeView = renderer.NativeView;
                nativeView.SetNeedsLayout();

                return nativeView;
            }
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            selectedIndex = (int)row;
            pickerView.Select(row, component, false);
            rsPicker.SelectedItem = this.SelectedItem;
            this.rSPickerRenderer.SetText();
        }
    }
    public class CustomCell : UITableViewCell
    {
        public Forms.View formsView;
        private IVisualElementRenderer renderer;

        public CustomCell(NSString cellId, Forms.View formsView) : base(UITableViewCellStyle.Default, cellId)
        {
            this.formsView = formsView;

            this.ContentView.AddSubview(FormsToNativeView());

            //Set new selected background so it wont higlight background but only tick left checkbox
            this.SelectedBackgroundView = new UIView(this.Frame);
            this.SelectedBackgroundView.BackgroundColor = UIColor.Clear;
        }

        private UIView FormsToNativeView()
        {
            renderer = Platform.CreateRenderer(formsView);
            this.ContentView.AddSubview(renderer.NativeView);

            return renderer.NativeView;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            //nativeView.Frame = new CGRect(0, 0, this.ContentView.Frame.Width, this.ContentView.Frame.Height);
            //nativeView.AutoresizingMask = UIViewAutoresizing.All;
            //nativeView.ContentMode = UIViewContentMode.ScaleToFill;
            renderer.Element.Layout(new CGRect(0, 0, this.ContentView.Frame.Width, this.ContentView.Frame.Height).ToRectangle());
        }
    }




    public class CustomUITableView : UITableView
    {
        public CustomUITableView() : base()
        {
        }

        public override CGSize IntrinsicContentSize 
        {
            get
            {
                return new CGSize(ContentSize.Width, ContentSize.Height);
            }
        }

        public override void ReloadData()
        {
            base.ReloadData();
        }
    }
    public class CustomTableSource : UITableViewSource
    {
        private List<object> list;
        private List<object> searchItems;
        private RSPickerRenderer rsPicker;


        public CustomTableSource(object[] list, RSPickerRenderer rsPicker)
        {
            this.list = rsPicker.Element.ItemsSource as List<object>;
            this.searchItems = rsPicker.Element.ItemsSource as List<object>; 
            this.rsPicker = rsPicker;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // iOS will create a cell automatically if one isn't available in the reuse pool
            UITableViewCell cell = tableView.DequeueReusableCell("MyCellId");

            if(cell == null)
                cell = new UITableViewCell(UITableViewCellStyle.Default, "MyCellId");


            var item = GetItemValue(indexPath.Row);
            if(item != null)
                cell.TextLabel.Text = item;



            //Set selected row
            if(rsPicker.Element.SelectionMode == PickerSelectionModeEnum.Multiple)
            {
                if (rsPicker.Element.SelectedItems.Contains(searchItems[indexPath.Row]))
                {
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }
            }
            else
            {
                if(rsPicker.Element.SelectedItem != null && rsPicker.Element.SelectedItem == searchItems[indexPath.Row])
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
            }


            //Set transparent background to cell
            UIView clearBackgroundView = new UIView();
            clearBackgroundView.BackgroundColor = UIColor.Clear;
            cell.SelectedBackgroundView = clearBackgroundView;

            if(rsPicker.Element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
                cell.BackgroundColor = UIColor.Clear;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var lolool = this.searchItems.Count();
            return lolool;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (!rsPicker.Control.IsFirstResponder)
            {
                rsPicker.canUpdate = false;
                rsPicker.Control.BecomeFirstResponder();
            }

            if (rsPicker.Element.SelectionMode == PickerSelectionModeEnum.Multiple)
            {
                rsPicker.Element.SelectedItems.Add(searchItems[indexPath.Row]);
            }
            else
            {
                rsPicker.Element.SelectedItem = searchItems[indexPath.Row];
                rsPicker.rSPopup.Dismiss(true);
                rsPicker.Control.ResignFirstResponder();
            }

            rsPicker.canUpdate = true;
            rsPicker.SetText();
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            if (!rsPicker.Control.IsFirstResponder)
            {
                rsPicker.canUpdate = false;
                rsPicker.Control.BecomeFirstResponder();
            }

            rsPicker.Element.SelectedItems.Remove(searchItems[indexPath.Row]);

            rsPicker.canUpdate = true;
            rsPicker.SetText();
        }


        public void PerformSearch(string searchText)
        {
            searchText = searchText.ToLower();
            this.searchItems = list.Where(x => GetItemValue2(x).ToString().ToLower().Contains(searchText)).ToList();
        }


        public string GetItemValue(int row)
        {
            if ((this.rsPicker != null && rsPicker.Element is RSPicker) && !string.IsNullOrEmpty((rsPicker.Element as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(searchItems[(int)row], (rsPicker.Element as RSPicker).DisplayMemberPath).ToString();
            else
                return searchItems[(int)row].ToString();
        }

        public string GetItemValue2(object item)
        {
            if ((this.rsPicker != null && rsPicker.Element is RSPicker) && !string.IsNullOrEmpty((rsPicker.Element as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(item, (rsPicker.Element as RSPicker).DisplayMemberPath).ToString();
            else
                return item.ToString();
        }
    }
}
