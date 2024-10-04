using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class AnimeDetailsPage : ContentPage
    {
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
                string jikanUrl = $"https://api.jikan.moe/v4/anime/01"; // Use anime ID in the API URL

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(jikanUrl);

                // Check if the response contains data
                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        // Add each anime to the ObservableCollection
                       
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No trending anime found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load trending anime: " + ex.Message, "OK");
            }
        }

        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var parentStackLayout = (StackLayout)button.Parent;
            Entry episodeEntry = (Entry)parentStackLayout.FindByName("EpisodeEntry");
            string episodeArgument = episodeEntry?.Text;

            Anime selectedAnime = (Anime)button.BindingContext;

            if (selectedAnime != null && !string.IsNullOrEmpty(episodeArgument))
            {
                await DisplayAlert("Watch/Download", $"You clicked Watch/Download for {selectedAnime.Title}, Episode: {episodeArgument}", "OK");
                var mainPage = new MainPage();
                await Navigation.PushAsync(mainPage);
                mainPage.OnReceiveAnimeTitle($"{selectedAnime.Title} {episodeArgument}");
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid episode number.", "OK");
            }
        }
    }
}
