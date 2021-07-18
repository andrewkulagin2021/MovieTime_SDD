using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MovieProjectSDD
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        //public Settings Settings { get; set; }
        public SettingsPage()
        {

            KeyboardAccelerator GoBack = new KeyboardAccelerator();
            GoBack.Key = VirtualKey.GoBack;
            GoBack.Invoked += BackInvoked;
            KeyboardAccelerator AltLeft = new KeyboardAccelerator();
            AltLeft.Key = VirtualKey.Left;
            AltLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(GoBack);
            this.KeyboardAccelerators.Add(AltLeft);
            // ALT routes here
            AltLeft.Modifiers = VirtualKeyModifiers.Menu;
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = this.Frame.CanGoBack;
            UpdateSettings();

            base.OnNavigatedTo(e);
        }
        private void UpdateSettings()
        {
            fav_stack_panel.Children.Clear();
            foreach (string favMov in Settings.FavouriteMovies)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = favMov;
                fav_stack_panel.Children.Add(textBlock);
            }
            Apple_chckbx.IsChecked = false;
            SBS_chckbx.IsChecked = false;
            Event_chbx.IsChecked = false;
            foreach (IMovieAdapter movAdapter in Settings.MovieAdapters)
            {
                if (movAdapter is AppleAdapter) Apple_chckbx.IsChecked = true;
                if (movAdapter is SBSAdapter) SBS_chckbx.IsChecked = true;
                if (movAdapter is EventAdapter) Event_chbx.IsChecked = true;
            }
            Settings.UpdateMovies = true;
        }
        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }
        private void ShowSettingsPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
        private async void SaveSettingsAsync(object sender, RoutedEventArgs e)
        {
            List<object> settings = new List<object>();
            settings.Add(Settings.MovieAdapters);
            settings.Add(Settings.FavouriteMovies);
            string jsonString = JsonConvert.SerializeObject(settings);

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "MovieTimeSettings";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteTextAsync(file, jsonString);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }


        private async void ReadSettings(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                string json = await FileIO.ReadTextAsync(file);
                dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
                List<IMovieAdapter> movieAdapters = new List<IMovieAdapter>();
                foreach (JToken movJAdapter in results[0] as JArray)
                {
                    if (movJAdapter["Name"].ToObject<string>() == "Apple") movieAdapters.Add(new AppleAdapter());
                    else if (movJAdapter["Name"].ToString() == "Event Cinemas") movieAdapters.Add(new EventAdapter());
                    else if (movJAdapter["Name"].ToString() == "SBS on Demand") movieAdapters.Add(new SBSAdapter());

                }
                List<string> favMovies = results[1].ToObject<List<string>>();
                Settings.MovieAdapters = movieAdapters;
                Settings.FavouriteMovies = favMovies;
                UpdateSettings();
            }

        }
        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }
        private void Adapters_CheckBox_Change(object sender, RoutedEventArgs e)
        {
            List<IMovieAdapter> selectedAdapters = new List<IMovieAdapter>();
            if (SBS_chckbx.IsChecked == true) selectedAdapters.Add(new SBSAdapter());
            if (Apple_chckbx.IsChecked == true) selectedAdapters.Add(new AppleAdapter());
            if (Event_chbx.IsChecked == true) selectedAdapters.Add(new EventAdapter());
            Settings.UpdateMovies = true;
            Settings.MovieAdapters = selectedAdapters;
        }
    }


}
