using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class TuesdayPage : ContentPage
    {
        public ObservableCollection<Anime> TuesdayAnimeList { get; set; }

        public TuesdayPage()
        {
            InitializeComponent();
            TuesdayAnimeList = new ObservableCollection<Anime>();
            BindingContext = this;

            FetchTuesdayAnimeData();
        }

        private async Task FetchTuesdayAnimeData()
        {
            try
            {
                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=tuesday";
                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

                TuesdayAnimeList.Clear();

                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        TuesdayAnimeList.Add(new Anime
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
                    await DisplayAlert("Error", "No anime found for Tuesday.", "OK");
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
                ((ListView)sender).SelectedItem = null;
            }
        }
    }
}
