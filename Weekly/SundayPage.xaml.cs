using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class SundayPage : ContentPage
    {
        public ObservableCollection<Anime> SundayAnimeList { get; set; }
        private int _currentSortModeIndex = 0; // Track the current sorting mode index
        private readonly string[] _sortModes = { "Airing Time", "Score", "Title" }; // Sort modes
        private readonly string[] _sortImages = { "time.png", "score.png", "name.png" }; // Corresponding images

        public SundayPage()
        {
            InitializeComponent();
            SundayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchSundayAnimeData();
        }

        // Fetch data for Sunday anime
        private async Task FetchSundayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=sunday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                SundayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        SundayAnimeList.Add(anime);
                    }

                    // Sort by airing time by default after fetching
                    SortByAiringTime();
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for Sunday.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime data: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false; // Hide loading indicator
            }
        }

        // Handle anime selection
        private async void OnAnimeSelected(object sender, EventArgs e)
        {
            var selectedFrame = sender as Frame;
            var selectedGrid = sender as Grid;

            Anime selectedAnime = null;

            if (selectedFrame != null)
            {
                selectedAnime = selectedFrame.BindingContext as Anime;
            }
            else if (selectedGrid != null)
            {
                selectedAnime = selectedGrid.BindingContext as Anime;
            }

            if (selectedAnime != null)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }

        // Function to sort by airing time
        private void SortByAiringTime()
        {
            var sortedList = SundayAnimeList
                .OrderBy(a => ExtractTimeFromAiringString(a.AiringTimeLocal))
                .ToList();

            SundayAnimeList.Clear();
            foreach (var anime in sortedList)
            {
                SundayAnimeList.Add(anime);
            }
        }

        // Function to extract time from 'AiringTimeLocal'
        private TimeSpan ExtractTimeFromAiringString(string airingTime)
        {
            // Use regular expressions to extract the time in HH:mm format
            var timeMatch = Regex.Match(airingTime, @"\d{2}:\d{2}");
            if (timeMatch.Success)
            {
                return TimeSpan.Parse(timeMatch.Value); // Return TimeSpan for comparison
            }
            return TimeSpan.Zero; // Default to midnight if no time is found
        }

        // Function to sort by score
        private void SortByScore()
        {
            var sortedList = SundayAnimeList.OrderByDescending(a => a.Score).ToList();
            SundayAnimeList.Clear();
            foreach (var anime in sortedList)
            {
                SundayAnimeList.Add(anime);
            }
        }

        // Function to sort by title (name)
        private void SortByTitle()
        {
            var sortedList = SundayAnimeList.OrderBy(a => a.Title).ToList();
            SundayAnimeList.Clear();
            foreach (var anime in sortedList)
            {
                SundayAnimeList.Add(anime);
            }
        }

        // Sort button clicked event handler
        private void OnSortButtonClicked(object sender, EventArgs e)
        {
            // Increment the current sort mode index and wrap around
            _currentSortModeIndex = (_currentSortModeIndex + 1) % _sortModes.Length;

            // Update the sort button image
            SortButton.Source = _sortImages[_currentSortModeIndex];

            // Perform sorting based on the current mode
            switch (_sortModes[_currentSortModeIndex])
            {
                case "Airing Time":
                    SortByAiringTime();
                    break;
                case "Score":
                    SortByScore();
                    break;
                case "Title":
                    SortByTitle();
                    break;
            }
        }
    }
}
