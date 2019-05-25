using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.RSControls.Controls;

namespace Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
            this.BindingContext = new Page1ViewModel();

            for (int i = 0; i < 1; i++)
            {
                RSNumericEntryInputLayout rSNumericEntryInputLayout = new RSNumericEntryInputLayout() { CustomUnit = "KG", Value = "i", Placeholder = i.ToString() };

                stack.Children.Add(rSNumericEntryInputLayout);
            }
        }
    }
}