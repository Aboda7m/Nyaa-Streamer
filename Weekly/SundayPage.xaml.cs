using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
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
            AnimeListView.ItemsSource = SundayAnimeList; // Set the ItemSource for the ListView

            // Fetch anime data when the page loads
            FetchSundayAnimeData();
        }

        // Model for Anime
        public class Anime
        {
            public string Title { get; set; }
            public string Synopsis { get; set; }
        }

        // Fetch data from Jikan API
        private async void FetchSundayAnimeData()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Send GET request to Jikan API
                    string url = "https://api.jikan.moe/v4/schedules?filter=sunday";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonSerializer.Deserialize<AnimeApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (apiResponse?.Data != null)
                        {
                            foreach (var anime in apiResponse.Data)
                            {
                                SundayAnimeList.Add(new Anime
                                {
                                    Title = anime.Title,
                                    Synopsis = anime.Synopsis ?? "No synopsis available."
                                });
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to load anime data", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // Model for API response
        public class AnimeApiResponse
        {
            public AnimeData[] Data { get; set; }
        }

        public class AnimeData
        {
            public string Title { get; set; }
            public string Synopsis { get; set; }
        }
    }
}
