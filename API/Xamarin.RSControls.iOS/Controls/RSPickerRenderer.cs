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
    public class RSPickerRenderer : PickerRenderer
    {
        private UIPickerView uIPickerView;
        private UITableView multipleSelectionList;
        CustomUITableView customTable;
        public RSPopupRenderer rSPopup;
        public bool canUpdate = true;
        private RSPickerBase element;


        public RSPickerRenderer()
        {
        }

        protected override UITextField CreateNativeControl()
        {
            if (Element != null)
                element = Element as RSPickerBase;

            if ((this.Element as IRSControl).RightIcon == null)
                (this.Element as IRSControl).RightIcon = new Helpers.RSEntryIcon()
                {
                    View = new RSSvgImage() { Source = "Xamarin.RSControls/Data/SVG/arrow.svg" }
                };

            return new RSUITextField(element as IRSControl);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;


            //Create uitextfield and set it as control
            Control.Font = UIFont.SystemFontOfSize((nfloat)element.FontSize);
            Control.AutocorrectionType = UITextAutocorrectionType.No;
            Control.TintColor = UIColor.Clear;
            Control.SpellCheckingType = UITextSpellCheckingType.No;

            //Set Text
            SetText();

            if (element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
            {
                Control.InputAccessoryView = CreateToolbar(Control);
                Control.InputAssistantItem.LeadingBarButtonGroups = new UIBarButtonItemGroup[0];
                Control.InputAssistantItem.TrailingBarButtonGroups = new UIBarButtonItemGroup[0];


                if (element.SelectionMode == PickerSelectionModeEnum.Single)
                    Control.InputView = CreateUIPickerView();
                else
                    Control.InputView = CreateMultipleSelectionPicker();
            }
            else
            {
                Control.InputView = new UIView(); // Hide original keyboard
                Control.InputAccessoryView = new UIView();
            }


            //Hides shortcut bar
            Control.InputAssistantItem.LeadingBarButtonGroups = null;
            Control.InputAssistantItem.TrailingBarButtonGroups = null;

            Control.EditingDidBegin += Entry_EditingDidBegin;

            //Delete placeholder as we use floating hint instead
            //Element.Placeholder = "";
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "SelectedItem" || e.PropertyName == "SelectedItems")
            {
                if (!(sender as Forms.View).IsFocused)
                {
                    SetText();
                    (this.Control as RSUITextField).UpdateView();
                }
            }
            else if (e.PropertyName == "Error")
            {
                (this.Control as RSUITextField).ErrorMessage = element.Error;
            }
        }

        //If collection has changed meanwhile update the data on click
        private void Entry_EditingDidBegin(object sender, EventArgs e)
        {
            if (canUpdate)
            {
                if (element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
                {
                    if (uIPickerView != null)
                        uIPickerView.Model = new CustomUIPickerViewModel(element.ItemsSource, element, this);

                    if (multipleSelectionList != null)
                    {
                        multipleSelectionList.Source = new CustomTableSource(element.Items.ToArray(), this, element);
                    }
                }
                else
                    CreateCustomUiPickerPopup();
            }
        }

        public void SetText()
        {
            if (element is RSPicker)
            {
                if (element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (element.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty((element as RSPicker).DisplayMemberPath))
                            this.Control.Text = Helpers.TypeExtensions.GetPropValue(element.SelectedItem, (Element as RSPicker).DisplayMemberPath);
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

                    if (element.SelectedItems != null && element.SelectedItems.Count >= 1)
                    {
                        foreach (object item in element.SelectedItems)
                        {
                            if (!string.IsNullOrEmpty((element as RSPicker).DisplayMemberPath))
                                this.Control.Text += Helpers.TypeExtensions.GetPropValue(item, (element as RSPicker).DisplayMemberPath);
                            else
                                this.Control.Text += item.ToString();

                            if (element.SelectedItems.IndexOf(item) < element.SelectedItems.Count - 1)
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
                if (element.SelectionMode == Enums.PickerSelectionModeEnum.Single)
                {
                    if (element.SelectedItem != null)
                        this.Control.Text = Element.SelectedItem.ToString();
                    else
                    {
                        this.Control.Text = "";
                    }
                }
                else
                {
                    this.Control.Text = "";

                    if (element.SelectedItems != null && element.SelectedItems.Count >= 1)
                    {
                        foreach (object item in element.SelectedItems)
                        {
                            this.Control.Text += item.ToString();

                            if (element.SelectedItems.IndexOf(item) < element.SelectedItems.Count - 1)
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

        private UIToolbar CreateToolbar(UITextField Control)
        {
            var width = UIScreen.MainScreen.Bounds.Width;
            UIToolbar toolbar = new UIToolbar(new CGRect(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
            //UIToolbar toolbar = new UIToolbar() { BarStyle = UIBarStyle.Default, Translucent = true };
            toolbar.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

            //Clear cancel button
            UIBarButtonItem clearCancelButton;

            clearCancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (sender, ev) =>
            {
                if (element.SelectionMode == PickerSelectionModeEnum.Single)
                    element.SelectedItem = null;
                else
                {
                    element.SelectedItems.Clear();
                    multipleSelectionList.ReloadData();
                }

                SetText();
                Control.EndEditing(true);
                //Control.ResignFirstResponder();
            });


            //Space
            var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

            //Done Button
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (sender, ev) =>
            {
                if (element.SelectionMode == PickerSelectionModeEnum.Single)
                    element.SelectedItem = (uIPickerView.Model as CustomUIPickerViewModel).SelectedItem;

                SetText();
                Control.ResignFirstResponder();
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
                Model = new CustomUIPickerViewModel(element.ItemsSource, element, this),
            };

            //Fixes selectedindicator not showing
            if (element.SelectedItem != null)
                uIPickerView.Select(element.SelectedIndex, 0, true);

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
            multipleSelectionList.Source = new CustomTableSource(element.Items.ToArray(), this, element);
            multipleSelectionList.RegisterClassForCellReuse(typeof(CustomCell), "MyCellId");

            return multipleSelectionList;
        }

        private UIView CreateCustomUiPickerPopup()
        {
            customTable = new CustomUITableView();
            customTable.Source = new CustomTableSource(element.Items.ToArray(), this, element);
            customTable.BackgroundColor = UIColor.Clear;

            if(element.SelectionMode == PickerSelectionModeEnum.Single)
            {
                //Separator visibility
                if (element.RsPopupSeparatorsUserSet)
                {
                    if (!element.RsPopupSeparatorsEnabled)
                        customTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
                    else
                        customTable.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
                }
                else
                    customTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }


            //Scroll to selected item if single selection mode
            if (element.SelectedItem != null)
            {
                var selectedIndex = element.ItemsSource.IndexOf(element.SelectedItem);
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

            if (element.IsSearchEnabled)
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
                searchBar.InputAccessoryView = null;
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
            rSPopup.Title = element.RSPopupTitle;
            rSPopup.Message = element.RSPopupMessage;
            rSPopup.Width = -1;
            rSPopup.Height = -2;
            rSPopup.TopMargin = 50;
            rSPopup.BottomMargin = 50;
            rSPopup.LeftMargin = 20;
            rSPopup.RightMargin = 20;
            rSPopup.BorderRadius = 16;
            rSPopup.BorderFillColor = element.RSPopupBackgroundColor;
            rSPopup.DimAmount = 0.8f;
            rSPopup.AddAction("Done", RSPopupButtonTypeEnum.Positive, null);
            rSPopup.AddAction("Clear", RSPopupButtonTypeEnum.Destructive, new Command(() => { clearPicker(); }));
            rSPopup.SetNativeView(holder);
            rSPopup.ShowPopup();

            rSPopup.DismissEvent += RSPopup_OnDismiss;

            return rSPopup;
        }

        private void RSPopup_OnDismiss(object sender, EventArgs e)
        {
            Control.ResignFirstResponder();
        }

        private void clearPicker()
        {
            if (element.SelectionMode == PickerSelectionModeEnum.Single)
                element.SelectedItem = null;
            else
            {
                element.SelectedItems.Clear();
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


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.Control != null)
                    this.Control.EditingDidBegin -= Entry_EditingDidBegin;

                if(rSPopup != null)
                    rSPopup.DismissEvent -= RSPopup_OnDismiss;
            }

            if (element != null && element is RSPicker)
                (element as RSPicker).Dispose();
        }
    }


    //Native picker
    public class CustomUIPickerViewModel : UIPickerViewModel
    {
        private IList<object> myItems;
        protected int selectedIndex = 0;
        private RSPickerBase rsPicker;
        private RSPickerRenderer rSPickerRenderer;
        private Xamarin.Forms.View formsView;

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
            get { if (myItems != null && myItems.Any()) return myItems[selectedIndex]; else return null; }
        }

        public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
        {
            if (formsView != null)
            {
                var sizeRequest = formsView.Measure(double.PositiveInfinity, nfloat.PositiveInfinity, MeasureFlags.IncludeMargins);
                if (sizeRequest.Request.Height > 32)
                    return (nfloat)sizeRequest.Request.Height;
                else
                    return 32;
            }
            else
                return 32;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            if (myItems != null)
                return myItems.Count;
            else
                return 0;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            if ((this.rsPicker != null && rsPicker is RSPicker) && !string.IsNullOrEmpty((rsPicker as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(myItems[(int)row], (rsPicker as RSPicker).DisplayMemberPath);
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
                formsView = rsPicker.ItemTemplate.CreateContent() as Xamarin.Forms.View;
                formsView.BindingContext = myItems[(int)row];
                var nativeView = Extensions.ViewExtensions.ConvertFormsToNative(formsView, 0, 0, pickerView.RowSizeForComponent(component).Width, 0);

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
        private UIView nativeView;
        public bool IsInit;
        private bool IsCustomTemplate;

        public CustomCell(IntPtr handle) : base(handle)
        {
        }

        public CustomCell(NSString cellId, Forms.View formsView) : base(UITableViewCellStyle.Default, cellId)
        {            
            this.formsView = formsView;
            UIStackView holder = new UIStackView();
            holder.Axis = UILayoutConstraintAxis.Vertical;
            renderer = Platform.CreateRenderer(formsView);
            var convertView = new Extensions.FormsToiosCustomDialogView(formsView, renderer, this);
            var sizeRequest = formsView.Measure(this.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
            var ccc = Extensions.ViewExtensions.ConvertFormsToNative(formsView, new CGRect(formsView.X, formsView.Y, this.Frame.Width, this.Frame.Height));
            //holder.AddArrangedSubview(ccc);


            this.ContentView.AddSubview(ccc);
            ccc.TranslatesAutoresizingMaskIntoConstraints = false;
            if (sizeRequest.Request.Height > this.Frame.Height)
                ccc.HeightAnchor.ConstraintEqualTo((nfloat)sizeRequest.Request.Height).Active = true;
            ccc.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor).Active = true;
            ccc.BottomAnchor.ConstraintEqualTo(this.ContentView.BottomAnchor).Active = true;
            ccc.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor).Active = true;
            ccc.TrailingAnchor.ConstraintEqualTo(this.ContentView.TrailingAnchor).Active = true;

            //this.ContentView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;


            //this.ContentView.TranslatesAutoresizingMaskIntoConstraints = false;
            //label.TranslatesAutoresizingMaskIntoConstraints = false;
            //this.ContentView.TopAnchor.ConstraintEqualTo(label.TopAnchor).Active = true;
            //this.ContentView.LeadingAnchor.ConstraintEqualTo(label.LeadingAnchor).Active = true;
            //this.ContentView.BottomAnchor.ConstraintEqualTo(label.BottomAnchor).Active = true;

            //label.TopAnchor.ConstraintEqualTo(this.ContentView.TopAnchor).Active = true;
            //label.BottomAnchor.ConstraintEqualTo(this.ContentView.BottomAnchor).Active = true;
            //label.LeadingAnchor.ConstraintEqualTo(this.ContentView.LeadingAnchor).Active = true;
            //label.TrailingAnchor.ConstraintEqualTo(this.ContentView.TrailingAnchor).Active = true;


            //Set new selected background so it wont higlight background but only tick left checkbox
            this.SelectedBackgroundView = new UIView(this.Frame);
            this.SelectedBackgroundView.BackgroundColor = UIColor.Clear;
        }

        public void CellInit(Forms.View formsView, UITableView uITableView)
        {
            this.formsView = formsView;
            renderer = Platform.CreateRenderer(this.formsView);
            nativeView = Extensions.ViewExtensions.ConvertFormsToNative(this.formsView, this.formsView.X, this.formsView.Y, this.Frame.Width, 0);
            this.ContentView.AddSubview(nativeView);

            //Set new selected background so it wont higlight background but only tick left checkbox
            this.SelectedBackgroundView = new UIView(this.Frame);
            this.SelectedBackgroundView.BackgroundColor = UIColor.Clear;

            IsInit = true;
            IsCustomTemplate = true;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            //Update size
            if(IsCustomTemplate)
            {
                nativeView.Frame = new CGRect(20, 0, this.ContentView.Frame.Width - 20, this.ContentView.Frame.Height);
                nativeView.AutoresizingMask = UIViewAutoresizing.All;
                nativeView.ContentMode = UIViewContentMode.ScaleToFill;
                renderer.Element.Layout(new CGRect(20, 0, this.ContentView.Frame.Width - 20, this.ContentView.Frame.Height).ToRectangle());
            }
        }
    }
    public class CustomUITableView : UITableView
    {
        public CustomUITableView() : base()
        {
            this.RowHeight = UITableView.AutomaticDimension;
            this.EstimatedRowHeight = 44f;
            this.RegisterClassForCellReuse(typeof(CustomCell), "MyCellId");
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
        private RSPickerBase element;


        public CustomTableSource(object[] list, RSPickerRenderer rsPicker, RSPickerBase element)
        {
            searchItems = new List<object>();

            foreach (var item in element.ItemsSource)
            {
                searchItems.Add(item);
            }

            this.list = element.ItemsSource as List<object>;
            //this.searchItems = element.ItemsSource as List<object>; 
            this.rsPicker = rsPicker;
            this.element = element;
        }


        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // iOS will create a cell automatically if one isn't available in the reuse pool
            UITableViewCell cell = tableView.DequeueReusableCell("MyCellId", indexPath);


            //Create tamplate if ItemTemplate set
            if (element.ItemTemplate != null && !(cell as CustomCell).IsInit)
            {
                Forms.View formsView = element.ItemTemplate.CreateContent() as Forms.View;
                formsView.BindingContext = searchItems[indexPath.Row];
                (cell as CustomCell).CellInit(formsView, tableView);

                //Set proper height
                var sizeRequest = formsView.Measure(tableView.Frame.Width, nfloat.PositiveInfinity, MeasureFlags.IncludeMargins);
                if (sizeRequest.Request.Height > cell.Frame.Height)
                {
                    formsView.HeightRequest = sizeRequest.Request.Height;
                    var frame = cell.Frame;
                    frame.Height = (nfloat)sizeRequest.Request.Height;
                    cell.Frame = frame;
                    tableView.RowHeight = (nfloat)sizeRequest.Request.Height;
                }
                else
                {
                    formsView.HeightRequest = cell.Frame.Height;
                }
            }


            //Binding/Value update
            if (element.ItemTemplate == null)
            {
                //if (cell == null)
                //    cell = new UITableViewCell(UITableViewCellStyle.Default, "MyCellId");


                var item = GetItemValue(indexPath.Row);
                if (item != null)
                    cell.TextLabel.Text = item;
            }
            else
            {
                //if (cell == null)
                //{
                //    Forms.View formsView = element.ItemTemplate.CreateContent() as Forms.View;
                //    formsView.BindingContext = searchItems[indexPath.Row];
                //    cell = new CustomCell(new NSString("MyCellId"), formsView);
                //}

                //Update bindings
                (cell as CustomCell).formsView.BindingContext = searchItems[indexPath.Row];
            }



            //Set selected row
            if (element.SelectionMode == PickerSelectionModeEnum.Multiple)
            {
                if (element.SelectedItems.Contains(searchItems[indexPath.Row]))
                {
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }
            }
            else
            {
                if(element.SelectedItem != null && element.SelectedItem == searchItems[indexPath.Row])
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
            }


            //Set transparent background to cell
            UIView clearBackgroundView = new UIView();
            clearBackgroundView.BackgroundColor = UIColor.Clear;
            cell.SelectedBackgroundView = clearBackgroundView;

            //if(element.RSPopupStyleEnum == RSPopupStyleEnum.Native)
                cell.BackgroundColor = UIColor.Clear;


            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (searchItems != null)
                return this.searchItems.Count();
            else
                return 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (!rsPicker.Control.IsFirstResponder)
            {
                rsPicker.canUpdate = false;
                rsPicker.Control.BecomeFirstResponder();
            }

            if (element.SelectionMode == PickerSelectionModeEnum.Multiple)
            {
                element.SelectedItems.Add(searchItems[indexPath.Row]);
            }
            else
            {
                element.SelectedItem = searchItems[indexPath.Row];
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

            element.SelectedItems.Remove(searchItems[indexPath.Row]);

            rsPicker.canUpdate = true;
            rsPicker.SetText();
        }


        public void PerformSearch(string searchText)
        {
            if(list.Count > 0)
            {
                searchText = searchText.ToLower();
                this.searchItems = list.Where(x => GetItemValue2(x).ToLower().Contains(searchText)).ToList();
            }
        }


        public string GetItemValue(int row)
        {
            if ((this.rsPicker != null && element is RSPicker) && !string.IsNullOrEmpty((element as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(searchItems[(int)row], (element as RSPicker).DisplayMemberPath);
            else
                return searchItems[(int)row].ToString();
        }

        public string GetItemValue2(object item)
        {
            if ((this.rsPicker != null && element is RSPicker) && !string.IsNullOrEmpty((element as RSPicker).DisplayMemberPath))
                return Helpers.TypeExtensions.GetPropValue(item, (element as RSPicker).DisplayMemberPath);
            else
                return item.ToString();
        }
    }
}
