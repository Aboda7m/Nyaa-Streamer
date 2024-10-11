using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class SearchAnimePage : ContentPage
    {
        public ObservableCollection<Anime> AnimeList { get; set; } = new ObservableCollection<Anime>();
        public ObservableCollection<Anime> AnimeSuggestions { get; set; } = new ObservableCollection<Anime>();
        public bool IsLoading { get; set; }
        public bool AreSuggestionsVisible { get; set; }
        private CancellationTokenSource cts = new CancellationTokenSource();

        public SearchAnimePage()
        {
            InitializeComponent();
            BindingContext = this;
            AreSuggestionsVisible = false;
        }

        // Handles text change for real-time suggestions
        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.Trim();

            // Cancel previous search if the user is typing quickly
            cts.Cancel();
            cts = new CancellationTokenSource();

            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    // Delay execution to debounce the search
                    await Task.Delay(300, cts.Token); // Task delay with cancellation support

                    if (!cts.Token.IsCancellationRequested)
                    {
                        await FetchSuggestionsAsync(query);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore task cancellation
                }
            }
            else
            {
                AreSuggestionsVisible = false;
                OnPropertyChanged(nameof(AreSuggestionsVisible));
            }
        }

        // Main search on pressing Enter in the search bar
        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string query = SearchBar.Text?.Trim();

            if (!string.IsNullOrEmpty(query))
            {
                // Clear suggestions
                AreSuggestionsVisible = false;
                OnPropertyChanged(nameof(AreSuggestionsVisible));

                // Perform main search
                await SearchAnimeAsync(query);
            }
        }

        // Fetch anime suggestions in real-time
        private async Task FetchSuggestionsAsync(string query)
        {
            try
            {
                IsLoading = true;
                OnPropertyChanged(nameof(IsLoading));

                using HttpClient client = new HttpClient();
                string apiUrl = $"https://api.jikan.moe/v4/anime?q={query}";
                HttpResponseMessage response = await client.GetAsync(apiUrl, cts.Token); // Use cancellation token
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync(cts.Token); // Use the token
                var result = JsonSerializer.Deserialize<AnimeApiResponse>(json);

                AnimeSuggestions.Clear();
                foreach (var anime in result.data)
                {
                    AnimeSuggestions.Add(new Anime
                    {
                        Title = anime.title,
                        ImageUrl = anime.images.jpg.image_url,
                        Id = anime.mal_id,
                        Synopsis = anime.synopsis,
                        Episodes = anime.episodes
                    });
                }

                AreSuggestionsVisible = AnimeSuggestions.Count > 0;
                OnPropertyChanged(nameof(AreSuggestionsVisible));
                OnPropertyChanged(nameof(AnimeSuggestions));
            }
            catch (TaskCanceledException)
            {
                // Ignore task cancellation
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load suggestions: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // Fetch anime based on the selected suggestion
        private async void OnSuggestionTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedAnime = e.Item as Anime;
            if (selectedAnime != null)
            {
                // Perform a search based on the selected suggestion
                SearchBar.Text = selectedAnime.Title;
                AreSuggestionsVisible = false;
                OnPropertyChanged(nameof(AreSuggestionsVisible));

                await SearchAnimeAsync(selectedAnime.Title);
            }
        }

        // Display anime list with images after hitting Enter
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
                        Id = anime.mal_id,
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
                AreSuggestionsVisible = false;
                OnPropertyChanged(nameof(AreSuggestionsVisible));

                // Navigate to anime details page
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }
    }

   
}
