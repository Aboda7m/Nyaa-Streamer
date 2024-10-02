using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
            LoadTrendingAnime();
        }

        // Method to fetch trending anime using Jikan API
        private async void LoadTrendingAnime()
        {
            try
            {
                // Replace the static data with a call to the Jikan API
                string apiUrl = "https://api.jikan.moe/v4/top/anime";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<JikanApiResponse>(apiUrl);

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
                            ImageUrl = animeData.images.jpg.image_url
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

        // Placeholder for Watch/Download button functionality
        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Anime selectedAnime = (Anime)button.BindingContext;

            // Show a popup with the anime title
            await DisplayAlert("Watch/Download", $"You clicked Watch/Download for {selectedAnime.Title}", "OK");

            // Return the anime title as a string (you can use it for further processing)
            string animeTitle = selectedAnime.Title;

            // Here you can call a method to search on nyaa.si with the animeTitle
            SearchOnNyaaSi(animeTitle);
        }

        // Method to handle searching on nyaa.si
        private void SearchOnNyaaSi(string animeTitle)
        {
            // Implement your logic to search on nyaa.si using the animeTitle
            // This could be opening a browser or passing the title to another method
            // Example: 
            // var url = $"https://nyaa.si/?f=0&c=0_0&q={Uri.EscapeDataString(animeTitle)}";
            // Launcher.OpenAsync(url); // This would open the URL in the default browser (if using Xamarin.Essentials)
        }


        // Model for Anime
        public class Anime
        {
            public string Title { get; set; }
            public string ImageUrl { get; set; }
        }

        // Model for API responseemo
        public class JikanApiResponse
        {
            public List<AnimeData> data { get; set; }
        }

        // Model for each anime in the API response
        public class AnimeData
        {
            public string title { get; set; }
            public AnimeImages images { get; set; }
        }

        // Model for anime images
        public class AnimeImages
        {
            public AnimeImageTypes jpg { get; set; }
        }

        // Model for image types
        public class AnimeImageTypes
        {
            public string image_url { get; set; }
        }
    }
}
