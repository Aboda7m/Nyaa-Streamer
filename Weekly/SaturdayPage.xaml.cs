namespace Nyaa_Streamer.Weekly
{
    public partial class SaturdayPage : BaseDayPage
    {
        public SaturdayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=saturday", "Saturday Anime Schedule")
        {
        }
    }
}
