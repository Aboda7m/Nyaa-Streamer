using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class FridayPage : ContentPage
    {
        public ObservableCollection<Anime> FridayAnimeList { get; set; }

        public FridayPage()
        {
            InitializeComponent();
            FridayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchFridayAnimeData();
        }

        // Fetch data from Jikan API using HttpClient.GetFromJsonAsync
        private async Task FetchFridayAnimeData()
        {
            try
            {
                // URL for Friday anime schedule
                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=friday";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

                // Clear the existing list
                FridayAnimeList.Clear();

                // Check if the response contains data
                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        // Add each anime to the ObservableCollection
                        FridayAnimeList.Add(new Anime
                        {
                            Title = animeData.title,
                            ImageUrl = animeData.images.jpg.image_url,
                            Id = animeData.mal_id, // Assign the ID from the API to the Anime object
                            Synopsis = animeData.synopsis,
                            Episodes = animeData.episodes
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for Friday.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime data: " + ex.Message, "OK");
            }
        }

        // Handle anime selection
        private async void OnAnimeSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Anime selectedAnime)
            {
                // Navigate to AnimeDetailsPage and pass the selected anime details
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));

                // Optionally, deselect the item
                ((ListView)sender).SelectedItem = null;
            }
        }
    }
}
