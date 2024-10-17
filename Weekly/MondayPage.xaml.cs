namespace Nyaa_Streamer.Weekly
{
    public partial class MondayPage : Nyaa_Streamer.Weekly.BaseDayPage
    {
        public MondayPage()
            : base("https://api.jikan.moe/v4/schedules?filter=monday", "Monday Anime Schedule")
        {
        }
    }
}
