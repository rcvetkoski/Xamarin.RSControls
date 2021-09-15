using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Helpers;

[assembly: ExportRenderer(typeof(RSTabbedViews), typeof(RSTabbedViewsRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSTabbedViewsRenderer : ViewRenderer<RSTabbedViews, global::Android.Views.View>, TabLayout.IOnTabSelectedListener
    {
        private List<global::Android.Views.View> pages;
        private ViewPager pager;
        private LinearLayout nativeView;
        private CustomTabLayout menuBar;
        private bool doOnce = false;


        public RSTabbedViewsRenderer(Context context) : base(context)
        {
            pages = new List<global::Android.Views.View>();
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
                nativeView = new LinearLayout(Context) { Orientation = Orientation.Vertical };
                nativeView.LayoutChange += NativeView_LayoutChange;

                menuBar = new CustomTabLayout(Context);
                menuBar.TabGravity = CustomTabLayout.GravityFill;
                menuBar.TabMode = CustomTabLayout.ModeScrollable;
                menuBar.SetSelectedTabIndicatorColor(this.Element.SliderColor.ToAndroid());
                menuBar.SetBackgroundColor(this.Element.BarColor.ToAndroid());
                menuBar.SetTabTextColors(menuBar.TabTextColors.DefaultColor, this.Element.BarTextColor.ToAndroid());
                menuBar.AddOnTabSelectedListener(this);
                nativeView.AddView(menuBar);


                foreach (var formsView in this.Element.Views)
                {
                    Page currentPage = TypeExtensions.GetParentPage(this.Element);
                    formsView.Parent = currentPage;
                    var renderer = Platform.CreateRendererWithContext(formsView, Context);
                    var natView = renderer.View;
                    pages.Add(natView);

                    var title = formsView.GetValue(RSTabbedViews.TitleProperty).ToString();

                    menuBar.AddView(new TabItem(Context) { Text = new Java.Lang.String(formsView.GetValue(RSTabbedViews.TitleProperty).ToString()) });
                }
                this.Element.SizeChanged += Element_SizeChanged;


                pager = new ViewPager(Context);
                pager.Adapter = new pageAdapter(Context, pages);
                pager.AddOnPageChangeListener(pager.Adapter as pageAdapter);
                pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(menuBar));
                nativeView.AddView(pager);

                this.SetNativeControl(nativeView);
            }
        }

        private void NativeView_LayoutChange(object sender, LayoutChangeEventArgs e)
        {
            if(!doOnce)
            {
                foreach (var formsView in this.Element.Views)
                {
                    double menuBarHeightToFormsUnit = ContextExtensions.FromPixels(this.Context, menuBar.Height);
                    formsView.Layout(new Rectangle(0, 0, this.Element.Width, this.Element.Height - menuBarHeightToFormsUnit));
                }

                doOnce = true;
            }
        }

        private void Element_SizeChanged(object sender, EventArgs e)
        {
            foreach (var formsView in this.Element.Views)
            {
                double menuBarHeightToFormsUnit = ContextExtensions.FromPixels(this.Context, menuBar.Height);
                formsView.Layout(new Rectangle(0, 0, this.Element.Width, this.Element.Height - menuBarHeightToFormsUnit));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(this.Element != null)
                this.Element.SizeChanged -= Element_SizeChanged;

            if(nativeView != null)
                nativeView.LayoutChange -= NativeView_LayoutChange;

            base.Dispose(disposing);
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {
            
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            if(pager != null)
                pager.SetCurrentItem(tab.Position, true);
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            
        }


        //PagerAdapter
        private class pageAdapter : PagerAdapter, IOnClickListener, ViewPager.IOnPageChangeListener
        {
            private Context context;
            private List<global::Android.Views.View> pages;

            public pageAdapter(Context context, List<global::Android.Views.View> pages)
            {
                this.context = context;
                this.pages = pages;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                pages.ElementAt(position).LayoutParameters = new ViewGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                container.AddView(pages.ElementAt(position));
                return pages.ElementAt(position);
            }

            public override int Count => pages.Count;

            public override bool IsViewFromObject(global::Android.Views.View view, Java.Lang.Object @object)
            {
                return view == @object;
            }

            public void OnClick(global::Android.Views.View v)
            {
                //throw new NotImplementedException();
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                //throw new NotImplementedException();
            }

            public void OnPageScrollStateChanged(int state)
            {
                //throw new NotImplementedException();
            }

            public void OnPageSelected(int position)
            {
                //throw new NotImplementedException();
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
            {
                container.RemoveView(@object as global::Android.Views.View);
            }
        }
    }


    public class CustomTabLayout : TabLayout
    {
        public CustomTabLayout(Context context) : base(context) { }

        public CustomTabLayout(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public CustomTabLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            ViewGroup tabLayout = (ViewGroup)GetChildAt(0);

            int childCount = tabLayout.ChildCount;

            if (childCount != 0)
            {
                DisplayMetrics displayMetrics = Context.Resources.DisplayMetrics;
                int tabMinWidth = displayMetrics.WidthPixels / childCount;

                for (int i = 0; i < childCount; ++i)
                {
                    tabLayout.GetChildAt(i).SetMinimumWidth(tabMinWidth);
                }
            }

            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

        }
    }
}