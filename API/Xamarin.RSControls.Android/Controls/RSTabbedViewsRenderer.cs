using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;
using Xamarin.RSControls.Helpers;

[assembly: ExportRenderer(typeof(RSTabbedViews), typeof(RSTabbedViewsRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSTabbedViewsRenderer : ViewRenderer<RSTabbedViews, global::Android.Views.View>
    {
        private List<global::Android.Views.View> pages;
        private LinearLayout nativeView;
        private TabLayout menuBar;


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
                nativeView.SetBackgroundColor(global::Android.Graphics.Color.Pink);

                
                menuBar = new TabLayout(Context);
                menuBar.SetSelectedTabIndicatorColor(global::Android.Graphics.Color.White);
                menuBar.TabMode = TabLayout.ModeScrollable;



                menuBar.SetBackgroundColor(global::Android.Graphics.Color.Blue);
                menuBar.SetMinimumHeight(60);

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

                ViewPager pager = new ViewPager(Context);
                pager.Adapter = new pageAdapter(Context, pages);
                pager.AddOnPageChangeListener(pager.Adapter as pageAdapter);
                pager.SetBackgroundColor(global::Android.Graphics.Color.Red);
                pager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(menuBar));


                nativeView.AddView(pager);


                this.SetNativeControl(nativeView);
            }
        }

        private void Element_SizeChanged(object sender, EventArgs e)
        {
            foreach (var formsView in this.Element.Views)
            {
                formsView.Layout(new Rectangle(0, 0, this.Element.Width, this.Element.Height));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.Element.SizeChanged -= Element_SizeChanged;
        }





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
}