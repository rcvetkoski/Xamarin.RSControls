using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

//[assembly: ExportRenderer(typeof(RSTabbedPage), typeof(RSTabbedPageRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSTabbedPageRenderer : PageRenderer
    {
        IntPtr tokenObserveContentSize = (IntPtr)1;

        private UIView layout;
        private UIScrollView pageScrollView;
        private UIScrollView tabsHolder;
        private UIStackView tabsStack;
        private UIView tabsSlider;
        private UIStackView container;
        private int tabsScrollHeight;
        private int pagesCount;
        private int currentPageIndex;
        private NSLayoutConstraint tabSliderWidthConstraint;
        private NSLayoutConstraint tabSliderXOffsetConstraint;
        private nfloat tempWidth;
        private bool manualScroll = true;
        private bool manualTabs = false;
        private List<UIView> pages;
        private bool isSliderWidthSetted = false;
        StackLayout stack;
        IVisualElementRenderer renderer;
        Grid grid;

        public RSTabbedPageRenderer()
        {
            tabsScrollHeight = 60;
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            //tempWidth is used to check if screen orientation is changing so it will trigger auto scroll function
            tempWidth = UIScreen.MainScreen.Bounds.Width;
            currentPageIndex = (this.Element as RSTabbedPage).TabIndex;
            pagesCount = (this.Element as RSTabbedPage).Children.Count;

            (this.Element as RSTabbedPage).CurrentPageChanged += RSTabbedPageRenderer_CurrentPageChanged;

            foreach (var formsPage in (this.Element as RSTabbedPage).Children)
            {
                formsPage.SizeChanged += FormsPage_SizeChanged;
                formsPage.TabIndex = (this.Element as RSTabbedPage).Children.IndexOf(formsPage);
            }

            pages = new List<UIView>();

            //Remove pages so we can add our new page holder wich is using scroll paging
            foreach (UIView view in this.View.Subviews)
            {
                view.RemoveFromSuperview();
                pages.Add(view);
            }

            layout = new UIView();
            this.View.AddSubview(layout);
            Extensions.ViewExtensions.EdgeTo(this.View, layout, true, true, true, true);
            //this.layout.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);


            //Tabs menu
            SetTabBar();
            //SetCollection();
            //Pages Scroll
            SetPages();
        }

        private void RSTabbedPageRenderer_CurrentPageChanged(object sender, EventArgs e)
        {
            //tabSliderWidthConstraint.Constant = tabsStack.Subviews[1].Frame.Width;
            //CoreAnimation.CATransaction();
            //CoreAnimation.CATransaction.DisableActions = true;

            //((tabsStack.ArrangedSubviews[lastPageIndex] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
            //((tabsStack.ArrangedSubviews[currentPageIndex] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;

            //CoreAnimation.CATransaction.Commit();



            //Console.WriteLine("Page" + currentPageIndex);
        }

        private void FormsPage_SizeChanged(object sender, EventArgs e)
        {
            if (layout.Frame.Height != 0)
                (sender as VisualElement).Layout(new Rectangle(layout.Frame.Width * (sender as VisualElement).TabIndex, 0, layout.Frame.Width, layout.Frame.Height - tabsHolder.Frame.Height));
        }

        //Set tabs menu
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
            layout.AddSubview(tabsHolder);
            tabsHolder.TranslatesAutoresizingMaskIntoConstraints = false;
            tabsHolder.LeadingAnchor.ConstraintEqualTo(layout.LeadingAnchor).Active = true;
            tabsHolder.TrailingAnchor.ConstraintEqualTo(layout.TrailingAnchor).Active = true;
            tabsHolder.TopAnchor.ConstraintEqualTo(layout.TopAnchor).Active = true;


            tabsStack = new UIStackView();
            tabsStack.BackgroundColor = UIColor.Clear;
            tabsStack.Axis = UILayoutConstraintAxis.Horizontal;
            tabsStack.Distribution = UIStackViewDistribution.FillProportionally;
            tabsHolder.AddSubview(tabsStack);
            tabsStack.TranslatesAutoresizingMaskIntoConstraints = false;

            tabsStack.LeadingAnchor.ConstraintEqualTo(tabsHolder.LeadingAnchor).Active = true;
            tabsStack.TrailingAnchor.ConstraintEqualTo(tabsHolder.TrailingAnchor).Active = true;
            tabsStack.TopAnchor.ConstraintEqualTo(tabsHolder.TopAnchor, 5).Active = true;
            //tabsStack.HeightAnchor.ConstraintEqualTo(tabsScrollHeight - 10).Active = true;
            tabsStack.WidthAnchor.ConstraintGreaterThanOrEqualTo(tabsHolder.WidthAnchor).Active = true;
            tabsHolder.HeightAnchor.ConstraintEqualTo(tabsStack.HeightAnchor, 1f, 10).Active = true;



            //Add tabs
            foreach (var item in (this.Element as RSTabbedPage).Children)
            {
                UIStackView tab = new UIStackView();
                tab.Axis = UILayoutConstraintAxis.Vertical;
                tab.Distribution = UIStackViewDistribution.FillEqually;
                tabsStack.AddArrangedSubview(tab);

                var currentPage = (this.Element as RSTabbedPage).Children.IndexOf(item);

                UITapGestureRecognizer tabTap = new UITapGestureRecognizer(() =>
                {
                    ////Highlight selected tab
                    ((tabsStack.ArrangedSubviews[currentPageIndex] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
                    ((tabsStack.ArrangedSubviews[currentPage] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;

                    manualTabs = true;
                    manualScroll = true;
                    var posX = (UIScreen.MainScreen.Bounds.Width * currentPage);
                    //currentPageIndex = currentPage;
                    (this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt((int)currentPage);
                    pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);

                    var currentSliderX = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Left;
                    var tabWidth = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Width;
                    var maxScrollAllowed = tabsHolder.ContentSize.Width - layout.Frame.Width;
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
                title.Text = item.Title;


                //Icon
                UIView icon = null;
                if (item.IconImageSource != null)
                {
                    var toBeSearched = "File: ";
                    string path = item.IconImageSource.ToString().Substring(item.IconImageSource.ToString().IndexOf(toBeSearched) + toBeSearched.Length);
                    RSSvgImage svgIcon = new RSSvgImage() { Source = path, Color = Color.Black, BackgroundColor = Color.Green };

                    Forms.Label vv = new Forms.Label() { Text = "ZTet", BackgroundColor = Color.Transparent };
                    var renderer = Platform.CreateRenderer(vv);
                    renderer.Element.Layout(new Rectangle(0, 0, 25, 25));
                    icon = renderer.NativeView;
                    //icon.BackgroundColor = UIColor.Black;
                    //icon.BackgroundColor = UIColor.Yellow;
                    //icon.TranslatesAutoresizingMaskIntoConstraints = false;
                    //icon.WidthAnchor.ConstraintEqualTo(25).Active = true;
                    //icon.HeightAnchor.ConstraintEqualTo(25).Active = true;
                    //icon.BackgroundColor = UIColor.SystemPinkColor;
                    //tab.AddArrangedSubview(icon);
                }

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
            tabsSlider.BottomAnchor.ConstraintEqualTo(tabsStack.BottomAnchor, 5).Active = true;
            tabSliderXOffsetConstraint = tabsSlider.LeadingAnchor.ConstraintEqualTo(tabsStack.LeadingAnchor);
            tabSliderXOffsetConstraint.Active = true;
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr ctx)
        {
            if (tempWidth != UIScreen.MainScreen.Bounds.Width)
            {
                manualScroll = false;
                tempWidth = UIScreen.MainScreen.Bounds.Width;
            }
            // Handle change.
            if (ctx == tokenObserveContentSize)
            {
                //Update scroll position if needed
                var currentPage = (this.Element as RSTabbedPage).Children.IndexOf((this.Element as RSTabbedPage).CurrentPage);
                if (!manualScroll)
                {
                    tabsHolder.LayoutIfNeeded();
                    //tabsStack.LayoutSubviews();

                    //PageScroll manual scroll to proper position
                    pageScrollView.SetContentOffset(new CGPoint(pageScrollView.Frame.Width * currentPageIndex, pageScrollView.ContentOffset.Y), false);


                    var tabX = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Left;
                    var tabWidth = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Width;


                    //Slider X position
                    tabSliderXOffsetConstraint.Constant = tabX;

                    //Set slider width
                    tabSliderWidthConstraint.Constant = tabWidth;


                    //Tabs manual scroll
                    var maxScrollAllowed = tabsHolder.ContentSize.Width - layout.Frame.Width;
                    if (tabX < maxScrollAllowed)
                    {
                        tabsHolder.SetContentOffset(new CGPoint(tabX, pageScrollView.ContentOffset.Y), false);
                    }
                    else
                    {
                        tabsHolder.SetContentOffset(new CGPoint(maxScrollAllowed, pageScrollView.ContentOffset.Y), false);
                    }

                    //(this.Element as VisualElement).Layout(new Rectangle(0, 0, (this.Element as VisualElement).Width * pages.Count(), pageScrollView.Frame.Height));
                }
            }
            else
            {
                // invoke the base implementation for unhandled events
                base.ObserveValue(keyPath, ofObject, change, ctx);
            }
        }

        //Set pages scroll
        private void SetPages()
        {
            pageScrollView = new UIScrollView();
            pageScrollView.Bounces = false;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                pageScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
            pageScrollView.Scrolled += PageScrollView_Scrolled;
            pageScrollView.DraggingStarted += PageScrollView_DraggingStarted;
            pageScrollView.DecelerationEnded += PageScrollView_DecelerationEnded;
            pageScrollView.ScrollAnimationEnded += PageScrollView_ScrollAnimationEnded;

            //ContenSize change observer
            pageScrollView.AddObserver(this, (NSString)"contentSize",
            NSKeyValueObservingOptions.Old | NSKeyValueObservingOptions.New,
            tokenObserveContentSize);

            pageScrollView.PagingEnabled = true;
            this.layout.AddSubview(pageScrollView);
            //Constraint pages scroll
            //Extensions.ViewExtensions.EdgeTo(layout, pageScrollView, true, true, true, true);

            pageScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
            pageScrollView.LeadingAnchor.ConstraintEqualTo(layout.LeadingAnchor).Active = true;
            pageScrollView.TrailingAnchor.ConstraintEqualTo(layout.TrailingAnchor).Active = true;
            pageScrollView.TopAnchor.ConstraintEqualTo(tabsHolder.BottomAnchor).Active = true;
            pageScrollView.BottomAnchor.ConstraintEqualTo(layout.BottomAnchor).Active = true;


            container = new UIStackView();
            container.Axis = UILayoutConstraintAxis.Horizontal;
            container.Distribution = UIStackViewDistribution.FillEqually;
            pageScrollView.AddSubview(container);
            Extensions.ViewExtensions.EdgeTo(pageScrollView, container, true, true, true, true);
            container.HeightAnchor.ConstraintEqualTo(pageScrollView.HeightAnchor).Active = true;
            container.WidthAnchor.ConstraintEqualTo(pageScrollView.WidthAnchor, pages.Count()).Active = true;

            foreach (var view in pages)
            {
                view.Hidden = false;
                container.AddArrangedSubview(view);
            }
        }

        private void PageScrollView_ScrollAnimationEnded(object sender, EventArgs e)
        {
            //var indexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / layout.Frame.Width), MidpointRounding.AwayFromZero);
            //(this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt(indexOfPage);
            //currentPageIndex = indexOfPage;
        }

        private void PageScrollView_DecelerationEnded(object sender, EventArgs e)
        {
            var indexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / layout.Frame.Width), MidpointRounding.AwayFromZero);
            currentPageIndex = indexOfPage;
            (this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt(indexOfPage);
        }

        //Used to tell that scroll is manualy started and no when screen size changes
        private void PageScrollView_DraggingStarted(object sender, EventArgs e)
        {
            var indexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / layout.Frame.Width), MidpointRounding.AwayFromZero);
            (this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt(indexOfPage);
            currentPageIndex = indexOfPage;
            manualScroll = true;
            manualTabs = false;

            //Console.WriteLine("dragging");
        }

        //Scroll event used when manualy scrolled
        private void PageScrollView_Scrolled(object sender, EventArgs e)
        {
            if (tempWidth != UIScreen.MainScreen.Bounds.Width)
            {
                manualScroll = false;
                tempWidth = UIScreen.MainScreen.Bounds.Width;
            }

            if (manualScroll)
            {
                //Highlight selected tab
                var indexOfPage = (int)Math.Round((pageScrollView.ContentOffset.X / layout.Frame.Width), MidpointRounding.AwayFromZero);

                if(!manualTabs)
                {
                    foreach (UIStackView tab in tabsStack.ArrangedSubviews)
                    {
                        if (tab == tabsStack.ArrangedSubviews[indexOfPage])
                            (tab.ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;
                        else
                            (tab.ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
                    }
                }


                //((tabsStack.ArrangedSubviews[currentPageIndex] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.White;
                //((tabsStack.ArrangedSubviews[indexOfPage] as UIStackView).ArrangedSubviews[0] as UILabel).TextColor = UIColor.Yellow;

                currentPageIndex = indexOfPage;
                var currentScrollingPageIdx = currentPageIndex;

                //Scroll Percentage per page
                var pageWidth = layout.Frame.Width;
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

                if(currentScrollPercentage < 0)
                    Console.WriteLine(currentScrollPercentage);

                Console.WriteLine(currentScrollX + "    " + currentScrollingPageIdx);


                if (!manualTabs)
                {
                    //Tabs manual scroll
                    var tabsScrollX = currentSliderX + (currentScrollPercentage * tabWidth);
                    var maxScrollAllowed = tabsHolder.ContentSize.Width - layout.Frame.Width;
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
        }

        //private void SetCollection()
        //{
        //    var flowLayout = new CustomUICollectionViewFlowLayout();
        //    tabsHolder = new CustomCollectionView(new CGRect(0, 0, 0, 0), flowLayout);
        //    this.layout.AddSubview(tabsHolder);
        //    tabsHolder.TranslatesAutoresizingMaskIntoConstraints = false;
        //    tabsHolder.LeadingAnchor.ConstraintEqualTo(layout.LeadingAnchor).Active = true;
        //    tabsHolder.TrailingAnchor.ConstraintEqualTo(layout.TrailingAnchor).Active = true;
        //    tabsHolder.TopAnchor.ConstraintEqualTo(layout.TopAnchor).Active = true;
        //    tabsHolder.HeightAnchor.ConstraintEqualTo(tabsScrollHeight).Active = true;


        //    var data = new CustomCollectionViewDataSource(this.Element as RSTabbedPage);
        //    tabsHolder.DataSource = data;
        //    tabsHolder.RegisterClassForCell(typeof(CustomUICollectionViewCell), "CellId");
        //    //tabsHolder.BackgroundColor = UIColor.Yellow;
        //    //foreach (var formsPage in (this.Element as RSTabbedPage).Children)
        //    //{
        //    //    formsPage.Layout(new Rectangle(0, 0, pageScrollView.Frame.Width, pageScrollView.Frame.Height));
        //    //}
        //}

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (!isSliderWidthSetted && tabsStack.ArrangedSubviews[currentPageIndex].Frame.Width != 0)
            {
                tabSliderWidthConstraint.Constant = tabsStack.ArrangedSubviews[currentPageIndex].Frame.Width;
                isSliderWidthSetted = true;

                foreach (var formsPage in (this.Element as RSTabbedPage).Children)
                {
                    formsPage.Layout(new Rectangle(layout.Frame.Width * formsPage.TabIndex, 0, layout.Frame.Width, layout.Frame.Height - tabsHolder.Frame.Height));
                }
            }

            //Console.WriteLine(layout.Frame.Height);

            this.Element.Height.ToString();
            this.View.ToString();

            if (!manualScroll)
            {
                //Forms pages layout on orientation change
                foreach (var formsPage in (this.Element as RSTabbedPage).Children)
                {
                    formsPage.Layout(new Rectangle(layout.Frame.Width * formsPage.TabIndex, 0, layout.Frame.Width, layout.Frame.Height - tabsHolder.Frame.Height));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(pageScrollView != null)
            {
                pageScrollView.Scrolled -= PageScrollView_Scrolled;
                pageScrollView.DraggingStarted -= PageScrollView_DraggingStarted;
                pageScrollView.DecelerationEnded -= PageScrollView_DecelerationEnded;
                pageScrollView.ScrollAnimationEnded -= PageScrollView_ScrollAnimationEnded;


                //ContenSize change observer remove
                pageScrollView.RemoveObserver(this, (NSString)"contentSize", tokenObserveContentSize);
            }

            if(this.Element != null)
            {
                foreach (var formsPage in (this.Element as RSTabbedPage).Children)
                {
                    formsPage.SizeChanged -= FormsPage_SizeChanged;
                }

            (this.Element as RSTabbedPage).CurrentPageChanged -= RSTabbedPageRenderer_CurrentPageChanged;
            }
        }
    }

    public class TabsTitle : UILabel
    {
        private UIEdgeInsets EdgeInsets { get; set; } = new UIEdgeInsets(0, 20, 0, 20);
        private UIEdgeInsets InverseEdgeInsets = new UIEdgeInsets(0, -20, 0, -20);


        public override void DrawText(CoreGraphics.CGRect rect)
        {
            base.DrawText(EdgeInsets.InsetRect(rect));
        }

        public override CoreGraphics.CGRect TextRectForBounds(CoreGraphics.CGRect bounds, nint numberOfLines)
        {
            var textRect = base.TextRectForBounds(EdgeInsets.InsetRect(bounds), numberOfLines);
            return InverseEdgeInsets.InsetRect(textRect);
        }
    }


    public class CustomView : UIView
    {
        public CustomView()
        {
        }

        public override CGRect Frame { get => base.Frame; set => base.Frame = value; }

        public override CGRect Bounds { get => base.Bounds; set => base.Bounds = value; }

        public override CGSize IntrinsicContentSize => base.IntrinsicContentSize;
    }

    public class CustomCollectionView : UICollectionView
    {
        public CustomCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
        {
            this.PagingEnabled = true;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                this.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }
        }
    }

    public class CustomCollectionViewDataSource : UICollectionViewDataSource
    {
        RSTabbedPage rSTabbedPage;

        public CustomCollectionViewDataSource(RSTabbedPage rSTabbedPage)
        {
            this.rSTabbedPage = rSTabbedPage;
        }



        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            CustomUICollectionViewCell cell = (CustomUICollectionViewCell)collectionView.DequeueReusableCell("CellId", indexPath);
            cell.BackgroundColor = indexPath.Row % 2 == 0 ? UIColor.Blue : UIColor.Yellow;
            var title = new UILabel();
            title.Text = this.rSTabbedPage.Children.ElementAt(indexPath.Row).Title;
            title.Frame = new CGRect(0, 0, cell.ContentView.Frame.Width, cell.ContentView.Frame.Height);
            cell.ContentView.AddSubview(title);

            var title2 = new UILabel();
            title2.Text = this.rSTabbedPage.Children.ElementAt(indexPath.Row).Title;
            title2.Frame = new CGRect(0, 30, cell.ContentView.Frame.Width, cell.ContentView.Frame.Height);
            cell.ContentView.AddSubview(title2);

            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return this.rSTabbedPage.Children.Count();
        }
    }

    public class CustomUICollectionViewFlowLayout : UICollectionViewFlowLayout
    {
        public CustomUICollectionViewFlowLayout()
        {
            this.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
            this.MinimumLineSpacing = 0;
        }

        public override UICollectionViewLayoutInvalidationContext GetInvalidationContextForBoundsChange(CGRect newBounds)
        {
            var context = (UICollectionViewFlowLayoutInvalidationContext)base.GetInvalidationContextForBoundsChange(newBounds);

            context.InvalidateFlowLayoutDelegateMetrics = newBounds != CollectionView.Bounds;

            return context;
        }

        public override CGSize ItemSize { get => new CGSize(this.CollectionView.Frame.Width / 3, this.CollectionView.Frame.Height); set => base.ItemSize = value; }
    }

    public class CustomUICollectionViewCell : UICollectionViewCell
    {
        public UIView pageView;

        public CustomUICollectionViewCell(CGRect frame, UIView pageView) : base(frame)
        {
            //this.ContentView.Add(pageView);
        }

        public CustomUICollectionViewCell(IntPtr intPtr) : base(intPtr)
        {
            //this.ContentView.Add(pageView);
        }
    }
}
