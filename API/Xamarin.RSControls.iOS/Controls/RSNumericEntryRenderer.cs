using System;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.iOS.Controls;

[assembly: ExportRenderer(typeof(RSNumericEntry), typeof(RSNumericEntryRenderer))]
namespace Xamarin.RSControls.iOS.Controls
{
    public class RSNumericEntryRenderer : RSEntryRenderer
    {
        public RSNumericEntryRenderer()
        {
        }
    }
}
