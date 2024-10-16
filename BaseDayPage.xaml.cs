using System.Collections.ObjectModel;

namespace Nyaa_Streamer;

[QueryProperty(nameof(Day), "day")]
public partial class BaseDayPage : ContentPage
{
    // Properties for binding
    public string Day { get; set; }
    public string PageTitle { get; set; }
    public ObservableCollection<Anime> AnimeList { get; set; }

    // Dictionary to map days to API URLs
    private readonly Dictionary<string, string> dayApiUrls = new Dictionary<string, string>
    {
        { "Sunday", "https://api.jikan.moe/v4/schedules?filter=sunday" },
        { "Monday", "https://api.jikan.moe/v4/schedules?filter=monday" },
        { "Tuesday", "https://api.jikan.moe/v4/schedules?filter=tuesday" },
        { "Wednesday", "https://api.jikan.moe/v4/schedules?filter=wednesday" },
        { "Thursday", "https://api.jikan.moe/v4/schedules?filter=thursday" },
        { "Friday", "https://api.jikan.moe/v4/schedules?filter=friday" },
        { "Saturday", "https://api.jikan.moe/v4/schedules?filter=saturday" }
    };

    // Default constructor, required for Shell navigation
    public BaseDayPage()
    {
        InitializeComponent();
        AnimeList = new ObservableCollection<Anime>();
        BindingContext = this;
    }

    // This method gets called when the page is about to appear
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(Day))
        {
            PageTitle = $"{Day} Anime Schedule";  // Dynamically set the page title
            if (dayApiUrls.ContainsKey(Day))
            {
                string apiUrl = dayApiUrls[Day];
                LoadAnimeSchedule(apiUrl);  // Fetch and load anime data
            }
            else
            {
                DisplayAlert("Error", "Invalid day selected.", "OK");
            }
        }
    }

    // Method to fetch anime schedule from API and populate UI
    private async void LoadAnimeSchedule(string apiUrl)
    {
        try
        {
            // Fetch anime details from the API
            var animeData = await Anime.FetchAnimeDetailsAsync(apiUrl);

            // Update AnimeList, which will automatically update the UI
            AnimeList.Clear();
            foreach (var anime in animeData)
            {
                AnimeList.Add(anime);
            }
        }
        catch (Exception ex)
        {
            // Show an error alert in case of failure
            await DisplayAlert("Error", $"Failed to load anime schedule: {ex.Message}", "OK");
        }
    }

    // Event handler when an anime item is selected
    private async void OnAnimeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Anime selectedAnime)
        {
            // Handle anime selection (e.g., navigate to a detailed view)
            await DisplayAlert("Selected", $"You selected {selectedAnime.Title}", "OK");
        }
    }
}
