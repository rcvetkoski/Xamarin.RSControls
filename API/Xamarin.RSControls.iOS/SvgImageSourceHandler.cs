using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.RSControls.Controls;

namespace Xamarin.RSControls.iOS
{
    public  class SvgImageSourceHandler : IImageSourceHandler
    {
        /// <summary>
        /// Loads the image async.
        /// </summary>
        /// <returns>The image async.</returns>
        /// <param name="imagesource">Imagesource.</param>
        /// <param name="cancelationToken">Cancelation token.</param>
        /// <param name="scale">Scale.</param>
        public async Task<UIImage> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
        {
            var svgImageSource = imagesource as RSImageSource;

            using (var stream = await svgImageSource.RSSvgToImageSource())
            {
                if (stream == null)
                {
                    return null;
                }
                return UIImage.LoadFromData(NSData.FromStream(stream), (nfloat)svgImageSource.Scale);
            }
        }
    }
}
