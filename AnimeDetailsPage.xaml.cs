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
                var response = await client.GetFromJsonAsync<AnimeData>(jikanUrl); // Use correct response type

                // Check if the response contains data
                if (response != null && response != null )
                {
                    var animeData = response; // Access the first AnimeData object from the list

                    // Update the properties of the bound Anime object
                    anime.Synopsis = animeData.synopsis;
                    //anime.Episodes = animeData.episodes; // Assuming episodes is an integer

                    // Notify UI of property changes
                    OnPropertyChanged(nameof(anime.Synopsis));
                    OnPropertyChanged(nameof(anime.Episodes));
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

    // Correct response type for anime details
   
}
