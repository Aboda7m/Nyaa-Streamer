using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class ThursdayPage : ContentPage
    {
        public ObservableCollection<Anime> ThursdayAnimeList { get; set; }

        public ThursdayPage()
        {
            InitializeComponent();
            ThursdayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchThursdayAnimeData();
        }

        // Fetch data for Thursday anime
        private async Task FetchThursdayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=thursday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                ThursdayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        ThursdayAnimeList.Add(anime);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for Thursday.", "OK");
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
