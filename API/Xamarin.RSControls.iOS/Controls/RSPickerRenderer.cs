﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSPickerBase), typeof(RSPickerRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSPickerRenderer : ViewRenderer<RSPickerBase, UITextField>
    {
        public RSPickerRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSPickerBase> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
                return;


            //Create uitextfield and set it as control
            var nativeEditText = new UITextField();
            this.SetNativeControl(nativeEditText);


            //Set uitextfield style
            this.Control.BorderStyle = UITextBorderStyle.RoundedRect;
            Control.Layer.BorderColor = UIColor.LightGray.CGColor;
            Control.Font = UIFont.SystemFontOfSize((nfloat)this.Element.FontSize);

            //Set icon
            SetIcon();

            //Set placeholder text
            SetPlaceHolderText();

            //Set Text
            SetText();

            //Create and add gesture to control
            UITapGestureRecognizer uIGesture = new UITapGestureRecognizer((obj) =>
            {
                CustomAlertView customAlertView = new CustomAlertView("", this);
                customAlertView.Show(true);
            });

            Control.AddGestureRecognizer(uIGesture);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "SelectedItem" || e.PropertyName == "SelectedItems")
            {
                SetText();
            }
            
            if(e.PropertyName == "SelectedIndex")
            {
                var lol = this.Element.SelectedIndex;
            }
        }

        private void SetPlaceHolderText()
        {
            this.Control.AttributedPlaceholder = new NSAttributedString(this.Element.Placeholder, null, this.Element.PlaceholderColor.ToUIColor());
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

        private void SetIcon()
        {
            string rightPath = string.Empty;
            string leftPath = string.Empty;


            //Right Icon
            if (Element.RightIcon == null)
                rightPath = "Samples/Data/SVG/arrow.svg";
            else
                rightPath = Element.RightIcon;

            RSSvgImage rightSvgIcon = new RSSvgImage() { Source = rightPath, HeightRequest = Element.IconHeight, WidthRequest = Element.IconHeight, Color = Element.IconColor };
            var convertedRightView = Extensions.ViewExtensions.ConvertFormsToNative(rightSvgIcon, new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
            var outerView = new UIView(new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
            outerView.AddSubview(convertedRightView);
            this.Control.RightView = outerView;
            this.Control.RightViewMode = UITextFieldViewMode.Always;


            //Left Icon
            if (Element.LeftIcon != null)
            {
                RSSvgImage leftSvgIcon = new RSSvgImage() { Source = leftPath, HeightRequest = Element.IconHeight, WidthRequest = Element.IconHeight, Color = Element.IconColor };
                var convertedLeftView = Extensions.ViewExtensions.ConvertFormsToNative(leftSvgIcon, new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
                var outerView2 = new UIView(new CGRect(x: 0, y: 0, width: Element.IconHeight, height: Element.IconHeight));
                outerView2.AddSubview(convertedLeftView);
                this.Control.LeftView = outerView;
                this.Control.LeftViewMode = UITextFieldViewMode.Always;
            }
        }
    }

    public interface IModal
    {
        void Show(bool animated);
        void Dismiss(bool animated);

        UIView BackgroundView { get; set; }
        UIView DialogView { get; set; }
    }

    public class CustomAlertView : UIView, IModal
    {
        public UIView BackgroundView { get; set; } = new UIView();
        public UIView DialogView { get; set; } = new UIView();
        private UITableView list;

        public CustomAlertView(string title, RSPickerRenderer renderer)
        {
            this.Frame = UIScreen.MainScreen.Bounds;
            this.Init();

            BackgroundView.Frame = UIScreen.MainScreen.Bounds;
            AddSubview(BackgroundView);

            var dialogViewWidth = this.Frame.Width;

            // Separator
            var separatorLineView = new UIView();
            var separatorFrame = separatorLineView.Frame;
            separatorFrame.Location = new CGPoint(x: 0, y: 0);
            separatorFrame.Size = new CGSize(width: dialogViewWidth, height: 1);
            separatorLineView.Frame = separatorFrame;
            separatorLineView.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            DialogView.AddSubview(separatorLineView);

            // Toolbar
            var toolBar = new UIToolbar();
            var toolBarFrame = toolBar.Frame;
            toolBarFrame.Location = new CGPoint(x: 0, y: 1);
            toolBar.Frame = toolBarFrame;
            toolBar.BarStyle = UIBarStyle.Default;
            toolBar.Translucent = true;
            toolBar.SizeToFit();
            DialogView.AddSubview(toolBar);

            // Buttons
            var doneButton = new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, ((sender, ev) =>
            {
                Dismiss(true);
            }));
            var spaceButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null, null);
            var cancelButton = new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, ((sender, ev) =>
            {
                if(renderer.Element.SelectedItems != null)
                    renderer.Element.SelectedItems.Clear();

                renderer.Element.SelectedItem = null;
                renderer.SetText();

                Dismiss(true);
            }));
            var newItems = new List<UIBarButtonItem>();
            newItems.Insert(0, cancelButton);


            //SearchBar
            UISearchBar uISearchBar;
            if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.LandscapeLeft && UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.LandscapeRight)
                uISearchBar = new UISearchBar(new CGRect(x: 0, y: 0, width: toolBar.Frame.Width - 150, height: toolBar.Frame.Height));
            else
                uISearchBar = new UISearchBar(new CGRect(x: 0, y: 0, width: toolBar.Frame.Width - 250, height: toolBar.Frame.Height));

            //uISearchBar.BackgroundColor = UIColor.Clear;
            uISearchBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            uISearchBar.ShowsCancelButton = false;
            uISearchBar.Translucent = false;
            uISearchBar.SearchBarStyle = UISearchBarStyle.Minimal;
            uISearchBar.Placeholder = "Search";
            UIBarButtonItem textfieldBarButton = new UIBarButtonItem(customView: uISearchBar);

            newItems.Insert(1, spaceButton);
            newItems.Insert(2, textfieldBarButton);
            newItems.Insert(3, spaceButton);
            newItems.Insert(4, doneButton);
            toolBar.Items = newItems.ToArray();
            toolBar.SetNeedsDisplay();


            // List
            list = new UITableView(new CGRect(x: 0, y: separatorLineView.Frame.Height + toolBar.Frame.Height, width: dialogViewWidth, height: 210));
            list.AllowsMultipleSelection = false;
            //list.SetEditing(true, true);
            //list.AllowsMultipleSelectionDuringEditing = true;
            list.Source = new TableSource(renderer.Element.Items.ToArray(), renderer);



            // UIPicker
            //
            // Empty is used, since UIPickerViews have auto-sizing,
            // all that is required is the origin
            //
            UIPickerView uIPickerView = new UIPickerView(CGRect.Empty)
            {
                ShowSelectionIndicator = true,
                Model = new CustomUIPickerViewModel(renderer.Element.ItemsSource, renderer.Element, renderer),
                BackgroundColor = UIColor.LightGray
            };
            // Now update it:
            uIPickerView.Frame = new CGRect(0f, separatorLineView.Frame.Height + toolBar.Frame.Height, dialogViewWidth, uIPickerView.Frame.Height);

            DialogView.AddSubview(uIPickerView);

            //Fixes selectedindicator not showing
            if (renderer.Element.SelectedItem != null)
                uIPickerView.Select(renderer.Element.SelectedIndex, 0, true);

            //DialogView.AddSubview(uIPickerView);



            //UIStackView uIStackView = new UIStackView();
            //uIStackView.Axis = UILayoutConstraintAxis.Horizontal;
            //uIStackView.Distribution = UIStackViewDistribution.FillEqually;
            //uIStackView.Spacing = 0;
            //uIStackView.Frame = new CGRect(0f, separatorLineView.Frame.Height + toolBar.Frame.Height, dialogViewWidth, uIPickerView.Frame.Height);
            //uIStackView.AddArrangedSubview(uIPickerView);
            //uIStackView.AddArrangedSubview(uIPickerView2);
            //uIStackView.AddArrangedSubview(uIPickerView3);


            // DialogView
            var dialogViewHeight = toolBar.Frame.Height + uIPickerView.Frame.Height;
            var DialogViewFrame = DialogView.Frame; 
            DialogViewFrame.Location = new CGPoint(x: 0, y: Frame.Height);
            DialogViewFrame.Size = new CGSize(width: Frame.Width, height: dialogViewHeight);
            DialogView.Frame = DialogViewFrame;
            DialogView.BackgroundColor = UIColor.Clear;
            DialogView.ClipsToBounds = true;
            AddSubview(DialogView);

            UITapGestureRecognizer didTappedOnBackgroundView = new UITapGestureRecognizer((obj) =>
            {
                Dismiss(true);
            });

            BackgroundView.AddGestureRecognizer(didTappedOnBackgroundView);
        }

        // Animation part
        public void Dismiss(bool animated)
        {
            if (animated)
            {
                UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0f; }, () => { });

                UIView.Animate(0.33, 0,
                               UIViewAnimationOptions.CurveEaseInOut,
                               () => { this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height + this.DialogView.Frame.Height / 2); },
                               () => { this.RemoveFromSuperview(); });
            }
            else
            {
                this.RemoveFromSuperview();
            }
        }

        public void Show(bool animated)
        {
            this.BackgroundView.Alpha = 0;
            this.DialogView.Center = new CGPoint(x: this.Center.X, y: this.Frame.Height + this.DialogView.Frame.Height / 2);
            UIApplication.SharedApplication.Delegate?.GetWindow()?.RootViewController?.View.AddSubview(this);

            if (animated)
            {
                UIView.Animate(0.33, () => { this.BackgroundView.Alpha = 0.66f; });

                UIView.Animate(0.33, 0,
                               UIViewAnimationOptions.CurveEaseInOut,
                               () => { this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height - this.DialogView.Frame.Height / 2); },
                               () => { });
            }
            else
            {
                this.BackgroundView.Alpha = 0.66f;
                this.DialogView.Center = new CGPoint(this.Center.X, this.Frame.Height - this.DialogView.Frame.Height / 2);
            }
        }
    }

    public class TableSource : UITableViewSource
    {
        private string[] tableItems;
        private List<object> tableItemsSource;
        private string cellIdentifier = "TableCell";
        private RSPickerRenderer renderer;

        public TableSource(string[] items, RSPickerRenderer Renderer)
        {
            renderer = Renderer;

            if (renderer.Element.ItemsSource == null)
                return;

            tableItems = items;

            if (tableItemsSource == null)
                tableItemsSource = new List<object>();
            else
            {
                tableItemsSource.Clear();
            }

            foreach (object item in renderer.Element.ItemsSource)
            {
                tableItemsSource.Add(item);
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tableItems.Length;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
            string item = tableItems[indexPath.Row];

            //if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
            }

            cell.TextLabel.Text = item;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            renderer.Element.SelectedItem = tableItemsSource[indexPath.Row];
            //renderer.Element.SelectedItems.Add(selectedItem);
            //renderer.CheckedItems[indexPath.Row] = true;
            renderer.SetText();
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            var delectedItem = tableItemsSource[indexPath.Row];
            //renderer.Element.SelectedItems.Remove(delectedItem);
            //renderer.CheckedItems[indexPath.Row] = false;
            renderer.SetText();
        }
    }

    public class CustomUIPickerViewModel : UIPickerViewModel
    {
        private IList<object> myItems;
        protected int selectedIndex = 0;
        private RSPickerBase rsPicker;
        private RSPickerRenderer renderer;

        public CustomUIPickerViewModel(System.Collections.IEnumerable items, RSPickerBase rsPicker, RSPickerRenderer renderer)
        {
            this.renderer = renderer;

            if (this.renderer.Element.ItemsSource == null)
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

        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            return myItems.Count;
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
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

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            selectedIndex = (int)row;
            picker.Select(row, component, false);
            rsPicker.SelectedItem = this.SelectedItem;
            this.renderer.SetText();
        }
    }
}
