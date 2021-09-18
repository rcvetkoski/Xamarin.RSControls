using System;
using System.IO;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Linq;
using System.Reflection;

namespace Xamarin.RSControls.Controls
{
    public class RSSvgImage : SKCanvasView
    {
        public RSSvgImage()
        {
            //Default value for this inherited properties
            this.WidthRequest = 22;
            this.HeightRequest = 22;
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(RSSvgImage), default(string), propertyChanged: OnSourcePropertyChanged);
        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(RSSvgImage), Color.DimGray);
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static void OnSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            RSSvgImage svg = (RSSvgImage)bindable;
            svg.InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            
            if (string.IsNullOrEmpty(Source))
                return;

            var assemblyName = Source.Substring(0, Source.IndexOf("/", StringComparison.CurrentCulture));
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);

            using (Stream stream = assembly.GetManifestResourceStream(Source.Replace("/", ".")))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);
                SKImageInfo info = e.Info;
                SKRect bounds = svg.ViewBox;

                float xRatio = info.Width / bounds.Width;
                float yRatio = info.Height / bounds.Height;
                float ratio = Math.Min(xRatio, yRatio);

                canvas.Scale(ratio);
                if (Color != Color.Transparent)
                {
                    SKPaint sKPaint = new SKPaint();
                    sKPaint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
                    canvas.DrawPicture(svg.Picture, 0, 0, sKPaint);
                }
                else
                    canvas.DrawPicture(svg.Picture, 0, 0);
            }
        }
    }
}
