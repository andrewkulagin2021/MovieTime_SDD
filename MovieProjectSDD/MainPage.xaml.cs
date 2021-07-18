using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace MovieProjectSDD
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private ObservableCollection<GroupMovies> groupsMovies = new ObservableCollection<GroupMovies>();
        public ObservableCollection<GroupMovies> GroupsMovies
        {
            get { return this.groupsMovies; }
            set
            {
                groupsMovies = value;
                OnPropertyChanged();
            }
        }
        private List<Movie> allMovies = new List<Movie>();

        public MainPage()
        {
            Settings.MovieAdapters.Add(new AppleAdapter());
            Settings.MovieAdapters.Add(new SBSAdapter());
            Settings.MovieAdapters.Add(new EventAdapter());
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
            this.DataContext = this;
            Loaded += MainPage_Loaded;
        }

        private async void ShowTutorial(object sender, RoutedEventArgs e)
        {          
            Windows.System.Launcher.LaunchUriAsync(new Uri("https://youtu.be/I25Lakx7Oto"));
            //Windows.Storage.StorageFile videoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/SampleMedia/MovieTimeTutorial.mp4"));
            //var source = MediaSource.CreateFromStorageFile(videoFile);
            //TutorialPlayer.Source = source;
            //await MediaPlayerDialog.ShowAsync();
        }

        private void MediaPlayerDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {            
            TutorialPlayer.Source = null;
        }
        private void ShowMoviePage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MoviePage), (sender as Button).DataContext);
        }
        private void ShowSettingsPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (allMovies.Any() && !Settings.UpdateMovies) return;
            await UpdateListOfAllMovies();           
            UpdateFiltersSort();
        }

        private async Task UpdateListOfAllMovies()
        {
            allMovies.Clear();
            foreach (IMovieAdapter movieAdapter in Settings.MovieAdapters)
                allMovies.AddRange(await movieAdapter.GetMovies());
            Settings.UpdateMovies = false;
            //get imdb rating and upload empty posters
            IMDBAdapter.AddRating(allMovies);
            Filter_combo.SelectedIndex = 0;

        }
        private void SortByGenre()
        {
            var genres = allMovies.Select(mov => mov.Genre).Distinct().OrderBy(name => name).ToList();
            ObservableCollection<GroupMovies> sortedgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var genreName in genres)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = genreName;
                groupMovies.Movies = allMovies.Where(x => x.Genre == genreName).ToList();
                if (groupMovies.Movies.Any()) sortedgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = sortedgroupsMovies;
        }
        private void SortByPlatform()
        {
            var platforms = allMovies.Select(mov => mov.Adapters[0]).Distinct().OrderBy(name => name).ToList();
            ObservableCollection<GroupMovies> sortedgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var platformName in platforms)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = platformName.ToString();
                groupMovies.Movies = allMovies.Where(x => x.Adapters.Contains(platformName)).ToList();
                if (groupMovies.Movies.Any()) sortedgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = sortedgroupsMovies;
        }
        private void SortByDate()
        {
            var years = allMovies.Select(mov => mov.ReleaseDate.Year).Distinct().OrderByDescending(yr => yr).ToList();
            ObservableCollection<GroupMovies> sortedgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var movYear in years)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = movYear.ToString();
                groupMovies.Movies = allMovies.Where(x => x.ReleaseDate.Year == movYear).ToList();
                if (groupMovies.Movies.Any()) sortedgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = sortedgroupsMovies;
        }

        private void SortByName()
        {
            var names = allMovies.Select(mov => mov.Title[0]).Distinct().OrderBy(name => name).ToList();
            ObservableCollection<GroupMovies> sortedgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var movName in names)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = movName.ToString();
                groupMovies.Movies = allMovies.Where(x => x.Title[0] == movName).ToList();
                if (groupMovies.Movies.Any()) sortedgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = sortedgroupsMovies;

        }
        private void FilterByAgeRating(string rating)
        {
            var platforms = allMovies.Select(mov => mov.Content_Rating).Distinct().OrderBy(name => name).ToList();
            ObservableCollection<GroupMovies> filteredgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var group in GroupsMovies)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = group.Name;
                if (rating == "PG") groupMovies.Movies = group.Movies.Where(x => x.Content_Rating.Contains("G")).ToList();
                if (rating == "M") groupMovies.Movies = group.Movies.Where(x => x.Content_Rating.Contains("M")).ToList();
                if (rating == "R") groupMovies.Movies = group.Movies.Where(x => x.Content_Rating.Contains("R")).ToList();
                if (groupMovies.Movies.Any()) filteredgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = filteredgroupsMovies;
        }
        private void FilterByFavourite()
        {
            ObservableCollection<GroupMovies> favouriteGroupMovies = new ObservableCollection<GroupMovies>();
            List<Movie> favouriteMovies = allMovies.Where(mv => Settings.FavouriteMovies.Contains(mv.Title)).ToList();
            GroupMovies favGroupMovies = new GroupMovies();
            favGroupMovies.Name = "Favourite Movies";
            favGroupMovies.Movies = favouriteMovies;
            if (favGroupMovies.Movies.Any()) favouriteGroupMovies.Add(favGroupMovies);
            GroupsMovies = favouriteGroupMovies;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            List<string> names = allMovies.Select(mov => mov.Title).Distinct().OrderBy(name => name).ToList();
            List<string> suitableItems = new List<string>();
            string[] splitText = sender.Text.ToLower().Split(" ");
            foreach (var name in names)
            {
                var found = splitText.All((key) =>
                {
                    return name.ToLower().Contains(key);
                });
                if (found)
                {
                    suitableItems.Add(name);
                }
            }
            if (suitableItems.Count == 0)
            {
                suitableItems.Add("No results found");
            }
            sender.ItemsSource = suitableItems;
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Movie movie = allMovies.FirstOrDefault(mov => mov.Title == (args.SelectedItem as string));
            this.Frame.Navigate(typeof(MoviePage), movie);

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Settings.UpdateFavourites) UpdateFiltersSort();
            BackButton.IsEnabled = this.Frame.CanGoBack;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            UpdateFiltersSort();
        }

        private void UpdateFiltersSort()
        {
            if (Sort_combo != null)
            {
                string sort_selection = Sort_combo.SelectedItem as string;
                if (sort_selection != null && sort_selection.Equals("Genre")) SortByGenre();
                else if (sort_selection != null && sort_selection.Equals("Name")) SortByName();
                else if (sort_selection != null && sort_selection.Equals("Date Added")) SortByDate();
                else if (sort_selection != null && sort_selection.Equals("Platform")) SortByPlatform();
            }

            //filterMovies
            if (Filter_combo != null)
            {
                string filter_selection = Filter_combo.SelectedItem as string;
                if (filter_selection != null && filter_selection.Equals("Show All")) return;
                else if (filter_selection != null && filter_selection.Equals("Rating > 6")) FilterByRating(6);
                else if (filter_selection != null && filter_selection.Equals("Rating > 7")) FilterByRating(7);
                else if (filter_selection != null && filter_selection.Equals("Rating > 8")) FilterByRating(8);
                else if (filter_selection != null && filter_selection.Equals("Content Rating PG")) FilterByAgeRating("PG");
                else if (filter_selection != null && filter_selection.Equals("Content Rating M")) FilterByAgeRating("M");
                else if (filter_selection != null && filter_selection.Equals("Content Rating R")) FilterByAgeRating("R");
                else if (filter_selection != null && filter_selection.Equals("Favourite Movies")) FilterByFavourite();
            }
            Settings.UpdateFavourites = false;
        }
        private void FilterByRating(double rating)
        {
            ObservableCollection<GroupMovies> filteredgroupsMovies = new ObservableCollection<GroupMovies>();
            foreach (var group in GroupsMovies)
            {
                GroupMovies groupMovies = new GroupMovies();
                groupMovies.Name = group.Name;
                groupMovies.Movies = group.Movies.Where(x => double.TryParse(x.ImdbRating, out double rat) && rat >= rating).ToList();
                if (groupMovies.Movies.Any()) filteredgroupsMovies.Add(groupMovies);
            }
            GroupsMovies = filteredgroupsMovies;
        }
        public void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        // Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        {
            // open the Popup if it isn't open already 
            if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
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
            }
           await UpdateListOfAllMovies();
            UpdateFiltersSort();
        }
    }
}
