using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.RSControls.Controls;

namespace Xamarin.RSControls.Droid
{
    public class SvgImageSourceHandler : IImageSourceHandler
    {
        /// <summary>
        /// Loads the image async.
        /// </summary>
        /// <returns>The image async.</returns>
        /// <param name="imagesource">Imagesource.</param>
        /// <param name="context">Context.</param>
        /// <param name="cancelationToken">Cancelation token.</param>
        public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
        {
            var svgImageSource = imagesource as RSImageSource;

            using (var stream = await svgImageSource.RSSvgToImageSource())
            {
                if (stream == null)
                {
                    return null;
                }
                return await BitmapFactory.DecodeStreamAsync(stream);
            }
        }
    }
}