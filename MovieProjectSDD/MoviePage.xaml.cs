using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
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
    public sealed partial class MoviePage : Page
    {
        private Movie mov { get; set; }
        private MediaPlayer mediaPlayer { get; set; }


        public MoviePage()
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
            Loaded += MoviePage_Loaded;
        }
        private async void MoviePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mov.TrailerUrl))
            {
                await TryToFindTrailer();
            }
            if (!string.IsNullOrEmpty(mov.TrailerUrl))
            {
                trailer_player.Source = MediaSource.CreateFromUri(new Uri(mov.TrailerUrl));

                mediaPlayer = trailer_player.MediaPlayer;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = this.Frame.CanGoBack;
            if (e.Parameter is Movie)
            {
                mov = (e.Parameter as Movie);
                MovTitle.Text = mov.Title + " " + mov.ReleaseYear;
                BitmapImage bitmapImage = new BitmapImage();
                Uri uri = new Uri(mov.PosterUrl);
                bitmapImage.UriSource = uri;
                Poster.Source = bitmapImage;
                mov_description.Text = mov.Description;
                mov_imdb_str.Text = mov.ImdbRating + "/10 (" + mov.ImdbVotes + ")";
                if (!string.IsNullOrEmpty(mov.ImdbGenre)) mov_genres.Text = mov.ImdbGenre;
                else mov_genres.Text = mov.Genre;
                if (!string.IsNullOrEmpty(mov.RTRating)) mov_rottom_str.Text = mov.RTRating;
                else mov_rottom_str.Text = "N/A";
                if (!string.IsNullOrEmpty(mov.Writers)) mov_writers.Text = mov.Writers;
                else mov_writers.Text = "N/A";
                if (!string.IsNullOrEmpty(mov.Directors)) mov_director.Text = mov.Directors;
                else mov_director.Text = "N/A";
                if (!string.IsNullOrEmpty(mov.Actors)) mov_actors.Text = mov.Actors;
                else mov_actors.Text = "N/A";
                if (!string.IsNullOrEmpty(mov.ReleaseDateString)) mov_release_date.Text = mov.ReleaseDateString;
                else mov_release_date.Text = "N/A";
                if (!string.IsNullOrEmpty(mov.Runtime)) mov_durarion.Text = mov.Runtime;
                else mov_durarion.Text = "N/A";
                mov_adapter_btn.Content = "Open " + mov.Adapters[0] + " link";
                if (Settings.FavouriteMovies.Contains(mov.Title))
                {
                    Favourites_Button.IsChecked = true;
                    Favourites_Button.Content = "Remove From Favourites";
                }
            }
            base.OnNavigatedTo(e);
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
                mediaPlayer = null;

            }
            On_BackRequested();
        }
        private async Task TryToFindTrailer()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string search_json;
            using (var client = new HttpClient())
            {
                search_json = await client.GetStringAsync("https://trailers.apple.com/trailers/home/scripts/quickfind.php?callback=searchCallback&q=" + mov.Title);
            }
            dynamic search_results = JsonConvert.DeserializeObject<dynamic>(search_json.Substring(15, search_json.Length - 18));
            if (!search_results.results.HasValues) return;
            string location = search_results.results[0].location;
            string html;
            using (var client = new HttpClient())
            {
                string address = "https://trailers.apple.com" + location;
                html = await client.GetStringAsync(address);
            }
            Match match = Regex.Match(html, @"\/movie\/detail/(\d+)");
            string movieID = null;
            if (match.Success) movieID = match.Groups[1].Value;
            else return;
            string movieJSON = "";
            using (var client = new HttpClient())
            {
                string address = "https://trailers.apple.com/trailers/feeds/data/" + movieID + ".json";
                movieJSON = await client.GetStringAsync(address);
            }
            dynamic results_movie = JsonConvert.DeserializeObject<dynamic>(movieJSON);
            mov.TrailerUrl = results_movie.clips[0].versions.enus.sizes.hd720.srcAlt;

        }
        private void ShowSettingsPage(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
                mediaPlayer = null;

            }
            this.Frame.Navigate(typeof(SettingsPage));
        }
        private void Imdb_button_click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.imdb.com/title/" + mov.ImdbID;
            Windows.System.Launcher.LaunchUriAsync(new Uri(url));

        }

        // Handles system-level BackRequested events and page-level back button Click events
        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private void mov_adapter_btn_Click(object sender, RoutedEventArgs e)
        {
            string url = mov.AdapterUrls[0];
            Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }

        private void Favourites_Button_Pressed(object sender, RoutedEventArgs e)
        {
            if (Favourites_Button.IsChecked == true)
            {
                Favourites_Button.Content = "Remove From Favourites";
                Settings.FavouriteMovies.Add(mov.Title);
            }
            else
            {
                Favourites_Button.Content = "Add To Favourites";
                int index = Settings.FavouriteMovies.IndexOf(mov.Title);
                Settings.FavouriteMovies.RemoveAt(index);
            }
            Settings.UpdateFavourites = true;
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
                Settings.UpdateFavourites = true;


                if (Settings.FavouriteMovies.Contains(mov.Title))                
                {
                    Favourites_Button.IsChecked = true;
                    Favourites_Button.Content = "Remove From Favourites";                   
                }
                else
                {
                    Favourites_Button.IsChecked = false;
                    Favourites_Button.Content = "Add To Favourites";                   
                }
                Settings.UpdateFavourites = true;

            }


        }
    }
}
