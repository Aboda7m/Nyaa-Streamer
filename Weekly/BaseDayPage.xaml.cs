using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer.Weekly
{
    public partial class BaseDayPage : ContentPage
    {
        public ObservableCollection<Anime> AnimeList { get; set; }
        public string PageTitle { get; set; }
        public string SortButtonImage { get; set; }
        private int _currentSortModeIndex = 0;
        private readonly string[] _sortModes = { "Airing Time", "Score", "Title" };
        private readonly string[] _sortImages = { "time.png", "score.png", "name.png" };
        private string _apiUrl;
        private static bool _isFetchingData = false; // Static flag to prevent simultaneous fetches

        public BaseDayPage(string apiUrl, string pageTitle)
        {
            InitializeComponent();

            AnimeList = new ObservableCollection<Anime>();
            _apiUrl = apiUrl;
            PageTitle = pageTitle;
            SortButtonImage = _sortImages[0];

            BindingContext = this;

            // Start data fetching
            _ = FetchAnimeData(); // Fire and forget, handle errors internally
        }

        // Fetch anime data from the API
        private async Task FetchAnimeData()
        {
            // Wait until data is not being fetched
            while (_isFetchingData)
            {
                // Optionally notify the user or log that fetching is in progress
                Console.WriteLine("Data is currently being fetched. Please wait.");
                await Task.Delay(500); // Wait for a short period before checking again
            }

            try
            {
                IsBusy = true;
                _isFetchingData = true; // Set the flag indicating fetching is in progress

                var animeList = await Anime.FetchAnimeDetailsAsync(_apiUrl);

                AnimeList.Clear();

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        AnimeList.Add(anime);
                    }

                    // Default sort by airing time
                    SortByAiringTime();
                }
                else
                {
                    await DisplayAlert("Error", "No anime found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime data: " + ex.Message, "OK");
            }
            finally
            {
                _isFetchingData = false; // Reset the flag when fetching is complete
                IsBusy = false; // Reset the busy state in the UI
            }
        }

        private void SortByAiringTime()
        {
            var sortedList = AnimeList.OrderBy(a => a.AiringTimeLocal).ToList();
            AnimeList.Clear();
            foreach (var anime in sortedList)
            {
                AnimeList.Add(anime);
            }
        }

        private void SortByScore()
        {
            var sortedList = AnimeList.OrderByDescending(a => a.Score).ToList();
            AnimeList.Clear();
            foreach (var anime in sortedList)
            {
                AnimeList.Add(anime);
            }
        }

        private void SortByTitle()
        {
            var sortedList = AnimeList.OrderBy(a => a.Title).ToList();
            AnimeList.Clear();
            foreach (var anime in sortedList)
            {
                AnimeList.Add(anime);
            }
        }

        protected void OnSortButtonClicked(object sender, EventArgs e)
        {
            _currentSortModeIndex = (_currentSortModeIndex + 1) % _sortModes.Length;
            SortButtonImage = _sortImages[_currentSortModeIndex];

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

            // Update the image binding
            OnPropertyChanged(nameof(SortButtonImage));
        }

        protected async void OnAnimeSelected(object sender, EventArgs e)
        {
            var selectedAnime = GetSelectedAnime(sender);
            if (selectedAnime != null)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }

        private Anime GetSelectedAnime(object sender)
        {
            if (sender is Frame selectedFrame)
                return selectedFrame.BindingContext as Anime;

            if (sender is Grid selectedGrid)
                return selectedGrid.BindingContext as Anime;

            return null;
        }
    }
}
