using System;
using Android.Content;
using Google.Android.Material.Tabs;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Droid.Controls;

[assembly: ExportRenderer(typeof(RSTabbedPage), typeof(RSTabbedPageRenderer))]
namespace Xamarin.RSControls.Droid.Controls
{
    public class RSTabbedPageRenderer : Xamarin.Forms.Platform.Android.AppCompat.TabbedPageRenderer
    {
        public RSTabbedPageRenderer(Context context) : base(context)
        {
            this.ViewGroup.ToString();



        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {
                //for (int i = 0; i < this.ViewGroup.ChildCount; i++)
                //{
                //    var child = this.ViewGroup.GetChildAt(i);
                //    child.ToString();
                //    for (int y = 0; y < (child as global::Android.Views.ViewGroup).ChildCount; y++)
                //    {
                //        var child2 = this.ViewGroup.GetChildAt(i);
                //        child2.ToString();
                //    }
                //}

                for (int i = 0; i < this.ViewGroup.ChildCount; i++)
                {
                    var child = this.ViewGroup.GetChildAt(i);
                    if(child is TabLayout)
                    {
                        var tabLayout = child as TabLayout;

                        tabLayout.LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent); 
                        tabLayout.TabGravity = TabLayout.GravityFill;
                        tabLayout.TabMode = TabLayout.ModeScrollable;

                        //tabLayout.Measure(2000, 2000);
                        //var lol = tabLayout.MeasuredWidth;

                        //var llll = Context.Resources.DisplayMetrics.WidthPixels;

                        //if (lol < llll)
                        //    tabLayout.TabMode = TabLayout.ModeFixed;
                        //else
                        //    tabLayout.TabMode = TabLayout.ModeScrollable;

                    }

                }
            }
        }

        //public override void OnViewAdded(global::Android.Views.View child)
        //{
        //    base.OnViewAdded(child);
        //    var tabLayout = child as TabLayout;
        //    if (tabLayout != null)
        //    {
        //        tabLayout.SetBackgroundColor(Color.Red.ToAndroid());
        //        tabLayout.TabGravity = TabLayout.GravityCenter;
        //        tabLayout.TabMode = TabLayout.ModeScrollable;
        //        tabLayout.ChildViewAdded += TabLayout_ChildViewAdded;
        //    }
        //}

        //private void TabLayout_ChildViewAdded(object sender, ChildViewAddedEventArgs e)
        //{
            
        //}
    }
}
