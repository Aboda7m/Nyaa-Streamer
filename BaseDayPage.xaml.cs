using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    [QueryProperty(nameof(DayName), "dayName")]
    public partial class BaseDayPage : ContentPage
    {
        private string _dayName;

        public string DayName
        {
            get => _dayName;
            set
            {
                Console.WriteLine($"Setting DayName to: {value}"); // Log for debugging
                _dayName = value;
                DayLabel.Text = $"Today is {_dayName}"; // Update label immediately when property is set
            }
        }

        public BaseDayPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // No need to set DayLabel again, it's already set in the property setter
        }
    }
}
