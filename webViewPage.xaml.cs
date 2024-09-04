
// WebViewPage.xaml.cs
namespace Nyaa_Streamer
{
    public partial class webViewPage : ContentPage
    {
        public webViewPage(string link)
        {
            InitializeComponent();

            // Replace with your IP and port
            string localUrl = link;
            webView.Source = localUrl;
        }
    }
}
