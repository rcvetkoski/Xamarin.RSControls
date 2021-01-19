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
using Xamarin.RSControls.Helpers;
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
        private NSLayoutConstraint tabSliderWidthConstraint;
        private NSLayoutConstraint tabSliderXOffsetConstraint;
        private bool manualScroll = true;
        private bool manualTabs = false;
        private int previousIndexOfPage = 0;
        private int currentIndexOfPage = 0;


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
                //Set current view to first if not set in binding
                if (this.Element.Views.Any() && this.Element.CurrentView == null)
                    this.Element.CurrentView = this.Element.Views.ElementAt(0);

                nativeView = new UIView(); 
                grid = new Grid() { ColumnSpacing = 0, RowSpacing = 0 };
                grid.Parent = this.Element; //So it knows on what page the grid situated
                nativeView.AutoresizingMask = UIViewAutoresizing.None;
                this.AutoresizingMask = UIViewAutoresizing.None;

                for (int i = 0; i < this.Element.Views.Count(); i++)
                {
                    if(this.Element.Views.ElementAt(i) is ContentPage)
                    {
                        Page currentPage = TypeExtensions.GetParentPage(this.Element);
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
                this.SetNativeControl(nativeView);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if(e.PropertyName == "CurrentView")
            {
                if(tabsStack != null && tabsStack.ArrangedSubviews.Any())
                {
                    if(previousIndexOfPage != currentIndexOfPage)
                    {
                        setTabItemsColor(tabsStack.ArrangedSubviews[previousIndexOfPage] as UIStackView, this.Element.BarTextColor);
                        setTabItemsColor(tabsStack.ArrangedSubviews[currentIndexOfPage] as UIStackView, this.Element.BarTextColorSelected);
                    }
                }

                previousIndexOfPage = currentIndexOfPage;
            }
        }


        ////Return page of element
        //public Page GetParentPage(VisualElement element)
        //{
        //    if (element != null)
        //    {
        //        var parent = element.Parent;
        //        while (parent != null)
        //        {
        //            if (parent is Page)
        //            {
        //                return parent as Page;
        //            }
        //            parent = parent.Parent;
        //        }
        //    }
        //    return null;
        //}

        //Set pages scroll
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
            gridNativeView.WidthAnchor.ConstraintEqualTo(pageScrollView.WidthAnchor, this.Element.Views.Count()).Active = true;

            pageScrollView.tabs = tabsStack;
        }

        private void PageScrollView_DraggingStarted(object sender, EventArgs e)
        {
            manualScroll = true;
        }

        //Set Tabs menu
        private void SetTabBar()
        {
            //Tabs holder
            tabsHolder = new UIScrollView() { BackgroundColor = this.Element.BarColor.ToUIColor() };
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                tabsHolder.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
            tabsHolder.Bounces = false;
            tabsHolder.ShowsHorizontalScrollIndicator = false;
            this.nativeView.AddSubview(tabsHolder);
            tabsHolder.TranslatesAutoresizingMaskIntoConstraints = false;

            if(this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
                tabsHolder.TopAnchor.ConstraintEqualTo(nativeView.TopAnchor).Active = true;
            else
                tabsHolder.BottomAnchor.ConstraintEqualTo(nativeView.BottomAnchor).Active = true;

            tabsHolder.LeadingAnchor.ConstraintEqualTo(nativeView.LeadingAnchor).Active = true;
            tabsHolder.TrailingAnchor.ConstraintEqualTo(nativeView.TrailingAnchor).Active = true;


            tabsStack = new UIStackView();
            tabsStack.Axis = UILayoutConstraintAxis.Horizontal;
            tabsStack.Distribution = UIStackViewDistribution.FillProportionally;
            tabsHolder.AddSubview(tabsStack);
            tabsStack.TranslatesAutoresizingMaskIntoConstraints = false;

            tabsStack.LeadingAnchor.ConstraintEqualTo(tabsHolder.LeadingAnchor).Active = true;
            tabsStack.TrailingAnchor.ConstraintEqualTo(tabsHolder.TrailingAnchor).Active = true;
            tabsStack.TopAnchor.ConstraintEqualTo(tabsHolder.TopAnchor, 10).Active = true;
            tabsStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(tabsHolder.WidthAnchor).Active = true;
            var tabsHolderHeightOffset = this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top ? 15 : 30;
            tabsHolder.HeightAnchor.ConstraintEqualTo(tabsStack.HeightAnchor, 1f, tabsHolderHeightOffset).Active = true;


            //Add tabs
            foreach (var item in this.Element.Views)
            {
                UIStackView tab = new UIStackView();
                tab.Axis = UILayoutConstraintAxis.Vertical;
                tab.Distribution = UIStackViewDistribution.FillEqually;
                tabsStack.AddArrangedSubview(tab);

                var currentView = this.Element.Views.IndexOf(item);
                UITapGestureRecognizer tabTap = new UITapGestureRecognizer(() =>
                {
                    var posX = (this.Element.Width * currentView);
                    (this.Element as RSTabbedViews).CurrentView = (this.Element as RSTabbedViews).Views.ElementAt((int)currentView);
                    pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
                    manualScroll = true;
                    var currentSliderX = tabsStack.ArrangedSubviews[currentIndexOfPage].Frame.Left;
                    var tabWidth = tabsStack.ArrangedSubviews[currentIndexOfPage].Frame.Width;
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


                //Icon
                UIView icon = null;
                if (item.GetValue(RSTabbedViews.IconProperty) != null)
                {
                    if(!(item.GetValue(RSTabbedViews.IconProperty).ToString().Contains("svg")))
                    {
                        UIImage image = new UIImage(item.GetValue(RSTabbedViews.IconProperty).ToString());
                        UIImageView imageView = new UIImageView();
                        imageView.Image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                        imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                        imageView.TintColor = this.Element.BarTextColor.ToUIColor();
                        tab.AddArrangedSubview(imageView);
                    }
                    else
                    {
                        RSSvgImage svgIcon = new RSSvgImage() { Source = item.GetValue(RSTabbedViews.IconProperty).ToString(), Color = this.Element.BarTextColor };
                        var renderer = Platform.CreateRenderer(svgIcon);
                        renderer.Element.Layout(new Rectangle(0, 0, 25, 25));
                        renderer.NativeView.TranslatesAutoresizingMaskIntoConstraints = false;
                        icon = renderer.NativeView;
                        UIView holder = new UIView();
                        holder.TranslatesAutoresizingMaskIntoConstraints = false;
                        holder.HeightAnchor.ConstraintEqualTo(25).Active = true;
                        holder.AddSubview(icon);
                        tab.AddArrangedSubview(holder);
                        renderer.NativeView.CenterXAnchor.ConstraintEqualTo(holder.CenterXAnchor, -12.5f).Active = true;
                    }
                }

                //Title
                TabsTitle title = new TabsTitle() { TextAlignment = UITextAlignment.Center, TextColor = this.Element.BarTextColor.ToUIColor() };
                title.Lines = 1;
                title.Font = UIFont.BoldSystemFontOfSize(12);
                title.Text = (string)item.GetValue(RSTabbedViews.TitleProperty);
                tab.AddArrangedSubview(title);
                tab.TranslatesAutoresizingMaskIntoConstraints = false;
                //tab.WidthAnchor.ConstraintGreaterThanOrEqualTo(200).Active = true;
                tab.HeightAnchor.ConstraintGreaterThanOrEqualTo(40).Active = true;
            }

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
            if(this.Element.RSTabPlacement == Enums.RSTabPlacementEnum.Top)
                separator.BottomAnchor.ConstraintEqualTo(tabsStack.BottomAnchor, 5).Active = true;
            else
                separator.TopAnchor.ConstraintEqualTo(tabsStack.TopAnchor, -10).Active = true;

            //Highlight selected tab
            setTabItemsColor(tabsStack.ArrangedSubviews[currentIndexOfPage] as UIStackView, this.Element.BarTextColorSelected);
        }

        private void setTabItemsColor(UIStackView tab, Color color)
        {
            foreach (var itm in tab.ArrangedSubviews)
            {
                if (itm is UILabel)
                    (itm as UILabel).TextColor = color.ToUIColor();
                else if(itm is UIImageView)
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
                currentIndexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / nativeView.Frame.Width), MidpointRounding.AwayFromZero);
            }
            pageScrollView.index = currentIndexOfPage;
            (this.Element as RSTabbedViews).CurrentView = (this.Element as RSTabbedViews).Views.ElementAt((int)currentIndexOfPage);

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

            var tabWidth2 = currentScrollingPageIdx + 1 < tabsStack.ArrangedSubviews.Count() ?
                                                        tabsStack.ArrangedSubviews[currentScrollingPageIdx + 1].Frame.Width :
                                                        tabsStack.ArrangedSubviews[currentScrollingPageIdx].Frame.Width;

            //Set slider X position
            tabSliderXOffsetConstraint.Constant = currentSliderX + currentScrollPercentage * tabWidth;

            //Set slider width
            tabSliderWidthConstraint.Constant = tabWidth - (tabWidth - tabWidth2) * currentScrollPercentage;

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
            if (currentIndexOfPage == 0)
            {
                //Set slider width
                tabsHolder.LayoutIfNeeded();
                tabsStack.LayoutIfNeeded();
                tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews.ElementAt(0).Frame.Width;
            }
            
            var width = this.Element.WidthRequest != -1 ? this.Element.WidthRequest : this.Element.Width;
            var height = this.Element.HeightRequest != -1 ? this.Element.HeightRequest : this.Element.Height;
            grid.Layout(new Rectangle(0, 0, width * this.Element.Views.Count(), height - tabsHolder.Frame.Height));
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
