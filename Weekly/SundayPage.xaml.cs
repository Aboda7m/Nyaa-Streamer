using System;
using System.Collections.ObjectModel;
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

        // Fetch data for Sunday anime
        private async Task FetchSundayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=sunday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                SundayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        SundayAnimeList.Add(anime);
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
                IsBusy = false; // Hide loading indicator
            }
        }

        // Handle anime selection via TapGestureRecognizer
        private async void OnAnimeTapped(object sender, EventArgs e)
        {
            // Get the tapped Frame's BindingContext (which is the anime)
            var tappedFrame = (Frame)sender;
            var selectedAnime = tappedFrame.BindingContext as Anime;

            if (selectedAnime != null)
            {
                // Navigate to AnimeDetailsPage and pass the selected anime details
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }
    }
}
