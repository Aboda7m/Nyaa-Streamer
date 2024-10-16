using System.Collections.ObjectModel;

public partial class BaseDayPage : ContentPage
{
    public string Day { get; set; }
    public string PageTitle { get; set; }
    public ObservableCollection<AnimeItem> AnimeList { get; set; }

    // A dictionary to hold API keys for each day
    private Dictionary<string, string> dayApiKeys = new Dictionary<string, string>
    {
        { "Sunday", "API_KEY_SUNDAY" },
        { "Monday", "API_KEY_MONDAY" },
        { "Tuesday", "API_KEY_TUESDAY" },
        { "Wednesday", "API_KEY_WEDNESDAY" },
        { "Thursday", "API_KEY_THURSDAY" },
        { "Friday", "API_KEY_FRIDAY" },
        { "Saturday", "API_KEY_SATURDAY" }
    };

    public BaseDayPage(string day)
    {
        InitializeComponent();
        Day = day;
        PageTitle = $"{day} Anime Schedule";  // Set the dynamic page title

        // Bind the dynamic properties to the UI
        BindingContext = this;

        // Fetch the anime data for the day using the correct API key
        if (dayApiKeys.ContainsKey(day))
        {
            string apiKey = dayApiKeys[day];
            LoadAnimeSchedule(apiKey);  // Load data from the API
        }
        else
        {
            throw new Exception("Invalid day provided.");
        }
    }

    private async void LoadAnimeSchedule(string apiKey)
    {
        // Simulate an API call using the API key for the specific day
        await Task.Delay(1000); // Simulate API delay

        // Example: Populate AnimeList with dummy data or data from an actual API call
        AnimeList = new ObservableCollection<AnimeItem>
        {
            new AnimeItem { Title = "Anime 1", Synopsis = "A cool anime.", ImageUrl = "image1.jpg", AiringTimeLocal = "9:00 AM" },
            new AnimeItem { Title = "Anime 2", Synopsis = "An even cooler anime.", ImageUrl = "image2.jpg", AiringTimeLocal = "12:00 PM" }
        };

        // Set the BindingContext to update the UI
        BindingContext = this;
    }
}
