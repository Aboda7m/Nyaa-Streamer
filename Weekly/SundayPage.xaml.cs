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

        // Fetch data for Sunday anime using the existing method in Anime class
        private async Task FetchSundayAnimeData()
        {
            try
            {
                // Display loading indicator (optional)
                IsBusy = true;

                // URL for Sunday anime schedule
                string apiUrl = "https://api.jikan.moe/v4/schedules?filter=sunday";

                // Fetch anime details using the existing method
                var animeList = await Anime.FetchAnimeDetailsAsync(apiUrl);

                // Clear the existing list
                SundayAnimeList.Clear();

                // Add each anime to the ObservableCollection
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
