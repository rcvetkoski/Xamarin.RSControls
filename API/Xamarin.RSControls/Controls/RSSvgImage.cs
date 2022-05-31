using System;
using System.IO;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Linq;
using System.Reflection;
using Xamarin.Essentials;
using System.Runtime.CompilerServices;

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
            var lololo = AppDomain.CurrentDomain.GetAssemblies();
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



    public class SvgHelper
    {
        public static ImageSource GetAsImageSource(string Source, VisualElement holder, Color color)
        {
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var scaleFactor = mainDisplayInfo.Density;

            var assemblyName = Source.Substring(0, Source.IndexOf("/", StringComparison.CurrentCulture));
            var lololo = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);


            using (Stream stream = assembly.GetManifestResourceStream(Source.Replace("/", ".")))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);


                var width = holder.WidthRequest != 0 ? holder.WidthRequest : holder.Width;
                var height = holder.HeightRequest != 0 ? holder.HeightRequest : holder.Height;


                var svgSize = svg.Picture.CullRect;
                var svgMax = Math.Max(svgSize.Width, svgSize.Height);
                float canvasMin = Math.Min((int)(width * scaleFactor), (int)(height * scaleFactor));
                var scale = canvasMin / svgMax;
                var matrix = SKMatrix.CreateScale(scale, scale);

                var bitmap = new SKBitmap((int)(width * scaleFactor), (int)(height * scaleFactor));

                var paint = new SKPaint()
                {
                    ColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.SrcIn)
                };

                var canvas = new SKCanvas(bitmap);
                canvas.DrawPicture(svg.Picture, ref matrix, paint);

                var image = SKImage.FromBitmap(bitmap);
                var encoded = image.Encode();
                var stream1 = encoded.AsStream();
                var source = ImageSource.FromStream(() => stream1);

                return source;
            }
        }
    }

    public class DrawImageExtension : Forms.Xaml.IMarkupExtension
    {
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var source = SvgHelper.GetAsImageSource(FileName, Holder, Color);
            return source;
        }

        public VisualElement Holder { get; set; }   
        public string FileName { get; set; }
        //public float Width { get; set; }
        //public float Height { get; set; }
        public Color Color { get; set; }
    }

    public class RSImageSource : ImageSource
    {
        protected override void OnParentSet()
        {
            base.OnParentSet();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SourceProperty.PropertyName)
            {
                OnSourceChanged();
                OnLoadingStarted();
                RSSvgToImageSource(Source, 40, 40, Color.Red);
                OnLoadingCompleted(false);
            }
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(RSImageSource), default(string));
        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }


        public RSImageSource()
        {
            
        }


        public Stream RSSvgToImageSource (string Source, float width, float height, Color color)
        {
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var scaleFactor = mainDisplayInfo.Density;

            var assemblyName = Source.Substring(0, Source.IndexOf("/", StringComparison.CurrentCulture));
            var lololo = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);

            using (Stream stream = assembly.GetManifestResourceStream(Source.Replace("/", ".")))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);


                var svgSize = svg.Picture.CullRect;
                var svgMax = Math.Max(svgSize.Width, svgSize.Height);
                float canvasMin = Math.Min((int)(width * scaleFactor), (int)(height * scaleFactor));
                var scale = canvasMin / svgMax;
                var matrix = SKMatrix.CreateScale(scale, scale);

                var bitmap = new SKBitmap((int)(width * scaleFactor), (int)(height * scaleFactor));

                var paint = new SKPaint()
                {
                    ColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.SrcIn)
                };

                var canvas = new SKCanvas(bitmap);
                canvas.DrawPicture(svg.Picture, ref matrix, paint);

                var image = SKImage.FromBitmap(bitmap);
                var encoded = image.Encode();
                var stream1 = encoded.AsStream();
                //var source = ImageSource.FromStream(() => stream1);
                //return source;

                return stream1;
            }
        }
    }
}
