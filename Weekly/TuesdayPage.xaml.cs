using System;
using System.Collections.ObjectModel;
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

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchTuesdayAnimeData();
        }

        // Fetch data for Tuesday anime
        private async Task FetchTuesdayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=tuesday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                TuesdayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        TuesdayAnimeList.Add(anime);
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
            finally
            {
                IsBusy = false; // Hide loading indicator
            }
        }

        // Handle anime selection
        private async void OnAnimeSelected(object sender, EventArgs e)
        {
            // Check if the sender is a Frame or Grid and retrieve the BindingContext
            var selectedFrame = sender as Frame;
            var selectedGrid = sender as Grid;

            Anime selectedAnime = null;

            if (selectedFrame != null)
            {
                selectedAnime = selectedFrame.BindingContext as Anime;
            }
            else if (selectedGrid != null)
            {
                selectedAnime = selectedGrid.BindingContext as Anime;
            }

            // Navigate to AnimeDetailsPage if the selected anime is not null
            if (selectedAnime != null)
            {
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }
    }
}
