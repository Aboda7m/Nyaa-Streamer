namespace Nyaa_Streamer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("BaseDayPage", typeof(BaseDayPage));
        }
    }
}
