using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class MondayPage : ContentPage
    {
        public ObservableCollection<Anime> MondayAnimeList { get; set; }

        public MondayPage()
        {
            InitializeComponent();
            MondayAnimeList = new ObservableCollection<Anime>();

            // Set the BindingContext for data binding
            BindingContext = this;

            // Fetch anime data when the page loads
            FetchMondayAnimeData();
        }

        // Fetch data for Monday anime
        private async Task FetchMondayAnimeData()
        {
            try
            {
                IsBusy = true; // Optional loading indicator

                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=monday";

                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                MondayAnimeList.Clear(); // Clear existing list

                if (animeList != null && animeList.Count > 0)
                {
                    foreach (var anime in animeList)
                    {
                        MondayAnimeList.Add(anime);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for Monday.", "OK"); // Corrected error message
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
        private async void OnAnimeSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedAnime = e.CurrentSelection.FirstOrDefault() as Anime;

            if (selectedAnime != null)
            {
                // Navigate to AnimeDetailsPage and pass the selected anime details
                await Navigation.PushAsync(new AnimeDetailsPage(selectedAnime));
            }
        }
    }
}
