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
        public class Anime
        {
            public string Title { get; set; }
            public string ImageUrl { get; set; }  // Anime image URL
            public string Synopsis { get; set; }  // Anime synopsis
        }

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
                if (response != null && response.Data != null)
                {
                    foreach (var animeData in response.Data)
                    {
                        // Add each anime to the ObservableCollection
                        SundayAnimeList.Add(new Anime
                        {
                            Title = animeData.Title,
                            Synopsis = animeData.Synopsis ?? "No synopsis available.",
                            ImageUrl = animeData.Images.Jpg.ImageUrl // Bind image URL
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
        public class JikanApiResponse
        {
            public AnimeData[] Data { get; set; }
        }

        public class AnimeData
        {
            public string Title { get; set; }
            public string Synopsis { get; set; }
            public AnimeImages Images { get; set; }
        }

        public class AnimeImages
        {
            public AnimeImageFormat Jpg { get; set; }
        }

        public class AnimeImageFormat
        {
            public string ImageUrl { get; set; }
        }
    }
}
