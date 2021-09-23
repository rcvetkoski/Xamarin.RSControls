using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.RSControls.Interfaces;

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
            IHaveError rsControl = view as IHaveError;

            Layout<View> parent = view.Parent as Layout<View>;


            if (rsControl != null)
            {
                foreach (IValidation validation in Validators)
                {
                    bool isValid = true;
                    var value = rsControl.GetType().GetProperty(PropertyName).GetValue(rsControl);

                    isValid = validation.Validate(value);

                    if (!isValid)
                    {
                        rsControl.Error = validation.Message;
                        return;
                    }
                }

                rsControl.Error = null;
            }
        }

        protected override void OnAttachedTo(View view)
        {
            view.PropertyChanged += View_PropertyChanged;
            base.OnAttachedTo(view);
        }

        private void View_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == this.PropertyName || e.PropertyName == "IsFocused" && (sender as View).IsFocused)
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
