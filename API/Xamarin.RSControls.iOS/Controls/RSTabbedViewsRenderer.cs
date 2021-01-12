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
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSTabbedViews), typeof(RSTabbedViewsRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSTabbedViewsRenderer : ViewRenderer<RSTabbedViews, UIView>
    {
        private Grid grid;
        private UIView gridNativeView;
        private UIView nativeView;
        private RSTabbedViewScroll pageScrollView;
        private UIScrollView tabsHolder;
        private UIStackView tabsStack;
        private UIView tabsSlider;
        private int currentPageIndex;
        private NSLayoutConstraint tabSliderWidthConstraint;
        private NSLayoutConstraint tabSliderXOffsetConstraint;
        private bool manualScroll = true;
        private bool manualTabs = false;
        private int indexOfPage = 0;


        public RSTabbedViewsRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<RSTabbedViews> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
                return;

            // Instantiate the native control and assign it to the Control property with
            // the SetNativeControl method
            if (Control == null)
            {
                nativeView = new UIView(); 
                grid = new Grid() { ColumnSpacing = 0, RowSpacing = 0 };
                grid.Parent = this.Element; //So it knows on what page the grid situated
                nativeView.AutoresizingMask = UIViewAutoresizing.None;
                this.AutoresizingMask = UIViewAutoresizing.None;

                for (int i = 0; i < this.Element.Views.Count(); i++)
                {
                    if(this.Element.Views.ElementAt(i) is ContentPage)
                    {
                        Page currentPage = GetParentPage(this.Element);
                        var page = this.Element.Views.ElementAt(i) as ContentPage;
                        page.Parent = currentPage; //Assign parent page otherwise it doesnt belong to a page
                        page.Content.BindingContext = page.BindingContext;
                        grid.Children.Add(page.Content, i, 0);
                    }
                    else
                    {
                        grid.Children.Add(this.Element.Views.ElementAt(i) as View, i, 0);
                    }

                }

                var renderer = Platform.CreateRenderer(grid);
                gridNativeView = renderer.NativeView;


                SetTabBar();
                SetPages();
                pageScrollView.tabs = tabsStack;
                this.SetNativeControl(nativeView);
            }
        }
 
        //Return page of element
        public Page GetParentPage(VisualElement element)
        {
            if (element != null)
            {
                var parent = element.Parent;
                while (parent != null)
                {
                    if (parent is Page)
                    {
                        return parent as Page;
                    }
                    parent = parent.Parent;
                }
            }
            return null;
        }

        //Set pages scroll
        private void SetPages()
        {
            pageScrollView = new RSTabbedViewScroll() { BackgroundColor = UIColor.Green};
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
            var top = pageScrollView.TopAnchor.ConstraintEqualTo(tabsHolder.BottomAnchor);
            top.Active = true;
            top.Priority = 999;
            pageScrollView.BottomAnchor.ConstraintEqualTo(nativeView.BottomAnchor).Active = true;

            pageScrollView.AddSubview(gridNativeView);
            Extensions.ViewExtensions.EdgeTo(pageScrollView, gridNativeView, true, true, true, true);
            gridNativeView.HeightAnchor.ConstraintEqualTo(pageScrollView.HeightAnchor).Active = true;
            gridNativeView.WidthAnchor.ConstraintEqualTo(pageScrollView.WidthAnchor, this.Element.Views.Count()).Active = true;
        }

        private void PageScrollView_DraggingStarted(object sender, EventArgs e)
        {
            manualScroll = true;
        }

        //Set Tabs menu
        private void SetTabBar()
        {
            //Tabs holder
            tabsHolder = new UIScrollView();
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                tabsHolder.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
            tabsHolder.Bounces = false;
            tabsHolder.ShowsHorizontalScrollIndicator = false;
            tabsHolder.BackgroundColor = UIColor.Red;
            this.nativeView.AddSubview(tabsHolder);
            tabsHolder.TranslatesAutoresizingMaskIntoConstraints = false;
            tabsHolder.TopAnchor.ConstraintEqualTo(nativeView.TopAnchor).Active = true;
            tabsHolder.LeadingAnchor.ConstraintEqualTo(nativeView.LeadingAnchor).Active = true;
            tabsHolder.TrailingAnchor.ConstraintEqualTo(nativeView.TrailingAnchor).Active = true;


            tabsStack = new UIStackView();
            tabsStack.BackgroundColor = UIColor.Blue;
            tabsStack.Axis = UILayoutConstraintAxis.Horizontal;
            tabsStack.Distribution = UIStackViewDistribution.FillProportionally;
            tabsHolder.AddSubview(tabsStack);
            tabsStack.TranslatesAutoresizingMaskIntoConstraints = false;

            tabsStack.LeadingAnchor.ConstraintEqualTo(tabsHolder.LeadingAnchor).Active = true;
            tabsStack.TrailingAnchor.ConstraintEqualTo(tabsHolder.TrailingAnchor).Active = true;
            tabsStack.TopAnchor.ConstraintEqualTo(tabsHolder.TopAnchor, 5).Active = true;
            tabsStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(tabsHolder.WidthAnchor).Active = true;
            tabsHolder.HeightAnchor.ConstraintEqualTo(tabsStack.HeightAnchor, 1f, 10).Active = true;


            //Add tabs
            foreach (var item in this.Element.Views)
            {
                UIStackView tab = new UIStackView();
                tab.Axis = UILayoutConstraintAxis.Vertical;
                tab.Distribution = UIStackViewDistribution.FillEqually;
                tabsStack.AddArrangedSubview(tab);

                var currentPage = this.Element.Views.IndexOf(item);

                UITapGestureRecognizer tabTap = new UITapGestureRecognizer(() =>
                {
                    ////Highlight selected tab
                    ((tabsStack.ArrangedSubviews[currentPageIndex] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
                    ((tabsStack.ArrangedSubviews[currentPage] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;

                    //manualTabs = true;
                    //manualScroll = true;
                    var posX = (this.Element.Width * currentPage);
                    //currentPageIndex = currentPage;
                    (this.Element as RSTabbedViews).CurrentView = (this.Element as RSTabbedViews).Views.ElementAt((int)currentPage);
                    pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
                    manualScroll = true;
                    var currentSliderX = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Left;
                    var tabWidth = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Width;
                    var maxScrollAllowed = tabsHolder.ContentSize.Width - nativeView.Frame.Width;
                    if (currentSliderX < maxScrollAllowed)
                    {
                        tabsHolder.SetContentOffset(new CGPoint(currentSliderX, pageScrollView.ContentOffset.Y), true);
                    }
                    else
                    {
                        tabsHolder.SetContentOffset(new CGPoint(maxScrollAllowed, pageScrollView.ContentOffset.Y), true);
                    }

                });
                tab.AddGestureRecognizer(tabTap);

                //Title
                TabsTitle title = new TabsTitle() { TextAlignment = UITextAlignment.Center, TextColor = UIColor.White, BackgroundColor = UIColor.Clear };
                title.Font = UIFont.SystemFontOfSize(19);
                title.Text = (string)item.GetValue(RSTabbedViews.TitleProperty);
                tab.AddArrangedSubview(title);
                tab.TranslatesAutoresizingMaskIntoConstraints = false;
                //tab.WidthAnchor.ConstraintGreaterThanOrEqualTo(200).Active = true;
                tab.HeightAnchor.ConstraintGreaterThanOrEqualTo(40).Active = true;
            }

            //Tabs slider
            tabsSlider = new UIView();
            tabsSlider.BackgroundColor = UIColor.White;
            tabsStack.AddSubview(tabsSlider);
            tabsSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            tabSliderWidthConstraint = tabsSlider.WidthAnchor.ConstraintEqualTo(0);
            tabSliderWidthConstraint.Active = true;
            //tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews.ElementAt(currentPageIndex).Frame.Width;
            tabsSlider.HeightAnchor.ConstraintEqualTo(3).Active = true;
            tabsSlider.BottomAnchor.ConstraintEqualTo(tabsStack.BottomAnchor, 4).Active = true;
            tabSliderXOffsetConstraint = tabsSlider.LeadingAnchor.ConstraintEqualTo(tabsStack.LeadingAnchor);
            tabSliderXOffsetConstraint.Active = true;
        }

        //Scroll event used when manualy scrolled
        private void PageScrollView_Scrolled(object sender, EventArgs e)
        {
            if (!manualScroll)
            {
                tabsStack.LayoutIfNeeded();
                pageScrollView.LayoutIfNeeded();
            }

            if(manualScroll)
            {
                //Console.WriteLine("Manual");
                indexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / nativeView.Frame.Width), MidpointRounding.AwayFromZero);
            }
            pageScrollView.index = indexOfPage;

            //Highlight selected tab
            if (!manualTabs)
            {
                foreach (UIStackView tab in tabsStack.ArrangedSubviews)
                {
                    if (tab == tabsStack.ArrangedSubviews[indexOfPage])
                        (tab.ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;
                    else
                        (tab.ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
                }
            }

            currentPageIndex = indexOfPage;
            var currentScrollingPageIdx = currentPageIndex;

            //Scroll Percentage per page
            var pageWidth = nativeView.Frame.Width;
            var currentScrollX = pageScrollView.ContentOffset.X;
            var currentPositionX = currentPageIndex * pageWidth;

            if (currentScrollX < currentPositionX)
                currentScrollingPageIdx--;

            var maxScrollX = (currentScrollingPageIdx + 1) * pageWidth;
            var minScrollX = maxScrollX - pageWidth;
            var currentScrollPercentage = (currentScrollX - minScrollX) / (maxScrollX - minScrollX);
            var currentSliderX = tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Left;
            var tabWidth = tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Width;

            var tabWidth2 = currentScrollingPageIdx + 1 < tabsStack.ArrangedSubviews.Count() ?
                                                        tabsStack.ArrangedSubviews[currentScrollingPageIdx + 1].Frame.Width :
                                                        tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Width;

            //Set slider X position
            tabSliderXOffsetConstraint.Constant = currentSliderX + currentScrollPercentage * tabWidth;

            //Set slider width
            tabSliderWidthConstraint.Constant = tabWidth - (tabWidth - tabWidth2) * currentScrollPercentage;

            if (currentScrollPercentage < 0)
                Console.WriteLine(currentScrollPercentage);


            if (!manualTabs)
            {
                //Tabs manual scroll
                var tabsScrollX = currentSliderX + (currentScrollPercentage * tabWidth);
                var maxScrollAllowed = tabsHolder.ContentSize.Width - nativeView.Frame.Width;
                if (tabsScrollX < maxScrollAllowed)
                {
                    tabsHolder.SetContentOffset(new CGPoint(tabsScrollX, pageScrollView.ContentOffset.Y), false);
                }
                else
                {
                    tabsHolder.SetContentOffset(new CGPoint(maxScrollAllowed, pageScrollView.ContentOffset.Y), false);
                }
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            manualScroll = false;
            if (currentPageIndex == 0)
            {
                //Set slider width
                Console.WriteLine("Manual");
                tabsStack.LayoutIfNeeded();
                tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews.ElementAt(0).Frame.Width;
            }

            var width = this.Element.WidthRequest != -1 ? this.Element.WidthRequest : this.Element.Width;
            var height = this.Element.HeightRequest != -1 ? this.Element.HeightRequest : this.Element.Height;
            grid.Layout(new Rectangle(0, 0, width * this.Element.Views.Count(), height - tabsStack.Frame.Height));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            pageScrollView.Scrolled -= PageScrollView_Scrolled;
            pageScrollView.DraggingStarted -= PageScrollView_DraggingStarted;
        }
    }

    public class RSTabbedViewScroll : UIScrollView
    {
        public UIStackView tabs;
        public int index;

        public RSTabbedViewScroll()
        {
        }

        public override CGSize ContentSize
        {
            get => base.ContentSize;
            set
            {
                base.ContentSize = value;

                this.SetContentOffset(new CGPoint(this.Bounds.Size.Width * index, this.ContentOffset.Y), false); 
            }
        }
    }
}
