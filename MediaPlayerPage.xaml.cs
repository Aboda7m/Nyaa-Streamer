// MediaPlayerPage.xaml.cs
namespace Nyaa_Streamer
{
    public partial class MediaPlayerPage : ContentPage
    {
        public MediaPlayerPage(string mediaSourceUrl)
        {
            InitializeComponent();

            // Set the media source URL to the MediaElement
            mediaElement.Source = new Uri(mediaSourceUrl);
        }
    }
}
