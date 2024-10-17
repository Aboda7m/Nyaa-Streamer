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

        public BaseDayPage(string apiUrl, string pageTitle)
        {
            InitializeComponent();

            AnimeList = new ObservableCollection<Anime>();
            _apiUrl = apiUrl;
            PageTitle = pageTitle;
            SortButtonImage = _sortImages[0];

            BindingContext = this;

            FetchAnimeData();
        }

        // Fetch anime data from the API
        private async Task FetchAnimeData()
        {
            try
            {
                IsBusy = true;

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
                IsBusy = false;
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
