using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class TrendingAnimePage : ContentPage
    {
        public ObservableCollection<Anime> TrendingAnimeList { get; set; }

        public TrendingAnimePage()
        {
            InitializeComponent();
            TrendingAnimeList = new ObservableCollection<Anime>();

            // Set Binding Context for data binding
            BindingContext = this;

            // Fetch trending anime when the page loads
            LoadTrendingAnime();
        }

        // Method to handle the refresh button click
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Refreshing", "Refreshing anime list...", "OK");
            await LoadTrendingAnime();
        }

        // Method to fetch trending anime using Jikan API
        private async Task LoadTrendingAnime()
        {
            try
            {
                // Replace the static data with a call to the Jikan API
                string apiUrl = "https://api.jikan.moe/v4/top/anime?type=tv&filter=airing&page=1&limit=10";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

                // Clear the existing list
                TrendingAnimeList.Clear();

                // Check if the response contains data
                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        // Add each anime to the ObservableCollection
                        TrendingAnimeList.Add(new Anime
                        {
                            Title = animeData.title,
                            ImageUrl = animeData.images.jpg.image_url,
                            Id = animeData.mal_id, // Assign the ID from the API to the Anime object
                            Synopsis = animeData.synopsis//,
                           // Episodes = animeData.episodes

                        });
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

        // Method to handle Watch/Download button functionality
        // Method to handle Watch/Download button functionality
        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var parentStackLayout = (StackLayout)button.Parent; // Get the parent StackLayout of the Button

            // Find the Entry in the parent StackLayout
            Entry episodeEntry = (Entry)parentStackLayout.FindByName("EpisodeEntry");

            // Get the text from the Entry
            string episodeArgument = episodeEntry.Text;

            Anime selectedAnime = (Anime)button.BindingContext;

            // Show a popup with the anime title
            await DisplayAlert("Watch/Download", $"You clicked Watch/Download for {selectedAnime.Title} {episodeArgument}", "OK");

            // Navigate to the MainPage and pass the anime title
            var mainPage = new MainPage();
            await Navigation.PushAsync(mainPage);
            mainPage.OnReceiveAnimeTitle($"{selectedAnime.Title} {episodeArgument}"); // Call the method to handle the title
        }

        private async void OnAnimeSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedAnime = e.CurrentSelection.FirstOrDefault() as Anime;
            if (selectedAnime != null)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }



    }
}
