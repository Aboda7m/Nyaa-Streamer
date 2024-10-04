using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class BaseDay : ContentPage
    {
        public ObservableCollection<Anime> AnimeList { get; set; }
        public string PageTitle { get; set; }

        public BaseDay(string day)
        {
            InitializeComponent();
            AnimeList = new ObservableCollection<Anime>();
            PageTitle = $"{day} Anime Schedule"; // Set the page title
            BindingContext = this;
            FetchAnimeData(day);
        }

        private async Task FetchAnimeData(string day)
        {
            try
            {
                // Use day to construct the API URL
                string apiUrl = $"https://api.jikan.moe/v4/schedules?filter={day.ToLower()}"; // Assumes the API accepts day names in lowercase
                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

                AnimeList.Clear();

                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        AnimeList.Add(new Anime
                        {
                            Title = animeData.title,
                            ImageUrl = animeData.images.jpg.image_url,
                            Id = animeData.mal_id,
                            Synopsis = animeData.synopsis,
                            Episodes = animeData.episodes
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for " + day, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime data: " + ex.Message, "OK");
            }
        }

        private async void OnAnimeSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Anime selectedAnime)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
                ((ListView)sender).SelectedItem = null; // Deselect the item
            }
        }
    }
}
