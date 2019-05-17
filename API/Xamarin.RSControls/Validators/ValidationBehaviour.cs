using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

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
            foreach(IValidation validation in Validators)
            {
                bool isNotValid = true;
                var value = view.GetType().GetProperty(PropertyName).GetValue(view);

                if (value != null)
                    isNotValid = validation.Validate(value.ToString());
                else
                    isNotValid = true;


                if (isNotValid)
                {
                    view.BackgroundColor = Color.Red;
                    return;
                }
            }

            view.BackgroundColor = Color.Transparent;
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
