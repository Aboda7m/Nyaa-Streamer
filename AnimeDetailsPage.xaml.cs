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
        string jikanUrl = $"https://api.jikan.moe/v4/anime/{anime.Id}"; // Use anime ID in the API URL

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(jikanUrl);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Parse the response JSON
            var jikanResponse = JsonSerializer.Deserialize<AnimeApiResponse>(jsonResponse);

            if (jikanResponse != null && jikanResponse.data.Length > 0) // Ensure there is at least one entry
            {
                var animeData = jikanResponse.data; // Access the first AnimeData object

                // Update anime properties based on the API response
                anime.Synopsis = animeData.synopsis;
                anime.Episodes = animeData.episodes;

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