using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.RSControls.Controls;
using Xamarin.RSControls.Enums;
using Xamarin.RSControls.Interfaces;

namespace Samples
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainPageViewModel();


            //for (int i = 0; i < 200; i++)
            //{
            //    //stack.Children.Add(new RSEntry() { Text = "lol", Helper = "Helper" });

            //    stack.Children.Add(new ImageButton() { Source = new RSImageSource() { Source = "Xamarin.RSControls/Data/SVG/plus.svg", Color = Color.Purple }, WidthRequest = 40, HeightRequest = 40, BackgroundColor = Color.Red });
            //}
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainTabbedPage());
        }


        private void Button_Clicked1(object sender, EventArgs e)
        {
            Editor entry = new Editor();
            entry.Placeholder = "Forms View";
            entry.AutoSize = EditorAutoSizeOption.TextChanges;
            //entry.HorizontalOptions = LayoutOptions.FillAndExpand;
            //entry.BackgroundColor = Color.Yellow;
            //entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";

            ListView listView = new ListView();
            listView.HeightRequest = 150;
            listView.HorizontalOptions = LayoutOptions.Center;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message Trolewoflwefeowl j2f2fk 2n2jfn \r\n j2k qwdwqdqwwqd");
            //rsPopup.SetCustomView(entry);
            rsPopup.SetDimAmount(0.6f);
            rsPopup.SetIsModal(false);
            rsPopup.HasCloseButton = true;
            rsPopup.SetMargin(0, 0, 0, 0);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Left);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);

            rsPopup.Show();

        }

        private void Button_Clicked2(object sender, EventArgs e)
        {
            //(this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(1);


            RSEditor rSEditor = new RSEditor() {Placeholder = "Enter some text", AutoSize= EditorAutoSizeOption.TextChanges};
            rSEditor.AutoSize = EditorAutoSizeOption.TextChanges;
            rSEditor.Helper = "Helper !";
            rSEditor.SetBinding(RSEditor.TextProperty, new Binding("Lolo", BindingMode.TwoWay) { Source = this.BindingContext });
            rSEditor.TextChanged += Entry_TextChanged;
            RSEntry entry2 = new RSEntry() { Placeholder = "Enter some text", Helper = "Helper !"};
            Grid stack = new Grid() { BackgroundColor = Color.Cyan};
            stack.Children.Add(rSEditor, 0, 0);
            stack.Children.Add(entry2, 1, 0);
            stack.Padding = new Thickness(30);

            RSEntry rSEntry = new RSEntry();
            rSEntry.Text = "sdiufhesfeofèeifwehweofhewofhoewf";
            //rSEntry.SetBinding(RSEntry.TextProperty, new Binding("Lolo", BindingMode.TwoWay) { Source = this.BindingContext });
            rSEntry.HorizontalOptions = LayoutOptions.Start;

            Label label = new Label();
            label.Text = "Troloeole Mehotn";

            Entry entry = new Entry(); 
            entry.Text = "Custom entry rreregregergregreergregregrereg";

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message");
            //rsPopup.SetCustomView(entry);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            //rsPopup.HasCloseButton = true;
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Right);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);



            rsPopup.Show();

        }


        private void Button_Clicked3(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";


            RSPopup rsPopup = new RSPopup("", "Message Tewowl ewolo werew we weg ewweg egw weg ewg wegwe gew g gwe gewg  weg");
            //rsPopup.SetCustomView(entry); 
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Left);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked4(object sender, EventArgs e)
        {
            //Entry entry = new Entry();
            //entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";

            //RSEditor rSEditor = new RSEditor() { Placeholder = "Enter some text", AutoSize = EditorAutoSizeOption.TextChanges };

            RSPopup rsPopup = new RSPopup("RSPopup Test wefwef w Test !", "Message");
            //rsPopup.SetCustomView(rSEditor);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.MatchParent, 150);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Over);
            //rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            //rsPopup.AddAction(" Remove ", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
            //                            (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked5(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "Trolololh wegewgewgw";
            entry.HorizontalOptions = LayoutOptions.End;


            Label label = new Label();
            label.MaxLines = 100;
            label.BackgroundColor = Color.Green;
            label.Text = "Trolololh wegewgewgw";
            label.HorizontalOptions = LayoutOptions.Start;
            //label.Margin = new Thickness(10, 10, 10, 10);
            label.HorizontalTextAlignment = TextAlignment.Start;
            label.VerticalOptions = LayoutOptions.Start;

            ListView listView = new ListView();
            listView.BackgroundColor = Color.Yellow;
            //listView.WidthRequest = 200;
            listView.Margin = new Thickness(50, 10, 50, 10);
            //listView.HorizontalOptions = LayoutOptions.FillAndExpand;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("RSPopup !", "Message");
            rsPopup.SetCustomView(label);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.MatchParent, RSPopupSizeEnum.WrapContent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Top);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Button_Clicked6(object sender, EventArgs e)
        {
            Entry entry = new Entry();
            entry.Text = "TrolololhghghhgjggjhghgfghgfholoWhr ";
            entry.HorizontalOptions = LayoutOptions.FillAndExpand;

            ListView listView = new ListView();
            //listView.WidthRequest = 50;
            listView.HorizontalOptions = LayoutOptions.Start;
            listView.ItemsSource = (this.BindingContext as MainPageViewModel).ListPicker;

            RSPopup rsPopup = new RSPopup("", "");
            rsPopup.SetCustomView(listView);
            rsPopup.SetDimAmount(0.0f);
            rsPopup.SetIsModal(false);
            rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.MatchParent, RSPopupSizeEnum.MatchParent);
            //rsPopup.SetPopupPositionRelativeTo(sender as View);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Bottom);
            rsPopup.AddAction("Done", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction("Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);
            rsPopup.AddAction("Remove", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Destructive,
                                        (this.BindingContext as MainPageViewModel).RSCommand, (this.BindingContext as MainPageViewModel).Lolo);


            rsPopup.Show();
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.BindingContext as MainPageViewModel).RSCommand.ChangeCanExecute();
        }

        private void RSTabbedViews_TabSelected(object sender, EventArgs e)
        {
            Console.WriteLine((sender as RSTabbedViews).CurrentPageIndex + "  " + ((sender as RSTabbedViews).CurrentView).BindingContext);
        }

        private void AddItem(object sender, EventArgs e)
        {
            (this.BindingContext as MainPageViewModel).ObsCollectionPicker.Add("Rade");
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            //if(rstabviews.CurrentPageIndex >= 0)
            //    (this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(rstabviews.CurrentPageIndex);


            (this.BindingContext as MainPageViewModel).ObsCollectionPicker.RemoveAt(0);
        }

        void RSPicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            Console.WriteLine("feff");
        }

        void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            //var rsPopup = new RSPopup("Title trololo", "Bidi mehe Pulitzer!", RSPopupPositionEnum.Bottom);
            var rsPopup = new RSPopup("Title trololo", "Quisque rutrum, hendrerit id, lorem. , mollis sed, nonummy id, metus. Sed consequat, Vestibulum ante posuere cubilia Curae; In ac dui quis mi coicidunt non, euismod vitae, posuere imperdiet, leo. Maecenas malesuada. Praesent congue erat at massa. Sed cursus turpis vitae tortor. Donec posuere vulputate arcu. Phasellus accumsan cursus velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed aliquam, nisi quis porttitor congue, elit erat euismod orci, ac placerat dolor lectus quis orci. Phasellus consectetuer vestibulum elit. Aenean tellus metus, bibendum sed, posuere ac, mattis non, nunc. Vestibulum fringilla pede sit amet augue. In turpis. Pellentesque posuere. Praesent turpis. Aenean posuere, tortor sed cursus feugiat, nunc augue blandit nunc, eu sollicitudin urna dolor sagittis lacus. Donec elit libero, sodales nec, volutpat a, suscipit non, turpis. Nullam sagittis. Suspendisse pulvinar, augue ac venenatis condimentum, sem libero volutpat nibh, nec pellentesque velit pede quis nunc. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce id purus. Ut varius tincidunt libero. Phasellus dolor. Maecenas vestibulum mollis",
                RSPopupPositionEnum.Bottom);

            //var rsPopup = new RSPopup("Title trololo", "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus. Nullam accumsan lorem in dui. Cras ultricies mi eu turpis hendrerit fringilla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia. Nam pretium turpis et arcu. Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris. Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris. Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc. Nunc nonummy metus. Vestibulum volutpat pretium libero. Cras id dui. Aenean ut eros et nisl sagittis vestibulum. Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede. Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis. Etiam imperdiet imperdiet orci. Nunc nec neque. Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi. Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo. Maecenas malesuada. Praesent congue erat at massa. Sed cursus turpis vitae tortor. Donec posuere vulputate arcu. Phasellus accumsan cursus velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed aliquam, nisi quis porttitor congue, elit erat euismod orci, ac placerat dolor lectus quis orci. Phasellus consectetuer vestibulum elit. Aenean tellus metus, bibendum sed, posuere ac, mattis non, nunc. Vestibulum fringilla pede sit amet augue. In turpis. Pellentesque posuere. Praesent turpis. Aenean posuere, tortor sed cursus feugiat, nunc augue blandit nunc, eu sollicitudin urna dolor sagittis lacus. Donec elit libero, sodales nec, volutpat a, suscipit non, turpis. Nullam sagittis. Suspendisse pulvinar, augue ac venenatis condimentum, sem libero volutpat nibh, nec pellentesque velit pede quis nunc. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce id purus. Ut varius tincidunt libero. Phasellus dolor. Maecenas vestibulum mollis");

            rsPopup.SetPopupAnimation(RSPopupAnimationEnum.BottomToTop);

            Editor label = new Editor() { Text = "Title trololo Lorem ipsum dolor sit amet, consectetuer adipiscing elit. " +
                "Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, " +
                "nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis eni" +
                "nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis eni",
              };

            Entry entry = new Entry() { Placeholder = "Entry ger wefe" };
            //entry.Margin = new Thickness(60, 60, 10, 10);

            label.BackgroundColor = Color.Yellow;
            //label.Margin = new Thickness(10, 10, 10, 10);

            //rsPopup.SetMargin(10, 10, 10, 10);
            //rsPopup.SetCustomView(entry);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetDimAmount(0.0f);
            //rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Center);
            rsPopup.AddAction(Title = "Close", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            //rsPopup.AddAction(Title = "Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral, new Command(() =>
            //{
            //}));
            rsPopup.Show();
        }

        void Button_Clicked_2(System.Object sender, System.EventArgs e)
        {
            var rsPopup = new RSPopup("Title trololo fwefewfewfewfewf", "Bidi mehe Pulitzer!", RSPopupPositionEnum.Bottom);
            //var rsPopup = new RSPopup("Title trololo", "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus. Nullam accumsan lorem in dui. Cras ultricies mi eu turpis hendrerit fringilla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia. Nam pretium turpis et arcu. Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris. Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris. Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc. Nunc nonummy metus. Vestibulum volutpat pretium libero. Cras id dui. Aenean ut eros et nisl sagittis vestibulum. Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede. Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis. Etiam imperdiet imperdiet orci. Nunc nec neque. Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi. Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo. Maecenas malesuada. Praesent congue erat at massa. Sed cursus turpis vitae tortor. Donec posuere vulputate arcu. Phasellus accumsan cursus velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed aliquam, nisi quis porttitor congue, elit erat euismod orci, ac placerat dolor lectus quis orci. Phasellus consectetuer vestibulum elit. Aenean tellus metus, bibendum sed, posuere ac, mattis non, nunc. Vestibulum fringilla pede sit amet augue. In turpis. Pellentesque posuere. Praesent turpis. Aenean posuere, tortor sed cursus feugiat, nunc augue blandit nunc, eu sollicitudin urna dolor sagittis lacus. Donec elit libero, sodales nec, volutpat a, suscipit non, turpis. Nullam sagittis. Suspendisse pulvinar, augue ac venenatis condimentum, sem libero volutpat nibh, nec pellentesque velit pede quis nunc. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce id purus. Ut varius tincidunt libero. Phasellus dolor. Maecenas vestibulum mollis");

            //rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetDimAmount(0f);
            rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Center);
            Entry rSEntry = new Entry() { Text = "MEHE wf", Margin = new Thickness(0, 0, 0, 0)};
            rsPopup.SetCustomView(rSEntry);
            rsPopup.AddAction(Title = "Ok", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive);
            rsPopup.AddAction(Title = "Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral, new Command(() =>
            {

            }));
            rsPopup.Show();
        }

        void Button_Clicked_3(System.Object sender, System.EventArgs e)
        {
            var rsPopup = new RSPopup("Title", "Message");
            //var rsPopup = new RSPopup("Title trololo", "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus. Nullam accumsan lorem in dui. Cras ultricies mi eu turpis hendrerit fringilla. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia. Nam pretium turpis et arcu. Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris. Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris. Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc. Nunc nonummy metus. Vestibulum volutpat pretium libero. Cras id dui. Aenean ut eros et nisl sagittis vestibulum. Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede. Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis. Etiam imperdiet imperdiet orci. Nunc nec neque. Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi. Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo. Maecenas malesuada. Praesent congue erat at massa. Sed cursus turpis vitae tortor. Donec posuere vulputate arcu. Phasellus accumsan cursus velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed aliquam, nisi quis porttitor congue, elit erat euismod orci, ac placerat dolor lectus quis orci. Phasellus consectetuer vestibulum elit. Aenean tellus metus, bibendum sed, posuere ac, mattis non, nunc. Vestibulum fringilla pede sit amet augue. In turpis. Pellentesque posuere. Praesent turpis. Aenean posuere, tortor sed cursus feugiat, nunc augue blandit nunc, eu sollicitudin urna dolor sagittis lacus. Donec elit libero, sodales nec, volutpat a, suscipit non, turpis. Nullam sagittis. Suspendisse pulvinar, augue ac venenatis condimentum, sem libero volutpat nibh, nec pellentesque velit pede quis nunc. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce id purus. Ut varius tincidunt libero. Phasellus dolor. Maecenas vestibulum mollis",
            //    RSPopupPositionEnum.Center);

            //rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetPopupSize(RSPopupSizeEnum.WrapContent, RSPopupSizeEnum.WrapContent);
            rsPopup.SetDimAmount(0f);
            //rsPopup.SetPopupPositionRelativeTo(sender as View, RSPopupPositionSideEnum.Center);
            //rsPopup.AddAction(Title = "Ok", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Positive, new Command(() =>
            //{
                
            //}));
            //rsPopup.AddAction(Title = "Cancel", Xamarin.RSControls.Enums.RSPopupButtonTypeEnum.Neutral);



            StackLayout stack = new StackLayout() { BackgroundColor = Color.Transparent, Margin = new Thickness(10, 10, 10, 10)};
            StackLayout stack2 = new StackLayout() { BackgroundColor = Color.Red, Margin = new Thickness(10, 10, 10, 10), VerticalOptions = LayoutOptions.EndAndExpand };
            RSNumericUpDown weight = new RSNumericUpDown() { Placeholder = "Weight", Margin = new Thickness(10, 10, 10, 10)};
            RSNumericUpDown reps = new RSNumericUpDown() { Placeholder = "Reps", Margin = new Thickness(10, 10, 10, 10) };
            Entry entry = new Entry() { Text = "lol" };
            stack2.Children.Add(weight);
            stack.Children.Add(stack2);
            stack.Children.Add(reps);


            //rsPopup.SetMargin(10, 10, 10, 10);
            rsPopup.SetCustomView(stack);

            rsPopup.Show();
        }
    }
}
