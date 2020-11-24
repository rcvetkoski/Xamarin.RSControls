using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSTabbedPage), typeof(RSTabbedPageRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSTabbedPageRenderer : PageRenderer
    {
        private CustomCollectionView collectionView;
        private UIView layout;
        private UIScrollView pageScrollView;
        private UIView tabsContent;
        private UIView tabsSlider;
        private CustomView container;
        private int tabsScrollHeight;
        private int pagesCount;
        private NSLayoutConstraint tabSliderWidthConstraint;
        private NSLayoutConstraint tabSliderXOffsetConstraint;
        private NSLayoutConstraint containerWidthConstraint;
        private NSLayoutConstraint containerHeightConstraint;
        private NSLayoutConstraint[] constraintArray;
        private nfloat tempWidth;
        private bool manualScroll = true;


        public RSTabbedPageRenderer()
        {
            tabsScrollHeight = 60;
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            //tempWidth is used to check if screen orientation is changing so it will trigger auto scroll function
            tempWidth = UIScreen.MainScreen.Bounds.Width;

            (this.Element as RSTabbedPage).CurrentPageChanged += RSTabbedPageRenderer_CurrentPageChanged;

            //Adjust forms pages to new screen size / substract menu height
            foreach (var item in (this.Element as RSTabbedPage).Children)
                item.SizeChanged += Item_SizeChanged;


            //Main view
            layout = new UIView();
            //layout = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height));
            
            //Tabs menu
            SetTabBar();

            //Pages Scroll
            SetPages();


            //Remove pages so we can add our new page holder wich is using scroll paging
            foreach (UIView view in this.View.Subviews)
            {
                view.RemoveFromSuperview();
            }

            //Add new page holder
            this.View.AddSubview(layout);
            layout.BackgroundColor = UIColor.Brown;
            layout.TranslatesAutoresizingMaskIntoConstraints = false;
            layout.LeadingAnchor.ConstraintEqualTo(this.View.LeadingAnchor).Active = true;
            layout.TopAnchor.ConstraintEqualTo(this.View.TopAnchor).Active = true;
            layout.TrailingAnchor.ConstraintEqualTo(this.View.TrailingAnchor).Active = true;
            layout.BottomAnchor.ConstraintEqualTo(this.View.BottomAnchor).Active = true;
        }


        private void Item_SizeChanged(object sender, EventArgs e)
        {
            //Adjusrt forms pages to new screen size / substract menu height
            foreach (var item in (this.Element as RSTabbedPage).Children)
                item.Layout(new Rectangle(0, 0, layout.Bounds.Width, layout.Frame.Height - tabsScrollHeight));

            Console.WriteLine(layout.Frame.Height);
        }

        private void RSTabbedPageRenderer_CurrentPageChanged(object sender, EventArgs e)
        {
            
        }

        //Set tabs menu
        private void SetTabBar()
        {
            //Tabs holder
            tabsContent = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, tabsScrollHeight));
            tabsContent.BackgroundColor = UIColor.Red;
            layout.AddSubview(tabsContent);

            //Add tabs
            nfloat tabsOffsetX = 0;
            foreach (var item in (this.Element as RSTabbedPage).Children)
            {
                UIStackView tab = new UIStackView() { BackgroundColor = UIColor.White };
                tab.Alignment = UIStackViewAlignment.Center;
                tab.Axis = UILayoutConstraintAxis.Vertical;
                //tab.Spacing = 1;
                //tab.LayoutMargins = new UIEdgeInsets(5, 5, 5, 5);
                //tab.LayoutMarginsRelativeArrangement = true;
                tabsContent.AddSubview(tab);

                var currentPage = (this.Element as RSTabbedPage).Children.IndexOf(item);

                UITapGestureRecognizer tabTap = new UITapGestureRecognizer(() => {
                    manualScroll = true;
                    var posX = (UIScreen.MainScreen.Bounds.Width * currentPage);
                    (this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt((int)currentPage);
                    pageScrollView.SetContentOffset(new CGPoint(posX, pageScrollView.ContentOffset.Y), true);
                });
                tab.AddGestureRecognizer(tabTap);


                UILabel title = new UILabel() { TextAlignment = UITextAlignment.Center };
                title.Frame = new CGRect(tabsOffsetX, 0, UIScreen.MainScreen.Bounds.Width / (this.Element as RSTabbedPage).Children.Count, tabsScrollHeight);
                title.Text = item.Title;
                tabsOffsetX += UIScreen.MainScreen.Bounds.Width / (this.Element as RSTabbedPage).Children.Count;


                if(item.IconImageSource != null)
                {
                    var toBeSearched = "File: ";
                    string path = item.IconImageSource.ToString().Substring(item.IconImageSource.ToString().IndexOf(toBeSearched) + toBeSearched.Length);
                    RSSvgImage svgIcon = new RSSvgImage() { Source = path, Color = Color.Black };
                    var icon = Extensions.ViewExtensions.ConvertFormsToNative(svgIcon, new CGRect(0, 0, 25, 25));

                    icon.TranslatesAutoresizingMaskIntoConstraints = false;
                    icon.WidthAnchor.ConstraintEqualTo(25).Active = true;
                    icon.HeightAnchor.ConstraintEqualTo(25).Active = true;
                    //icon.BackgroundColor = UIColor.SystemPinkColor;
                    tab.AddArrangedSubview(icon);
                }

                tab.AddArrangedSubview(title);

            }

            //Tabs slider
            tabsSlider = new UIView();
            tabsSlider.BackgroundColor = UIColor.Blue;
            tabsContent.AddSubview(tabsSlider);
            tabsSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            tabSliderWidthConstraint = tabsSlider.WidthAnchor.ConstraintEqualTo(UIScreen.MainScreen.Bounds.Width / (this.Element as RSTabbedPage).Children.Count);
            tabSliderWidthConstraint.Active = true;
            tabsSlider.HeightAnchor.ConstraintEqualTo(3).Active = true;
            tabSliderXOffsetConstraint = tabsSlider.LeadingAnchor.ConstraintEqualTo(tabsContent.LeadingAnchor);
            tabSliderXOffsetConstraint.Active = true;
            tabsSlider.BottomAnchor.ConstraintEqualTo(tabsContent.BottomAnchor).Active = true;
        }

        //Set pages scroll
        private void SetPages()
        {
            pagesCount = (this.Element as RSTabbedPage).Children.Count;

            //Page scroll
            pageScrollView = new UIScrollView(new CGRect(0, tabsScrollHeight, layout.Bounds.Width * pagesCount, layout.Bounds.Height - tabsScrollHeight));
            layout.AddSubview(pageScrollView);
            pageScrollView.ContentSize = new CGSize(layout.Bounds.Width * pagesCount, layout.Bounds.Height - tabsScrollHeight);
            pageScrollView.Scrolled += PageScrollView_Scrolled;
            pageScrollView.DraggingStarted += PageScrollView_DraggingStarted;
            pageScrollView.Bounces = false;
            pageScrollView.PagingEnabled = true;


            nfloat offsetX = 0;
            container = new CustomView();

            //Save constraints so that we can update them later
            constraintArray = new NSLayoutConstraint[pagesCount];

            //Add pages
            int i = 0;
            foreach (var item in this.View.Subviews)
            {
                var nativeView = item.Subviews[0];
                container.AddSubview(nativeView);
                nativeView.TranslatesAutoresizingMaskIntoConstraints = false;
                var containerLeadingConstraint = nativeView.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor, offsetX);
                containerLeadingConstraint.Active = true;
                constraintArray[i] = containerLeadingConstraint;
                nativeView.WidthAnchor.ConstraintEqualTo(container.WidthAnchor, (nfloat)1 / (nfloat)pagesCount).Active = true;
                nativeView.HeightAnchor.ConstraintEqualTo(container.HeightAnchor).Active = true;
                offsetX += UIScreen.MainScreen.Bounds.Width;
                i++;
            }


            //container constraints
            pageScrollView.AddSubview(container);
            container.TranslatesAutoresizingMaskIntoConstraints = false;
            container.LeadingAnchor.ConstraintEqualTo(pageScrollView.LeadingAnchor).Active = true;
            containerWidthConstraint = container.WidthAnchor.ConstraintEqualTo(layout.Bounds.Width * pagesCount);
            containerWidthConstraint.Active = true;
            containerHeightConstraint = container.HeightAnchor.ConstraintEqualTo(layout.Bounds.Height - tabsScrollHeight);
            containerHeightConstraint.Active = true;
        }

        //Used to tell that scroll is manualy started and no when screen size changes
        private void PageScrollView_DraggingStarted(object sender, EventArgs e)
        {
            manualScroll = true;
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
                tabSliderXOffsetConstraint.Constant = (sender as UIScrollView).ContentOffset.X / pagesCount;
                var indexOfPage = Math.Round((pageScrollView.ContentOffset.X / UIScreen.MainScreen.Bounds.Width), MidpointRounding.AwayFromZero);
                (this.Element as RSTabbedPage).CurrentPage = (this.Element as RSTabbedPage).Children.ElementAt((int)indexOfPage);
            }
        }
        
        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();


            //Adjusrt forms pages to new screen size / substract menu height
            foreach (var item in (this.Element as RSTabbedPage).Children)
                item.Layout(new Rectangle(0, 0, layout.Bounds.Width, layout.Bounds.Height - tabsScrollHeight));

            //Tabs
            tabsContent.Frame = new CGRect(0, 0, layout.Bounds.Width, tabsScrollHeight);
            tabSliderWidthConstraint.Constant = layout.Bounds.Width / (this.Element as RSTabbedPage).Children.Count;
            containerWidthConstraint.Constant = layout.Bounds.Width * pagesCount;
            containerHeightConstraint.Constant = layout.Bounds.Height - tabsScrollHeight;
            nfloat tabsOffsetX = 0;
            foreach (var item in tabsContent.Subviews)
            {
                item.Frame = new CGRect(tabsOffsetX, 0, layout.Bounds.Width / (this.Element as RSTabbedPage).Children.Count, tabsScrollHeight);
                tabsOffsetX += layout.Bounds.Width / (this.Element as RSTabbedPage).Children.Count;
            }

            //Pages
            nfloat offsetX = 0;
            for (int i = 0; i < constraintArray.Count(); i++)
            {
                var constraint = constraintArray[i];
                constraint.Constant = offsetX;
                offsetX += UIScreen.MainScreen.Bounds.Width;
            }
            pageScrollView.Frame = new CGRect(0, tabsScrollHeight, layout.Bounds.Width, layout.Bounds.Height - tabsScrollHeight);
            pageScrollView.ContentSize = new CGSize(layout.Bounds.Width * pagesCount, layout.Bounds.Height - tabsScrollHeight);


            //Update scroll position if needed
            var currentPage = (this.Element as RSTabbedPage).Children.IndexOf((this.Element as RSTabbedPage).CurrentPage);
            if(!manualScroll)
            {
                pageScrollView.SetContentOffset(new CGPoint(layout.Bounds.Width * currentPage, pageScrollView.ContentOffset.Y), true);
                tabSliderXOffsetConstraint.Constant = (layout.Bounds.Width * currentPage) / pagesCount;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            pageScrollView.Scrolled -= PageScrollView_Scrolled;
            pageScrollView.DraggingStarted -= PageScrollView_DraggingStarted;
            (this.Element as RSTabbedPage).CurrentPageChanged -= RSTabbedPageRenderer_CurrentPageChanged;

            if(this.Element != null)
            {
                foreach (var item in (this.Element as RSTabbedPage).Children)
                    item.SizeChanged -= Item_SizeChanged;
            }
        }


        private void SetCollection()
        {
            var flowLayout = new CustomUICollectionViewFlowLayout();
            collectionView = new CustomCollectionView(new CGRect(0, 0, 0, 0), flowLayout);
            var data = new CustomCollectionViewDataSource(this.View);
            collectionView.DataSource = data;
            collectionView.RegisterClassForCell(typeof(CustomUICollectionViewCell), "CellId");
            collectionView.BackgroundColor = UIColor.Yellow;
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
        }
    }

    public class CustomCollectionViewDataSource : UICollectionViewDataSource
    {
        public CustomCollectionViewDataSource(UIView view)
        {
            Element = view;
        }

        public UIView Element;


        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            CustomUICollectionViewCell cell = (CustomUICollectionViewCell)collectionView.DequeueReusableCell("CellId", indexPath);
            //cell.pageView = Element;
            cell.BackgroundColor = indexPath.Row % 2 == 0 ? UIColor.Blue : UIColor.Yellow;
            var item = Element.Subviews[indexPath.Row];
            cell.ContentView.AddSubview(item);
            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return Element.Subviews.Count();
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

        public override CGSize ItemSize { get => new CGSize(this.CollectionView.Frame.Width, this.CollectionView.Frame.Height); set => base.ItemSize = value; }
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
