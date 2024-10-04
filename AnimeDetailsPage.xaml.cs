using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class AnimeDetailsPage : ContentPage
    {
        public AnimeDetailsPage(Anime anime)
        {
            InitializeComponent();
            BindingContext = anime; // Bind the selected anime to the details page
        }

        private async void OnWatchDownloadClicked(object sender, EventArgs e)
        {
            // Get the button that was clicked
            Button button = (Button)sender;

            // Get the parent StackLayout
            var parentStackLayout = (StackLayout)button.Parent;

            // Find the Entry named "EpisodeEntry"
            Entry episodeEntry = (Entry)parentStackLayout.FindByName("EpisodeEntry");

            // Get the text from the Entry (episode number)
            string episodeArgument = episodeEntry?.Text;

            // Get the selected anime from BindingContext
            Anime selectedAnime = (Anime)button.BindingContext;

            if (selectedAnime != null && !string.IsNullOrEmpty(episodeArgument))
            {
                // Show a popup with the anime title and episode number
                await DisplayAlert("Watch/Download", $"You clicked Watch/Download for {selectedAnime.Title}, Episode: {episodeArgument}", "OK");

                // Navigate to MainPage (or another page) and pass the anime title and episode number
                var mainPage = new MainPage();
                await Navigation.PushAsync(mainPage);

                // Call the method to handle the anime title and episode number in MainPage
                mainPage.OnReceiveAnimeTitle($"{selectedAnime.Title} Episode {episodeArgument}");
            }
            else
            {
                // Show a message if episode number is not entered
                await DisplayAlert("Error", "Please enter a valid episode number.", "OK");
            }
        }
    }
}
