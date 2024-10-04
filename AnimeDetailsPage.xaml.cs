using System.Net.Http;
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
                // Assuming that the Anime class contains an ID property (replace this with the correct ID source)
                string jikanUrl = $"https://api.jikan.moe/v4/anime/{anime.Id}"; // Use anime ID in the API URL

                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(jikanUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the response JSON
                    var jikanResponse = JsonSerializer.Deserialize<AnimeApiResponse>(jsonResponse);

                    if (jikanResponse != null)
                    {
                        var animeData = jikanResponse.data[0]; // Adjust indexing as necessary

                        // Update anime properties based on the API response
                        anime.Synopsis = animeData.synopsis;
                        anime.Episodes = animeData.episodes; // Assuming episodes is part of the response

                        // Notify UI of property changes if needed
                        OnPropertyChanged(nameof(anime.Synopsis));
                        OnPropertyChanged(nameof(anime.Episodes));
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to retrieve anime details.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            // Get the button that was clicked
            Button button = (Button)sender;

            // Get the parent StackLayout
            var parentStackLayout = (StackLayout)button.Parent;

            // Find the Entry named "EpisodeEntry"
            Entry episodeEntry = (Entry)parentStackLayout.FindByName("EpisodeEntry");

            // Get the text from the Entry (episode number)
            string episodeArgument = episodeEntry?.Text;

            // Get the selected anime from BindingContext
            Anime selectedAnime = (Anime)button.BindingContext;

            if (selectedAnime != null && !string.IsNullOrEmpty(episodeArgument))
            {
                // Show a popup with the anime title and episode number
                await DisplayAlert("Watch/Download", $"You clicked Watch/Download for {selectedAnime.Title}, Episode: {episodeArgument}", "OK");

                // Navigate to MainPage (or another page) and pass the anime title and episode number
                var mainPage = new MainPage();
                await Navigation.PushAsync(mainPage);

                // Call the method to handle the anime title and episode number in MainPage
                mainPage.OnReceiveAnimeTitle($"{selectedAnime.Title} {episodeArgument}");
            }
            else
            {
                // Show a message if episode number is not entered
                await DisplayAlert("Error", "Please enter a valid episode number.", "OK");
            }
        }
    }
}
