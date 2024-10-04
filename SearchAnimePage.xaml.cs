using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class SearchAnimePage : ContentPage
    {
        public ObservableCollection<Anime> AnimeList { get; set; } = new ObservableCollection<Anime>();
        public bool IsLoading { get; set; }

        public SearchAnimePage()
        {
            InitializeComponent();
            BindingContext = this; // Set the page's BindingContext to itself
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = SearchBar.Text?.Trim();

            if (!string.IsNullOrEmpty(query))
            {
                await SearchAnimeAsync(query);
            }
        }

        private async Task SearchAnimeAsync(string query)
        {
            try
            {
                IsLoading = true;
                OnPropertyChanged(nameof(IsLoading));

                using HttpClient client = new HttpClient();
                string apiUrl = $"https://api.jikan.moe/v4/anime?q={query}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AnimeApiResponse>(json);

                AnimeList.Clear();
                foreach (var anime in result.data)
                {
                    AnimeList.Add(new Anime
                    {
                        Title = anime.title,
                        ImageUrl = anime.images.jpg.image_url,
                        Id = anime.mal_id, // Assign the ID from the API to the Anime object
                        Synopsis = anime.synopsis,
                        Episodes = anime.episodes
                    });
                }

                OnPropertyChanged(nameof(AnimeList));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load anime: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            }
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
