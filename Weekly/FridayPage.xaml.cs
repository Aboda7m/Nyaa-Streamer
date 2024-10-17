namespace Nyaa_Streamer.Weekly
{
    public partial class FridayPage : BaseDayPage
    {
        public FridayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=friday", "Friday Anime Schedule")
        {
        }
    }
}
