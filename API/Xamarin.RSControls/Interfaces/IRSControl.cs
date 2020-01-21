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
        bool IsPlaceholderAlwaysFloating { get; set; }
        string Placeholder { get; set; }
        Color PlaceholderColor { get; set; }
        string Helper { get; set; }
        int Counter { get; set; }
        int CounterMaxLength { get; set; }
        Color BorderFillColor { get; set; }
        Color BorderColor { get; set; }
        Color ActiveColor { get; set; }
        Color ErrorColor { get; set; }
        RSEntryStyleSelectionEnum RSEntryStyle { get; set; }
        double FontSize { get; set; }
        bool HasError { get; }
        bool IsPassword { get; set; }
        float BorderRadius { get; set; }
        RSEntryIcon LeadingIcon { get; set; }
        RSEntryIcon TrailingIcon { get; set; }
        RSEntryIcon LeftIcon { get; set; }
        RSEntryIcon RightIcon { get; set; }
        RSEntryIcon LeftHelpingIcon { get; set; }
        RSEntryIcon RightHelpingIcon { get; set; }
        IList<RSEntryIcon> LeftIcons { get; set; }
        bool HasRighIconSeparator { get; set; }
        bool HasLeftIconSeparator { get; set; }
        Thickness Padding { get; set; }
    }
}
