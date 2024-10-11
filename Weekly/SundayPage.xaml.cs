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

        // Fetch data from Jikan API using HttpClient.GetFromJsonAsync
        private async Task FetchSundayAnimeData()
        {
            try
            {
                // Display loading indicator (optional)
                IsBusy = true;

                // URL for Sunday anime schedule
                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=sunday";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

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
                            ImageUrl = animeData.images.jpg.image_url,
                            Id = animeData.mal_id, // Assign the ID from the API to the Anime object
                            Synopsis = animeData.synopsis,
                            Episodes = animeData.episodes
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
            finally
            {
                // Hide loading indicator
                IsBusy = false;
            }
        }

        // Handle anime selection (CollectionView uses SelectionChangedEventArgs instead of SelectedItemChangedEventArgs)
        private async void OnAnimeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                if (e.CurrentSelection[0] is Anime selectedAnime)
                {
                    // Navigate to AnimeDetailsPage and pass the selected anime details
                    await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));

                    // Optionally, deselect the item
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
        }
    }
}
