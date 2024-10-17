namespace Nyaa_Streamer.Weekly
{
    public partial class SundayPage : BaseDayPage
    {
        public SundayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=sunday", "Sunday Anime Schedule")
        {
        }
    }
}
