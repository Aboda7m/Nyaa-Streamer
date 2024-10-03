using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class SundayPage : ContentPage
    {
        public ObservableCollection<Anime> SundayAnimeList { get; set; }

        public SundayPage()
        {
            InitializeComponent();
            SundayAnimeList = new ObservableCollection<Anime>
            {
                new Anime { Title = "Placeholder Anime 1", Synopsis = "This is a placeholder synopsis for anime 1." },
                new Anime { Title = "Placeholder Anime 2", Synopsis = "This is a placeholder synopsis for anime 2." },
                new Anime { Title = "Placeholder Anime 3", Synopsis = "This is a placeholder synopsis for anime 3." }
            };
            AnimeListView.ItemsSource = SundayAnimeList; // Set the ItemSource for the ListView
        }

        // Model for Anime
        public class Anime
        {
            public string Title { get; set; }
            public string Synopsis { get; set; }
        }
    }
}
