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

     
        private void OnNyaaSearchClicked(object sender, EventArgs e)
        {
            var anime = BindingContext as Anime;
            if (anime != null)
            {
                // Redirect to Nyaa.si search with the anime title
                string nyaaUrl = $"https://nyaa.si/?f=0&c=0_0&q={anime.Title.Replace(" ", "+")}";
                Browser.OpenAsync(nyaaUrl, BrowserLaunchMode.SystemPreferred);
            }
        }


    }
}
