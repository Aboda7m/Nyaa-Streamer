using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using static Nyaa_Streamer.TrendingAnimePage;

namespace Nyaa_Streamer
{
    public partial class WeeklySchedulePage : ContentPage
    {
        public ObservableCollection<string> DaysOfWeek { get; set; }
        public ObservableCollection<Anime> FilteredAnimeList { get; set; }

        private string selectedDay;

        public string SelectedDay
        {
            get => selectedDay;
            set
            {
                selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
                // Load anime based on the selected day
                LoadAnimeByDay(selectedDay.ToLower());
            }
        }

        public WeeklySchedulePage()
        {
            InitializeComponent();
            DaysOfWeek = new ObservableCollection<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            FilteredAnimeList = new ObservableCollection<Anime>();
            BindingContext = this;
        }

        // Method to fetch anime by day
        private async Task LoadAnimeByDay(string day)
        {
            try
            {
                string apiUrl = $"https://api.jikan.moe/v4/schedules/{day}";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<JikanApiResponse>(apiUrl);

                // Clear the existing list
                FilteredAnimeList.Clear();

                // Add the new list of anime airing on the selected day
                if (response != null && response.data != null)
                {
                    foreach (var animeData in response.data)
                    {
                        FilteredAnimeList.Add(new Anime
                        {
                            Title = animeData.title,
                            ImageUrl = animeData.images.jpg.image_url
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No anime found for this day.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load anime for this day: " + ex.Message, "OK");
            }
        }
    }
}
