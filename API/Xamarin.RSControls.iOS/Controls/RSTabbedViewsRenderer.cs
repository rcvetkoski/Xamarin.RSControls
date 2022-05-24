using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Helpers;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSTabbedViews), typeof(RSTabbedViewsRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSTabbedViewsRenderer : ViewRenderer<RSTabbedViews, UIView>
    {
        private UIView nativeView; // main view
        private Grid grid; // Views/Pages 
        private UIView gridNativeView; // Views/Pages
        private RSTabbedViewScroll pageScrollView;
        private UIScrollView tabsHolder; // Tab menu
        private UIStackView tabsStack; // Tab menu
        private UIView tabsSlider; // Tab slider - Tab menu
        private NSLayoutConstraint tabSliderWidthConstraint;
        private NSLayoutConstraint tabSliderXOffsetConstraint;
        private NSLayoutConstraint gridWidthConstraint;
        private bool canUpdateCurrentIndex = false;
        private bool manualScroll = true;
        private int previousIndexOfPage = 0;
        private int currentIndexOfPage = 0;
        private NSObject orientationObserver;
        private UIDeviceOrientation orientation;


        public RSTabbedViewsRenderer()
        {
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "CurrentView")
            {
                //Console.WriteLine("prev: " + previousIndexOfPage + "   current: " + currentIndexOfPage);

                if (tabsStack != null && tabsStack.ArrangedSubviews.Any())
                {
                    if (previousIndexOfPage >= 0 && previousIndexOfPage < tabsStack.ArrangedSubviews.Count())
                        setTabItemsColor(tabsStack.ArrangedSubviews[previousIndexOfPage] as UIStackView, this.Element.BarTextColor);

                    setTabItemsColor(tabsStack.ArrangedSubviews[currentIndexOfPage] as UIStackView, this.Element.BarTextColorSelected);

                    previousIndexOfPage = currentIndexOfPage;
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSTabbedViews> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            // Instantiate the native control and assign it to the Control property with the SetNativeControl method
            if (Control == null)
            {
                // Set main view
                nativeView = new UIView();
                nativeView.AutoresizingMask = UIViewAutoresizing.None;
                this.AutoresizingMask = UIViewAutoresizing.None;

                // Set view/page holder
                grid = new Grid() { ColumnSpacing = 0, RowSpacing = 0 };
                grid.Parent = this.Element; //So it knows on what page the grid situated
                var renderer = Platform.CreateRenderer(grid);
                gridNativeView = renderer.NativeView;


                SetTabBar();
                SetPages();

                // Populate tabs and views/pages at load
                if (this.Element.ItemsSource != null)
                {
                    // Register add or remove event in order to update the pages
                    (Element.ItemsSource as INotifyCollectionChanged).CollectionChanged += RSTabbedViewsRenderer_CollectionChanged;

                    foreach (var item in this.Element.ItemsSource)
                        CreateAndAddToPager(item);
                }
                else
                {
                    foreach (var item in Element.Views)
                        AddViewToPager(item);
                }

                //Highlight selected tab
                setTabItemsColor(tabsStack.ArrangedSubviews[currentIndexOfPage] as UIStackView, this.Element.BarTextColorSelected);

                setGridWidthConstraint();

                gridNativeView.BackgroundColor = UIColor.Purple;
                grid.BackgroundColor = Color.Brown;

                orientation = UIDevice.CurrentDevice.Orientation;
                orientationObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);


                //Set current view to first if not set in binding
                if (this.Element.Views.Any() && this.Element.CurrentView == null)
                    this.Element.CurrentView = this.Element.Views.ElementAt(0);

                this.SetNativeControl(nativeView);
            }
        }

        // Update tab position when screen is rotated
        void DeviceRotated(NSNotification notification)
        {
            if (orientation == UIDeviceOrientation.Unknown)
            {
                orientation = UIDevice.CurrentDevice.Orientation;
            }
            else if (orientation != UIDevice.CurrentDevice.Orientation)
            {
                orientation = UIDevice.CurrentDevice.Orientation;

                // Update scroll position to fit current index
                var posX = (Element.Width * currentIndexOfPage);
                pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
            }
        }

        // Only used if ItemsSource set
        private void RSTabbedViewsRenderer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CreateAndAddToPager(e.NewItems[0]);

                if(currentIndexOfPage <= 0)
                {
                    tabsStack.LayoutIfNeeded();
                    tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews[0].Frame.Width;
                }

                TabItemTap(null, e.NewStartingIndex);

                //DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default).DispatchAsync(() =>
                //{
                //    DispatchQueue.MainQueue.DispatchAsync(() =>
                //    {
                //        TabItemTap(null, e.NewStartingIndex);
                //    });
                //});
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveItemFromViews(e.OldStartingIndex);
            }

            // TODO
            setGridWidthConstraint();
        }

        private void setGridWidthConstraint()
        {
            if (gridWidthConstraint != null)
                gridWidthConstraint.Active = false;

            gridWidthConstraint = gridNativeView.WidthAnchor.ConstraintEqualTo(pageScrollView.WidthAnchor, this.Element.Views.Count());
            gridWidthConstraint.Active = true;
        }

        // Creates and adds ItemsSource item to pager
        private void CreateAndAddToPager(object item)
        {
            // Create forms view
            Forms.View formsView = (this.Element.ItemTemplate).CreateContent() as Forms.View;
            formsView.BindingContext = item as object;
            Element.Views.Add(formsView);

            // Convert to native view and add it to pager
            AddViewToPager(formsView);
        }

        // Convert forms view to native and add it to pages vaiable which is later used in pageAdapter
        private void AddViewToPager(VisualElement formsView)
        {
            // Ads view to grid ("pager")
            if (formsView is ContentPage)
            {
                Page currentPage = TypeExtensions.GetParentPage(this.Element);
                var page = formsView as ContentPage;
                page.Parent = currentPage; //Assign parent page otherwise it doesnt belong to a page
                page.Content.BindingContext = page.BindingContext;
                grid.Children.Add(page.Content, Element.Views.IndexOf(formsView), 0);
            }
            else
            {
                grid.Children.Add(formsView as View, Element.Views.IndexOf(formsView), 0);
            }

            // TODO
            var width = this.Element.WidthRequest != -1 ? this.Element.WidthRequest : this.Element.Width;
            var height = this.Element.HeightRequest != -1 ? this.Element.HeightRequest : this.Element.Height;
            grid.Layout(new Rectangle(0, 0, width * this.Element.Views.Count(), height - tabsHolder.Frame.Height));


            // Add view as tab in tab menu
            AddTab(formsView);
        }

        // Removes item / view from views and pager
        private void RemoveItemFromViews(int pos)
        {
            var tabToRemove = tabsStack.ArrangedSubviews.ElementAt(pos);
            tabsStack.RemoveArrangedSubview(tabToRemove);
            tabToRemove.RemoveFromSuperview();
            Element.Views.RemoveAt(pos);
            grid.Children.RemoveAt(pos);


            // Set new item collumn position since one item has been removed
            for (int i = grid.Children.Count - 1; i >= pos; i--)
            {
                Grid.SetColumn(grid.Children[i], i);
                var tab = tabsStack.ArrangedSubviews[i];
                (tab as RSUIStackView).Index = i;
            }


            // Layout forms views
            var width = this.Element.WidthRequest != -1 ? this.Element.WidthRequest : this.Element.Width;
            var height = this.Element.HeightRequest != -1 ? this.Element.HeightRequest : this.Element.Height;
            grid.Layout(new Rectangle(0, 0, width * this.Element.Views.Count(), height - tabsHolder.Frame.Height));


            // Stop scroll in case previous scroll animation hasn't ended which can cause crash when removing item
            CGPoint offset = pageScrollView.ContentOffset;
            pageScrollView.SetContentOffset(offset, false);


            manualScroll = false;
            // Set CurrentPageIndex to forms element
            if (pos > 0)
            {
                currentIndexOfPage = pos - 1;
                Element.CurrentView = Element.Views.ElementAt(pos - 1);
                Element.CurrentPageIndex = pos - 1;
            }
            else if (Element.Views.Any())
            {
                currentIndexOfPage = 0;
                Element.CurrentView = Element.Views.ElementAt(0);
                Element.CurrentPageIndex = 0;
            }
            else
            {
                currentIndexOfPage = -1;
                Element.CurrentView = null;
                Element.CurrentPageIndex = -1;
            }


            tabsStack.LayoutIfNeeded();
            if (currentIndexOfPage >= 0)
            {
                tabSliderXOffsetConstraint.Constant = tabsStack.ArrangedSubviews[currentIndexOfPage].Frame.Left;
                tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews[currentIndexOfPage].Frame.Width;
            }
            else
            {
                tabSliderXOffsetConstraint.Constant = 0;
                tabSliderWidthConstraint.Constant = 0;
            }

            var posX = (this.Element.Width * currentIndexOfPage);

            if(currentIndexOfPage != Element.Views.Count() - 1)
                pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
        }

        // Set Tabs menu
        private void SetTabBar()
        {
            // Tabs holder - ScrollVIew
            tabsHolder = new UIScrollView() { BackgroundColor = this.Element.BarColor.ToUIColor() };
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                tabsHolder.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
            tabsHolder.Bounces = false;
            tabsHolder.ShowsHorizontalScrollIndicator = false;
            this.nativeView.AddSubview(tabsHolder);
            tabsHolder.TranslatesAutoresizingMaskIntoConstraints = false;
            tabsHolder.LeadingAnchor.ConstraintEqualTo(nativeView.LeadingAnchor).Active = true;
            tabsHolder.TrailingAnchor.ConstraintEqualTo(nativeView.TrailingAnchor).Active = true;
            if (this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
                tabsHolder.TopAnchor.ConstraintEqualTo(nativeView.TopAnchor).Active = true;
            else
                tabsHolder.BottomAnchor.ConstraintEqualTo(nativeView.BottomAnchor).Active = true;


            // Tabs - Content
            tabsStack = new UIStackView();
            tabsStack.Axis = UILayoutConstraintAxis.Horizontal;
            tabsStack.Distribution = UIStackViewDistribution.FillProportionally;
            tabsStack.Spacing = 1; // Do not remove this, it fixes the bug when dynamically adding new tabs
            tabsHolder.AddSubview(tabsStack);
            tabsStack.TranslatesAutoresizingMaskIntoConstraints = false;
            tabsStack.LeadingAnchor.ConstraintEqualTo(tabsHolder.LeadingAnchor).Active = true;
            tabsStack.TrailingAnchor.ConstraintEqualTo(tabsHolder.TrailingAnchor).Active = true;
            tabsStack.TopAnchor.ConstraintEqualTo(tabsHolder.TopAnchor, 10).Active = true;
            tabsStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(tabsHolder.WidthAnchor).Active = true;
            var tabsHolderHeightOffset = this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top ? 15 : 30;
            tabsHolder.HeightAnchor.ConstraintEqualTo(tabsStack.HeightAnchor, 1f, tabsHolderHeightOffset).Active = true;


            //Tabs slider
            tabsSlider = new UIView();
            tabsSlider.BackgroundColor = this.Element.SliderColor.ToUIColor();
            tabsStack.AddSubview(tabsSlider);
            tabsSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            tabSliderWidthConstraint = tabsSlider.WidthAnchor.ConstraintEqualTo(0);
            tabSliderWidthConstraint.Active = true;
            //tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews.ElementAt(currentPageIndex).Frame.Width;
            tabsSlider.HeightAnchor.ConstraintEqualTo(2).Active = true;
            tabsSlider.BottomAnchor.ConstraintEqualTo(tabsStack.BottomAnchor, 5).Active = true;
            tabSliderXOffsetConstraint = tabsSlider.LeadingAnchor.ConstraintEqualTo(tabsStack.LeadingAnchor);
            tabSliderXOffsetConstraint.Active = true;


            //Shadow
            tabsHolder.Layer.ShadowColor = UIColor.LightGray.CGColor;
            tabsHolder.Layer.ShadowOpacity = 0.5f;
            tabsHolder.Layer.ShadowOffset = new CGSize(0, 0);
            tabsHolder.Layer.ShadowRadius = 10;
            tabsHolder.Layer.MasksToBounds = false; // This is important
            UIView separator = new UIView() { BackgroundColor = UIColor.LightGray };
            tabsStack.AddSubview(separator);
            separator.TranslatesAutoresizingMaskIntoConstraints = false;
            separator.LeadingAnchor.ConstraintEqualTo(tabsStack.LeadingAnchor).Active = true;
            separator.TrailingAnchor.ConstraintEqualTo(tabsStack.TrailingAnchor).Active = true;
            separator.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;
            if (this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
                separator.BottomAnchor.ConstraintEqualTo(tabsStack.BottomAnchor, 5).Active = true;
            else
                separator.TopAnchor.ConstraintEqualTo(tabsStack.TopAnchor, -10).Active = true;
        }

        // Create and add tab to tabbar menu
        private void AddTab(VisualElement item)
        {
            RSUIStackView tab = new RSUIStackView();
            tab.Axis = UILayoutConstraintAxis.Vertical;
            tab.Distribution = UIStackViewDistribution.FillProportionally;
            tabsStack.AddArrangedSubview(tab);


            // Tab tap select gesture event
            tab.Index = this.Element.Views.IndexOf(item);
            UITapGestureRecognizer tabTap = new UITapGestureRecognizer();
            tabTap.AddTarget(() => TabItemTap(tabTap, tab.Index));
            tab.AddGestureRecognizer(tabTap);


            //Icon
            if (!string.IsNullOrEmpty(item.GetValue(RSTabbedViews.IconProperty).ToString()))
            {
                UIView icon = null;

                if (!(item.GetValue(RSTabbedViews.IconProperty).ToString().Contains("svg")))
                {
                    UIImage image = new UIImage(item.GetValue(RSTabbedViews.IconProperty).ToString());
                    UIImageView imageView = new UIImageView();
                    imageView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    imageView.TintColor = this.Element.BarTextColor.ToUIColor();
                    imageView.TranslatesAutoresizingMaskIntoConstraints = false;
                    imageView.HeightAnchor.ConstraintEqualTo(24).Active = true;
                    tab.AddArrangedSubview(imageView);
                }
                else
                {
                    RSSvgImage svgIcon = new RSSvgImage() { Source = item.GetValue(RSTabbedViews.IconProperty).ToString(), Color = this.Element.BarTextColor };
                    var renderer = Platform.CreateRenderer(svgIcon);
                    renderer.Element.Layout(new Rectangle(0, 0, 24, 24));
                    renderer.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;
                    icon = renderer.NativeView;
                    UIView holder = new UIView();
                    holder.TranslatesAutoresizingMaskIntoConstraints = false;
                    holder.HeightAnchor.ConstraintEqualTo(24).Active = true;
                    holder.AddSubview(icon);
                    tab.AddArrangedSubview(holder);
                    renderer.NativeView.CenterXAnchor.ConstraintEqualTo(holder.CenterXAnchor, -12f).Active = true;
                }
            }

            //Title
            TabsTitle title = new TabsTitle() { TextAlignment = UITextAlignment.Center, TextColor = this.Element.BarTextColor.ToUIColor() };
            title.Lines = 1;
            title.Font = UIFont.BoldSystemFontOfSize(12);
            title.Text = (string)item.GetValue(RSTabbedViews.TitleProperty);
            tab.AddArrangedSubview(title);
            tab.TranslatesAutoresizingMaskIntoConstraints = false;
            tab.HeightAnchor.ConstraintGreaterThanOrEqualTo(40).Active = true;
        }

        // Tab item tap gesture method
        private void TabItemTap(UITapGestureRecognizer uITapGesture, int index)
        {
            // Set CurrentPageIndex used for TabSelected event in RSTabbedViews
            canUpdateCurrentIndex = false;
            currentIndexOfPage = index;
            Element.CurrentView = Element.Views.ElementAt(index); // CurrentView needs to be set before CurrentPageIndex, becasue CurrentPageIndex setter triggers eventhandler "TabSelected"
            Element.CurrentPageIndex = index;
            manualScroll = true;
            var posX = (this.Element.Width * index);
            pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
        }

        // Set pages
        private void SetPages()
        {
            pageScrollView = new RSTabbedViewScroll();
            pageScrollView.ShowsHorizontalScrollIndicator = false;
            pageScrollView.Bounces = false;
            pageScrollView.Scrolled += PageScrollView_Scrolled;
            pageScrollView.DraggingStarted += PageScrollView_DraggingStarted;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                pageScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
            pageScrollView.PagingEnabled = true;
            this.nativeView.AddSubview(pageScrollView);

            pageScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
            pageScrollView.LeadingAnchor.ConstraintEqualTo(nativeView.LeadingAnchor).Active = true;
            pageScrollView.TrailingAnchor.ConstraintEqualTo(nativeView.TrailingAnchor).Active = true;

            if (this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
            {
                var top = pageScrollView.TopAnchor.ConstraintEqualTo(tabsHolder.BottomAnchor);
                top.Active = true;
                top.Priority = 999;
            }
            else
            {
                var bottom = pageScrollView.TopAnchor.ConstraintEqualTo(nativeView.TopAnchor);
                bottom.Active = true;
                bottom.Priority = 999;
            }

            if (this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
                pageScrollView.BottomAnchor.ConstraintEqualTo(nativeView.BottomAnchor).Active = true;
            else
                pageScrollView.BottomAnchor.ConstraintEqualTo(tabsHolder.TopAnchor).Active = true;

            pageScrollView.AddSubview(gridNativeView);
            Extensions.ViewExtensions.EdgeTo(pageScrollView, gridNativeView, true, true, true, true);
            gridNativeView.HeightAnchor.ConstraintEqualTo(pageScrollView.HeightAnchor).Active = true;
            //gridNativeView.WidthAnchor.ConstraintEqualTo(pageScrollView.WidthAnchor, this.Element.Views.Count()).Active = true;
        }

        // Set tabs color
        private void setTabItemsColor(UIStackView tab, Color color)
        {
            foreach (var itm in tab.ArrangedSubviews)
            {
                if (itm is UILabel)
                    (itm as UILabel).TextColor = color.ToUIColor();
                else if (itm is UIImageView)
                {
                    itm.TintColor = color.ToUIColor();
                }
                else
                {
                    RSSvgImage svImage = (itm.Subviews[0] as SkiaSharp.Views.Forms.SKCanvasViewRenderer).Element as RSSvgImage;
                    svImage.Color = color;
                    svImage.InvalidateSurface();
                }
            }
        }

        // Set manual scroll to true
        private void PageScrollView_DraggingStarted(object sender, EventArgs e)
        {
            manualScroll = true;
            canUpdateCurrentIndex = true;
        }

        // Scroll event used when manualy scrolled
        private void PageScrollView_Scrolled(object sender, EventArgs e)
        {
            if (manualScroll)
                currentIndexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / nativeView.Frame.Width), MidpointRounding.AwayFromZero);


            // Fix slider position bug when rotating screen
            //if (!manualScroll)
            //{
            //    pageScrollView.index = currentIndexOfPage;
            //    tabsStack.LayoutIfNeeded();
            //    pageScrollView.LayoutIfNeeded();
            //}


            // Set CurrentView and CurrentPageIndex used for TabSelected event in RSTabbedViews
            if (Element.CurrentPageIndex != currentIndexOfPage && canUpdateCurrentIndex)
            {
                // CurrentView needs to be set before CurrentPageIndex, becasue CurrentPageIndex setter triggers eventhandler "TabSelected"
                Element.CurrentView = Element.Views.ElementAt(currentIndexOfPage);
                Element.CurrentPageIndex = currentIndexOfPage;
            }

            // Scrolling calculations
            var currentScrollingPageIdx = currentIndexOfPage;


            //Scroll Percentage per page
            var pageWidth = nativeView.Frame.Width;
            var currentScrollX = pageScrollView.ContentOffset.X;
            var currentPositionX = currentIndexOfPage * pageWidth;

            if (currentScrollX < currentPositionX)
                currentScrollingPageIdx--;

            var maxScrollX = (currentScrollingPageIdx + 1) * pageWidth;
            var minScrollX = maxScrollX - pageWidth;
            var currentScrollPercentage = (currentScrollX - minScrollX) / (maxScrollX - minScrollX);
            var currentSliderX = tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Left;
            var tabWidth = tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Width;

            var tabWidth2 =
                (currentScrollingPageIdx + 1) < tabsStack.ArrangedSubviews.Count() ?
                tabsStack.ArrangedSubviews[currentScrollingPageIdx + 1].Frame.Width :
                tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Width;


            //Set slider X position
            tabSliderXOffsetConstraint.Constant = currentSliderX + currentScrollPercentage * tabWidth;

            //Set slider width
            tabSliderWidthConstraint.Constant = tabWidth - (tabWidth - tabWidth2) * currentScrollPercentage;


            //Tabs manual scroll
            var tabsScrollX = currentSliderX + (currentScrollPercentage * tabWidth);
            var maxScrollAllowed = tabsHolder.ContentSize.Width - nativeView.Frame.Width;
            var halfWidth = nativeView.Frame.Width / 2;
            var halfSliderWidth = tabSliderWidthConstraint.Constant / 2;


            if ((tabsScrollX + halfSliderWidth - halfWidth) < maxScrollAllowed)
            {
                if ((tabSliderXOffsetConstraint.Constant + halfSliderWidth) >= halfWidth)
                    tabsHolder.SetContentOffset(new CGPoint(tabsScrollX + halfSliderWidth - halfWidth, pageScrollView.ContentOffset.Y), false);
                else
                    tabsHolder.SetContentOffset(new CGPoint(0, pageScrollView.ContentOffset.Y), false);
            }
            else
            {
                tabsHolder.SetContentOffset(new CGPoint(maxScrollAllowed, pageScrollView.ContentOffset.Y), false);
            }
        }

        // Do measures and adjust size
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            //// Update scroll position to fit current index
            //if (rotated && tempWidth != pageScrollView.Frame.Width)
            //{
            //    var posX = (this.Element.Width * currentIndexOfPage);
            //    rotated = false;
            //    tempWidth = pageScrollView.Frame.Width;
            //}

            manualScroll = false;

            // Set slider width
            if (currentIndexOfPage == 0)
            {
                tabsHolder.LayoutIfNeeded();
                tabsStack.LayoutIfNeeded();
                tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews.ElementAt(0).Frame.Width;
            }

            // Layout forms views
            var width = this.Element.WidthRequest != -1 ? this.Element.WidthRequest : this.Element.Width;
            var height = this.Element.HeightRequest != -1 ? this.Element.HeightRequest : this.Element.Height;
            grid.Layout(new Rectangle(0, 0, width * this.Element.Views.Count(), height - tabsHolder.Frame.Height));
        }

        // Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Element != null)
            {
                if (Element.ItemsSource != null)
                    (Element.ItemsSource as INotifyCollectionChanged).CollectionChanged -= RSTabbedViewsRenderer_CollectionChanged;
            }

            NSNotificationCenter.DefaultCenter.RemoveObserver(orientationObserver);
            pageScrollView.Scrolled -= PageScrollView_Scrolled;
            pageScrollView.DraggingStarted -= PageScrollView_DraggingStarted;
        }
    }

    // Used for size adjustement (Fix selected tab position on rotation change)
    public class RSTabbedViewScroll : UIScrollView
    {
        public int index;

        public override CGSize ContentSize
        {
            get => base.ContentSize;
            set
            {
                base.ContentSize = value;

                // Fix selected tab position on rotation change
                //this.SetContentOffset(new CGPoint(this.Bounds.Size.Width * index, this.ContentOffset.Y), false);
            }
        }
    }

    // Used to asign index to AddTarget on tap gesture as position parameter so it can be changed if elements change
    public class RSUIStackView : UIStackView
    {
        public int Index {get; set;}
    }
}
