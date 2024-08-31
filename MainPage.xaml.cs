using HtmlAgilityPack;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SuRGeoNix;
using SuRGeoNix.BitSwarmLib;
using SuRGeoNix.BitSwarmLib.BEP;
// Include BitSwarm namespace

namespace Nyaa_Streamer
{
    public partial class MainPage : ContentPage
    {
        private const string NyaaBaseUrl = "https://nyaa.si/?f=0&c=0_0&q={0}&s=seeders&o=desc";
        private readonly HttpClient httpClient = new HttpClient();
        private Dictionary<string, TorrentDetails> resultsDictionary = new Dictionary<string, TorrentDetails>();
        private TorrentDetails? selectedTorrentDetails;

        public MainPage()
        {
            InitializeComponent();
            // Set a default User-Agent header
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // Initialize ListView and Button references
            ResultsListView.ItemsSource = new List<string>(); // Ensure it's initialized
            SaveDetailsButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            string animeName = AnimeNameEntry.Text;

            if (!string.IsNullOrEmpty(animeName))
            {
                var results = await SearchNyaaAsync(animeName);

                // Clear previous results
                (ResultsListView.ItemsSource as List<string>)?.Clear();
                resultsDictionary.Clear();

                // Transform and add results directly
                var titles = results.Keys.ToList();
                ResultsListView.ItemsSource = titles; // Update ListView with titles
                resultsDictionary = results; // Assign the results dictionary
            }
            else
            {
                await DisplayAlert("Input Error", "Please enter an anime name.", "OK");
            }
        }

        private async Task<Dictionary<string, TorrentDetails>> SearchNyaaAsync(string animeName)
        {
            var resultTitles = new Dictionary<string, TorrentDetails>();
            string url = string.Format(NyaaBaseUrl, Uri.EscapeDataString(animeName));

            try
            {
                var response = await httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                var titleNodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, '/view/') and not(contains(@class, 'comments'))]");
                if (titleNodes != null)
                {
                    var tasks = titleNodes.Take(10).Select(async node =>
                    {
                        try
                        {
                            var title = node.GetAttributeValue("title", string.Empty);
                            var href = node.GetAttributeValue("href", string.Empty);

                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                            {
                                var fullUrl = "https://nyaa.si" + href;
                                var details = await GetTorrentDetailsAsync(title, fullUrl);
                                if (details != null)
                                {
                                    lock (resultTitles)
                                    {
                                        resultTitles[title] = details;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle individual task errors
                            System.Diagnostics.Debug.WriteLine($"Error processing node: {ex.Message}");
                        }
                    }).ToList();

                    await Task.WhenAll(tasks);
                }
            }
            catch (HttpRequestException ex)
            {
                await DisplayAlert("Error", "Error fetching results: " + ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An unexpected error occurred: " + ex.Message, "OK");
            }

            return resultTitles;
        }

        private async Task<TorrentDetails?> GetTorrentDetailsAsync(string title, string url)
        {
            var torrentDetails = new TorrentDetails();

            try
            {
                var response = await httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                // Extract details efficiently
                torrentDetails.Title = title;
                torrentDetails.ViewLink = url;
                torrentDetails.DownloadLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '.torrent')]")?.GetAttributeValue("href", string.Empty);
                torrentDetails.MagnetLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]")?.GetAttributeValue("href", string.Empty);
            }
            catch (HttpRequestException ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error fetching torrent details: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
            }

            return torrentDetails;
        }

        private void OnResultSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string selectedItem && resultsDictionary.TryGetValue(selectedItem, out var details))
            {
                selectedTorrentDetails = details;
                DetailsLabel.Text = $"Title: {details.Title}\nView Link: {details.ViewLink}\nDownload Link: {details.DownloadLink}\nMagnet Link: {details.MagnetLink}";
                SaveDetailsButton.IsEnabled = true;
                DownloadButton.IsEnabled = true;
            }
            else
            {
                DetailsLabel.Text = "No details available";
                SaveDetailsButton.IsEnabled = false;
                DownloadButton.IsEnabled = false;
            }
        }

        private void OnSaveDetailsButtonClicked(object sender, EventArgs e)
        {
            if (selectedTorrentDetails != null)
            {
                SaveTorrentDetails(selectedTorrentDetails);
                DisplayAlert("Success", "Torrent details saved.", "OK");
            }
            else
            {
                DisplayAlert("Error", "No torrent details to save.", "OK");
            }
        }

        private void OnStartDownloadButtonClicked(object sender, EventArgs e)
        {
            if (selectedTorrentDetails != null)
            {
                StartDownload(selectedTorrentDetails);
                DisplayAlert("Download Started", "The download has started.", "OK");
            }
            else
            {
                DisplayAlert("Error", "No torrent details available to start the download.", "OK");
            }
        }

        private void SaveTorrentDetails(TorrentDetails details)
        {
            // Implement logic to save torrent details (e.g., to a file or database)
        }

        private async void StartDownload(TorrentDetails details)
        {
            try
            {
                if (!string.IsNullOrEmpty(details.MagnetLink))
                {
                    // Use BitSwarm to handle the torrent download
                    var bitSwarm = new BitSwarm();
                    bitSwarm.Open(details.MagnetLink);
                    bitSwarm.Start();

                    //wait 1000
                    await Task.Delay(5000);
                    DisplayAlert("torrent downloading", bitSwarm.torrent.file.name, "Ok");
                    // Track download progress or any additional logic here
                }
                else
                {
                    DisplayAlert("Error", "No magnet link found for the torrent.", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Download Error", $"An error occurred while starting the download: {ex.Message}", "OK");
            }
        }
    }

    public class TorrentDetails
    {
        public string Title { get; set; } = string.Empty;
        public string ViewLink { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public string MagnetLink { get; set; } = string.Empty;
    }
}
