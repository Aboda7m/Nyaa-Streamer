using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Nyaa_Streamer
{
    public partial class AnimeDetailsPage : ContentPage
    {
        // ObservableCollection to hold anime data (if needed)
        public ObservableCollection<AnimeData> AnimeCollection { get; set; } = new ObservableCollection<AnimeData>();

        private bool _isExpanded = false; // Track the expansion state

        public AnimeDetailsPage(Anime anime)
        {
            InitializeComponent();
            BindingContext = anime; // Bind the selected anime to the details page

            // Fetch additional details from Jikan API
            FetchAnimeDetailsFromJikan(anime);
        }

        // Method to fetch anime details from Jikan API
        private async void FetchAnimeDetailsFromJikan(Anime anime)
        {
            try
            {
                string jikanUrl = $"https://api.jikan.moe/v4/anime/{anime.Id}";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeData>(jikanUrl);

                if (response != null)
                {
                    anime.Synopsis = response.synopsis;
                    anime.Episodes = response.episodes;
                    anime.Score = response.score;

                    // Convert airing time from JST to GMT using the new method
                    if (response.broadcast != null)
                    {
                        anime.AiringTime = Anime.ConvertJSTToGMT(response.broadcast.day, response.broadcast.time);
                    }

                    // Notify UI of property changes
                    OnPropertyChanged(nameof(anime.Synopsis));
                    OnPropertyChanged(nameof(anime.Episodes));
                    OnPropertyChanged(nameof(anime.Score));
                    OnPropertyChanged(nameof(anime.AiringTime));
                }
                else
                {
                    await DisplayAlert("Error", "No anime details found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime details: " + ex.Message, "OK");
            }
        }




        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var parentStackLayout = (StackLayout)button.Parent;
            Entry episodeEntry = (Entry)parentStackLayout.FindByName("EpisodeEntry");
            string episodeArgument = episodeEntry?.Text;

            Anime selectedAnime = (Anime)button.BindingContext;

            if (selectedAnime != null)
            {
                string message = string.IsNullOrEmpty(episodeArgument)
                    ? $"You clicked Watch/Download for {selectedAnime.Title} (all episodes or season)"
                    : $"You clicked Watch/Download for {selectedAnime.Title}, Episode: {episodeArgument}";

                await DisplayAlert("Watch/Download", message, "OK");

                var mainPage = new MainPage();
                await Navigation.PushAsync(mainPage);

                string searchQuery = string.IsNullOrEmpty(episodeArgument)
                    ? selectedAnime.Title
                    : $"{selectedAnime.Title} {episodeArgument}";
                mainPage.OnReceiveAnimeTitle(searchQuery);
            }
            else
            {
                await DisplayAlert("Error", "No anime selected.", "OK");
            }
        }


        private void OnSynopsisTapped(object sender, EventArgs e)
        {
            var synopsisLabel = (Label)FindByName("SynopsisLabel");
            var scrollView = (ScrollView)FindByName("SynopsisScrollView");

            if (_isExpanded)
            {
                // Collapse the synopsis
                synopsisLabel.MaxLines = 3; // Show only the first three lines
                scrollView.HeightRequest = 100; // Reset scroll view height
            }
            else
            {
                // Expand the synopsis
                synopsisLabel.MaxLines = int.MaxValue; // Show full text
                scrollView.HeightRequest = double.NaN; // Remove height limit to show all text
            }

            _isExpanded = !_isExpanded; // Toggle the state
        }
    }
}
