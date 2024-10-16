using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class WeeklySchedulePage : Shell
    {
        public WeeklySchedulePage()
        {
            InitializeComponent();

            // Add an event handler for ShellNavigating to manage tab changes
            Navigating += OnNavigating;
        }

        // This method handles tab selection changes to navigate with day parameter
        private async void OnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            // Extract the target route to determine which tab was selected
            var selectedRoute = e.Target.Location.OriginalString;

            string day = selectedRoute switch
            {
                "sundayPage" => "Sunday",
                "mondayPage" => "Monday",
                "tuesdayPage" => "Tuesday",
                "wednesdayPage" => "Wednesday",
                "thursdayPage" => "Thursday",
                "fridayPage" => "Friday",
                "saturdayPage" => "Saturday",
                _ => "Sunday"  // Default to Sunday if no valid tab is found
            };

            // Navigate to the BaseDayPage with the appropriate day argument
            if (e.Target.Location.OriginalString != e.Current?.Location.OriginalString)
            {
                await Shell.Current.GoToAsync($"BaseDayPage?day={day}");
            }
        }
    }
}
