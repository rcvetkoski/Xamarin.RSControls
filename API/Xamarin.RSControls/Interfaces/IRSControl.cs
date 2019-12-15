using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Helpers;

namespace Xamarin.RSControls.Interfaces
{
    public interface IRSControl
    {
        string Placeholder { get; set; }
        Color PlaceholderColor { get; set; }
        string Helper { get; set; }
        int Counter { get; set; }
        int CounterMaxLength { get; set; }
        Color BorderColor { get; set; }
        Color ActiveColor { get; set; }
        Color ErrorColor { get; set; }
        RSEntryStyleSelectionEnum RSEntryStyle { get; set; }
        double FontSize { get; set; }
        bool HasError { get; }
        bool IsPassword { get; set; }
        float BorderRadius { get; set; }
        string LeadingIcon { get; set; }
        string TrailingIcon { get; set; }
        string LeftIcon { get; set; }
        RSEntryIcon RightIcon { get; set; }
        Color IconColor { get; set; }
        double IconSize { get; set; }
        Thickness Padding { get; set; }
    }
}
