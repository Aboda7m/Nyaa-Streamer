using HtmlAgilityPack;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nyaa_Streamer
{
    public partial class MainPage : ContentPage
    {
        private const string NyaaBaseUrl = "https://nyaa.si/?f=0&c=0_0&q={0}&s=seeders&o=desc";
        private Dictionary<string, TorrentDetails> resultsDictionary = new Dictionary<string, TorrentDetails>();
        private TorrentDetails? selectedTorrentDetails;

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
            else
            {
                await DisplayAlert("Input Error", "Please enter an anime name.", "OK");
            }
        }

        private async Task<Dictionary<string, TorrentDetails>> SearchNyaaAsync(string animeName)
        {
            var resultTitles = new Dictionary<string, TorrentDetails>();
            string url = string.Format(NyaaBaseUrl, Uri.EscapeDataString(animeName));

            System.Diagnostics.Debug.WriteLine($"Searching URL: {url}"); // Debug log for URL

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    System.Diagnostics.Debug.WriteLine($"Response Length: {response.Length}"); // Debug log for response length

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    var titleNodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, '/view/') and not(contains(@class, 'comments'))]");
                    if (titleNodes != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found {titleNodes.Count} title nodes"); // Debug log for number of nodes

                        var tasks = new List<Task>();

                        foreach (var node in titleNodes)
                        {
                            if (resultTitles.Count >= 10)
                                break;

                            var title = node.GetAttributeValue("title", string.Empty);
                            var href = node.GetAttributeValue("href", string.Empty);

                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                            {
                                var fullUrl = "https://nyaa.si" + href; // Construct the full URL
                                System.Diagnostics.Debug.WriteLine($"Full URL: {fullUrl}"); // Debug log for full URL

                                // Fetch torrent details asynchronously
                                var task = Task.Run(async () =>
                                {
                                    var details = await GetTorrentDetailsAsync(title, fullUrl);
                                    if (details != null)
                                    {
                                        lock (resultTitles)
                                        {
                                            resultTitles[title] = details;
                                        }
                                    }
                                });
                                tasks.Add(task);
                            }
                        }

                        // Wait for all tasks to complete
                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No title nodes found"); // Debug log if no nodes found
                    }
                }
                catch (HttpRequestException ex)
                {
                    await DisplayAlert("Error", "Error fetching results: " + ex.Message, "OK");
                    System.Diagnostics.Debug.WriteLine($"HttpRequestException: {ex.Message}"); // Debug log for HTTP request exceptions
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "An unexpected error occurred: " + ex.Message, "OK");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}"); // Debug log for general exceptions
                }
            }

            return resultTitles;
        }

        private async Task<TorrentDetails?> GetTorrentDetailsAsync(string title, string url)
        {
            var torrentDetails = new TorrentDetails();

            System.Diagnostics.Debug.WriteLine($"Fetching torrent details for URL: {url}"); // Debug log for torrent details URL

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    System.Diagnostics.Debug.WriteLine($"Response Length: {response.Length}"); // Debug log for response length

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // Extract details efficiently
                    torrentDetails.Title = title ?? "No Title";
                    torrentDetails.ViewLink = url;
                    torrentDetails.DownloadLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '.torrent')]")?.GetAttributeValue("href", string.Empty);
                    torrentDetails.MagnetLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]")?.GetAttributeValue("href", string.Empty);

                    System.Diagnostics.Debug.WriteLine($"Title: {torrentDetails.Title}"); // Debug log for title
                    System.Diagnostics.Debug.WriteLine($"Download Link: {torrentDetails.DownloadLink}"); // Debug log for download link
                    System.Diagnostics.Debug.WriteLine($"Magnet Link: {torrentDetails.MagnetLink}"); // Debug log for magnet link
                }
                catch (HttpRequestException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"HttpRequestException: {ex.Message}"); // Debug log for HTTP request exceptions
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}"); // Debug log for general exceptions
                    return null;
                }
            }

            return torrentDetails;
        }

        private void OnResultSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string selectedItem)
            {
                if (resultsDictionary.TryGetValue(selectedItem, out var details))
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

        private void StartDownload(TorrentDetails details)
        {
            // Implement logic to start the download
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
