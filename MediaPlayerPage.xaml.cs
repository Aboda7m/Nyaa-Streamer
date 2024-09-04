
// MediaPlayerPage.xaml.cs
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class MediaPlayerPage : ContentPage
    {
        public MediaPlayerPage(string mediaSourceUrl)
        {
            InitializeComponent();

            // Set the media source URL to the MediaElement
            Debug.WriteLine("new Uri(mediaSourceUrl); " + mediaSourceUrl);
            mediaElement.Source = mediaSourceUrl;
            mediaElement.Play();
        }
    }
}
