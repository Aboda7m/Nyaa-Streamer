namespace Nyaa_Streamer.Weekly
{
    public partial class TuesdayPage : BaseDayPage
    {
        public TuesdayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=tuesday", "Tuesday Anime Schedule")
        {
        }
    }
}
