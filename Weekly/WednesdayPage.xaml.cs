namespace Nyaa_Streamer.Weekly
{
    public partial class WednesdayPage : BaseDayPage
    {
        public WednesdayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=wednesday", "Wednesday Anime Schedule")
        {
        }
    }
}
