using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class MainPage : ContentPage
    {
        private const string NyaaBaseUrl = "https://nyaa.si/?f=0&c=0_0&q={0}&s=seeders&o=desc";
        private Dictionary<string, string> resultsDictionary = new Dictionary<string, string>();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            string animeName = AnimeNameEntry.Text;

            if (!string.IsNullOrEmpty(animeName))
            {
                var results = await SearchNyaaAsync(animeName);
                ResultsListView.ItemsSource = results.Keys; // Set titles as the source
                resultsDictionary = results; // Store the results dictionary
            }
        }

        private async Task<Dictionary<string, string>> SearchNyaaAsync(string animeName)
        {
            var resultTitles = new Dictionary<string, string>();
            string url = string.Format(NyaaBaseUrl, Uri.EscapeDataString(animeName));

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // Select the title nodes
                    var titleNodes = htmlDoc.DocumentNode.SelectNodes("//a[not(contains(@class, 'comments')) and contains(@href, '/view/')]");
                    if (titleNodes != null)
                    {
                        foreach (var node in titleNodes)
                        {
                            if (resultTitles.Count >= 10)
                                break;

                            var title = node.GetAttributeValue("title", string.Empty);
                            var href = node.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                            {
                                var fullUrl = "https://nyaa.si" + href; // Construct the full URL
                                resultTitles[title] = fullUrl;
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle request exceptions
                    await DisplayAlert("Error", "Error fetching results: " + ex.Message, "OK");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    await DisplayAlert("Error", "An unexpected error occurred: " + ex.Message, "OK");
                }
            }

            return resultTitles;
        }

        private void OnResultSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                ProceedBtn.IsEnabled = true; // Enable the Proceed button
            }
        }

        private async void OnProceedButtonClicked(object sender, EventArgs e)
        {
            var selectedResult = ResultsListView.SelectedItem as string;
            if (selectedResult != null && resultsDictionary.TryGetValue(selectedResult, out var url))
            {
                var torrentDetails = await GetTorrentDetailsAsync(url);

                //torrentDetails.MagnetLink;
                //C:\Users\D7o_m\source\repos\Nyaa Streamer\Flyleaf

                //string exePath = Path.Combine(Directory.GetCurrentDirectory(), @"Flyleaf\FlyleafPlayer.exe");
                //string exePath = @"C:\Users\D7o_m\source\repos\Nyaa Streamer\Flyleaf\FlyleafPlayer.exe";
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Flyleaf\FlyleafPlayer.exe");
                string arguments = torrentDetails.MagnetLink;  // Example argument

                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = arguments;
                process.Start();
                process.WaitForExit();
                // Commenting out the alert box output
                // if (torrentDetails != null)
                // {
                //     await DisplayAlert("Torrent Details",
                //         $"Title: {torrentDetails.Title}\n" +
                //         $"View Link: {torrentDetails.ViewLink}\n" +
                //         $"Download Link: {torrentDetails.DownloadLink}\n" +
                //         $"Magnet Link: {torrentDetails.MagnetLink}",
                //         "OK");
                // }
                // else
                // {
                //     await DisplayAlert("Error", "Failed to fetch torrent details.", "OK");
                // }

                // Record the torrent details for future use
                // You can process or store these details as needed
                // For example:
                // SaveTorrentDetails(torrentDetails);
            }
        }

        private async Task<TorrentDetails> GetTorrentDetailsAsync(string url)
        {
            var torrentDetails = new TorrentDetails();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // Extract torrent details
                    torrentDetails.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim();
                    torrentDetails.ViewLink = url;
                    torrentDetails.DownloadLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '.torrent')]")?.GetAttributeValue("href", string.Empty);
                    torrentDetails.MagnetLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]")?.GetAttributeValue("href", string.Empty);

                }
                catch (HttpRequestException)
                {
                    // Handle request exceptions
                    return null;
                }
                catch
                {
                    // Handle other exceptions
                    return null;
                }
            }

            return torrentDetails;
        }

        private class TorrentDetails
        {
            public string Title { get; set; }
            public string ViewLink { get; set; }
            public string DownloadLink { get; set; }
            public string MagnetLink { get; set; }
        }
    }
}
