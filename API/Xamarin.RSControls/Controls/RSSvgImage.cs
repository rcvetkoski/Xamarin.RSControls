using System;
using System.IO;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Linq;
using System.Reflection;
using Xamarin.Essentials;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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


                //// Calculate matrix scale
                //float xRatio = (info.Width * (float)Scale) / svg.Picture.CullRect.Size.Width;
                //float yRatio = (info.Height * (float)Scale) / svg.Picture.CullRect.Size.Height;
                //var matrix = SKMatrix.CreateScale(xRatio, yRatio);

                canvas.Scale(ratio);
                if (Color != Color.Transparent)
                {
                    SKPaint sKPaint = new SKPaint();
                    sKPaint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn);
                    canvas.DrawPicture(svg.Picture, 0, 0, sKPaint);
                    //canvas.DrawPicture(svg.Picture, ref matrix, sKPaint);
                }
                else
                    canvas.DrawPicture(svg.Picture, 0, 0);
            }
        }
    }


    public class DrawImageExtension : Forms.Xaml.IMarkupExtension
    {
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var source = GetAsImageSource(FileName, Holder, Color);
            return source;
        }

        private static ImageSource GetAsImageSource(string Source, VisualElement holder, Color color)
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

        public VisualElement Holder { get; set; }   
        public string FileName { get; set; }
        //public float Width { get; set; }
        //public float Height { get; set; }
        public Color Color { get; set; }
    }









    /// <summary>
    /// Add Xamarin.Forms.Internals.Registrar.Registered.Register(typeof(Xamarin.RSControls.Controls.RSImageSource), typeof(SvgImageSourceHandler)); to MainActivity.cs and AppDelegate.cs
    /// </summary>
    public class RSImageSource : ImageSource
    {
        // Image Color
        public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(RSImageSource), Color.Gray);
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Source path must be embeded ressource
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(RSImageSource), default(string));
        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        // Image scale per density of screen
        public double Scale { get; set; }

        // Width
        public static readonly BindableProperty WidthProperty = BindableProperty.Create("Width", typeof(double), typeof(RSImageSource), default(double));
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Height
        public static readonly BindableProperty HeightProperty = BindableProperty.Create("Height", typeof(double), typeof(RSImageSource), default(double));
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }


        // If Source changes reload Image generation
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SourceProperty.PropertyName)
            {
                OnSourceChanged();
                OnLoadingStarted();
                RSSvgToImageSource();
                OnLoadingCompleted(false);
            }
        }

        // Svg to ImageSource method
        public Task<Stream> RSSvgToImageSource ()
        {
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            Scale = mainDisplayInfo.Density;

            var assemblyName = Source.Substring(0, Source.IndexOf("/", StringComparison.CurrentCulture));
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);

            using (Stream stream = assembly.GetManifestResourceStream(Source.Replace("/", ".")))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);
                var size = CalcSize(svg.Picture.CullRect.Size, Width, Height);

                // Calculate matrix scale
                float xRatio = (size.Width * (float)Scale) / svg.Picture.CullRect.Size.Width;
                float yRatio = (size.Height * (float)Scale) / svg.Picture.CullRect.Size.Height;
                var matrix = SKMatrix.CreateScale(xRatio, yRatio);

                // Create SKBitmap
                var bitmap = new SKBitmap((int)(size.Width * Scale), (int)(size.Height * Scale));
                var paint = new SKPaint()
                {
                    ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn)
                };
                var canvas = new SKCanvas(bitmap);
                canvas.DrawPicture(svg.Picture, ref matrix, paint);
                var image = SKImage.FromBitmap(bitmap);
                var encoded = image.Encode();
                var stream1 = encoded.AsStream();

                return Task.FromResult(stream1);
            }
        }

        public static SKSize CalcSize(SkiaSharp.SKSize size, double width, double height)
        {
            double w;
            double h;

            if (width <= 0 && height <= 0)
            {
                return size;
            }
            else if (width <= 0)
            {
                h = height;
                w = height * (size.Width / size.Height);
            }
            else if (height <= 0)
            {
                w = width;
                h = width * (size.Height / size.Width);
            }
            else
            {
                w = width;
                h = height;
            }

            return new SkiaSharp.SKSize((float)w, (float)h);
        }
    }
}
