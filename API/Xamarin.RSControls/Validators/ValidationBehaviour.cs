using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.RSControls.Helpers;

namespace Xamarin.RSControls.Validators
{
    public class ValidationBehaviour : Behavior<View>
    {
        public string PropertyName { get; set; }

        public IList<IValidation> Validators { get; set; }

        public ValidationBehaviour()
        {
            Validators = new List<IValidation>();
        }

        private void Validate(View view)
        {
            IRSControl rsControl = view as IRSControl;

            Layout<View> parent = view.Parent as Layout<View>;


            if (rsControl != null)
            {
                foreach (IValidation validation in Validators)
                {
                    bool isValid = false;
                    var value = rsControl.GetType().GetProperty(PropertyName).GetValue(rsControl);

                    if (value != null)
                        isValid = validation.Validate(value.ToString());
                    else
                        isValid = false;


                    if (!isValid)
                    {
                        view.PropertyChanged -= View_PropertyChanged;

                        rsControl.TextColor = Color.Red;
                        rsControl.BorderColor = Color.Red;

                        view.HeightRequest = view.Height * 2;

                        //StackLayout stackLayout = new StackLayout();
                        //Label labelMessage = new Label() { Text = "Test", TextColor = Color.Red };
                        //stackLayout.Children.Add(view);
                        //stackLayout.Children.Add(labelMessage);

                        //var index = parent.Children.IndexOf(view);
                        //parent.Children.Remove(view);
                        //parent.Children.Insert(index, stackLayout);

                        view.PropertyChanged += View_PropertyChanged;

                        return;
                    }
                }

                rsControl.TextColor = Color.Black;
                rsControl.BorderColor = Color.Black;
            }
        }

        protected override void OnAttachedTo(View view)
        {
            view.PropertyChanged += View_PropertyChanged;
            base.OnAttachedTo(view);
        }

        private void View_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == this.PropertyName)
            {
                Validate(sender as View);
            }
        }

        protected override void OnDetachingFrom(View view)
        {
            view.PropertyChanged -= View_PropertyChanged;
            base.OnDetachingFrom(view);
        }
    }
}
