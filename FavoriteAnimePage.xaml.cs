using System.Collections.ObjectModel;

namespace AnimeLibraryApp
{
    public partial class FavoriteAnimePage : ContentPage
    {
        public ObservableCollection<Anime> FavoriteAnimeList { get; set; }

        public FavoriteAnimePage()
        {
            InitializeComponent();
            FavoriteAnimeList = new ObservableCollection<Anime>();
            BindingContext = this;

            // Load favorite anime (initially empty)
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            // Initially, no favorites are loaded
            // You can implement logic here to load from persistent storage if necessary
        }

        public void AddToFavorites(Anime anime)
        {
            // Add anime to favorites if it does not already exist
            if (!FavoriteAnimeList.Contains(anime))
            {
                FavoriteAnimeList.Add(anime);
                DisplayAlert("Added", $"{anime.Title} added to favorites.", "OK");
            }
            else
            {
                DisplayAlert("Already Favorited", $"{anime.Title} is already in your favorites.", "OK");
            }
        }

        private void OnRemoveFavoriteClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Anime selectedAnime = (Anime)button.BindingContext;

            // Remove from the favorite list
            FavoriteAnimeList.Remove(selectedAnime);
            // Optionally, show a message
            DisplayAlert("Removed", $"{selectedAnime.Title} removed from favorites.", "OK");
        }
    }
}
