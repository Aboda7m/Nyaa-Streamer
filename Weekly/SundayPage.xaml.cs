using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class SundayPage : ContentPage
    {
        public ObservableCollection<Anime> SundayAnimeList { get; set; }

        public SundayPage()
        {
            InitializeComponent();
            SundayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchSundayAnimeData();
        }

        // Model for Anime
       

        // Fetch data from Jikan API using HttpClient.GetFromJsonAsync
        private async Task FetchSundayAnimeData()
        {
            try
            {
                // URL for Sunday anime schedule
                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=sunday";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<JikanApiResponse>(apiUrl);

                // Clear the existing list
                SundayAnimeList.Clear();

                // Check if the response contains data
                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        // Add each anime to the ObservableCollection
                        SundayAnimeList.Add(new Anime
                        {
                            Title = animeData.title,
                            
                            ImageUrl = animeData.images.jpg.image_url // Bind image URL
                        });
                    }
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
        }

        // API response models
        public class Anime
        {
            public string Title { get; set; }
            public string ImageUrl { get; set; }
        }

        // Model for API response
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
