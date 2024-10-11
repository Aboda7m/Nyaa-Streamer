using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class WednesdayPage : ContentPage
    {
        public ObservableCollection<Anime> WednesdayAnimeList { get; set; }

        public WednesdayPage()
        {
            InitializeComponent();
            WednesdayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchWednesdayAnimeData();
        }

        // Fetch data for Wednesday anime
        private async Task FetchWednesdayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=wednesday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                WednesdayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        WednesdayAnimeList.Add(anime);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for Wednesday.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime data: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false; // Hide loading indicator
            }
        }

        // Handle anime selection
        private async void OnAnimeSelected(object sender, EventArgs e)
        {
            var selectedAnime = ((Grid)sender).BindingContext as Anime;

            if (selectedAnime != null)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }
    }
}
