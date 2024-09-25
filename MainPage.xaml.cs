using HtmlAgilityPack;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Streaming;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel; // Add this using statement
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class MainPage : ContentPage
    {
        private const string NyaaBaseUrl = "https://nyaa.si/?f=0&c=0_0&q={0}&s=seeders&o=desc";
        private ObservableCollection<string> resultsList = new ObservableCollection<string>(); // Change to ObservableCollection
        private Dictionary<string, string> resultsDictionary = new Dictionary<string, string>();
        private string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"downloads");
        private ClientEngine engine;
        private List<TorrentManager> torrentManagers = new List<TorrentManager>(); // List to store all torrent managers

        public MainPage()
        {
            InitializeComponent();
            Directory.CreateDirectory(downloadDirectory);

            // Populate ListView with the ObservableCollection
            ResultsListView.ItemsSource = resultsList;

            var engineSettings = new EngineSettingsBuilder()
            {
                CacheDirectory = Path.Combine(downloadDirectory, "cache"),
                DiskCacheBytes = 512 * 1024 * 1024, // Increased cache size for better performance
                HttpStreamingPrefix = "http://localhost:8889/"
            }.ToSettings();

            engine = new ClientEngine(engineSettings);
            Debug.WriteLine("ClientEngine initialized.");
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            string animeName = AnimeNameEntry.Text;

            if (!string.IsNullOrEmpty(animeName))
            {
                var results = await SearchNyaaAsync(animeName);
                // Update UI on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    resultsList.Clear(); // Clear previous results
                    foreach (var title in results.Keys)
                    {
                        resultsList.Add(title); // Add new results
                    }
                });
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

                    var titleNodes = htmlDoc.DocumentNode.SelectNodes("//a[not(contains(@class, 'comments')) and contains(@href, '/view/')]");

                    if (titleNodes != null)
                    {
                        foreach (var node in titleNodes.Take(10)) // Limit to 10 results
                        {
                            var title = node.GetAttributeValue("title", string.Empty);
                            var href = node.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(href))
                            {
                                var fullUrl = "https://nyaa.si" + href;
                                resultTitles[title] = fullUrl;
                            }
                        }
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
            }

            return resultTitles;
        }
        
        private void OnResultSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                SaveBtn.IsEnabled = true;
            }
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            var selectedResult = ResultsListView.SelectedItem as string;
            if (selectedResult != null && resultsDictionary.TryGetValue(selectedResult, out var url))
            {
                var torrentDetails = await GetTorrentDetailsAsync(url);
                if (torrentDetails != null && !string.IsNullOrEmpty(torrentDetails.MagnetLink))
                {
                    await StartTorrentDownloadAsync(torrentDetails.MagnetLink);
                    ManagerBtn.IsEnabled = true;
                }
                else
                {
                    await DisplayAlert("Error", "No magnet link found for the selected torrent.", "OK");
                }
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

                    torrentDetails.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim();
                    torrentDetails.ViewLink = url;
                    torrentDetails.DownloadLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '.torrent')]")?.GetAttributeValue("href", string.Empty);
                    torrentDetails.MagnetLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]")?.GetAttributeValue("href", string.Empty);
                    if (torrentDetails.MagnetLink != null)
                    {
                        torrentDetails.MagnetLink = torrentDetails.MagnetLink.Replace("&amp;", "&");
                    }
                }
                catch (HttpRequestException)
                {
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            return torrentDetails;
        }

        private async Task StartTorrentDownloadAsync(string magnetLink)
        {
            try
            {
                Debug.WriteLine($"Parsing magnet link: {magnetLink}");
                var magnet = MagnetLink.Parse(magnetLink);
                Debug.WriteLine($"Magnet link parsed: {magnet}");

                Debug.WriteLine("Adding torrent for streaming...");
                var newManager = await engine.AddStreamingAsync(magnet, downloadDirectory);
                torrentManagers.Add(newManager); // Add the new manager to the list
                Debug.WriteLine("Torrent added for streaming.");

                Debug.WriteLine("Starting torrent...");
                await newManager.StartAsync();
                Debug.WriteLine("Torrent started.");

                Debug.WriteLine("Waiting for metadata...");
                await newManager.WaitForMetadataAsync();
                Debug.WriteLine("Metadata received.");
                await DisplayAlert("Success", "Torrent metadata has been successfully loaded.", "OK");

            }
            catch (Exception ex) when (ex.Message.Contains("A manager for this torrent has already been registered"))
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", "already registered", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void ShowManager(object sender, EventArgs e)
        {
            foreach (var manager in torrentManagers) // Iterate through all managers
            {
                foreach (var file in manager.Files)
                {
                    Debug.WriteLine("ShowManager FileName: " + file.Path);
                    Debug.WriteLine("ShowManager .Add(new TorrentFile: " + file.Length);
                }
            }

            // Pass the list to TorrentManagerPage
            await Navigation.PushAsync(new TorrentManagerPage(torrentManagers));
        }

        private async void ClearCacheAndExit()
        {
            try
            {
                if (Directory.Exists(downloadDirectory))
                {
                    // Delete the downloads folder and its contents
                    Directory.Delete(downloadDirectory, true);
                    Debug.WriteLine("Downloads folder deleted.");
                    await DisplayAlert("Done", "Downloads folder deleted, the app will close now after confirmation.", "OK");
                }

                // Close the app (works for desktop applications)
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to clear cache and exit: {ex.Message}");
            }
        }

        // Example button for debugging
        public void OnDebugClearCacheClicked()
        {
            ClearCacheAndExit();
        }

        // Existing code...

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MenuPopUpPage(this));
        }

        public async Task NavigateToDownloadedFilesPage()
        {
            var downloadedFilesPage = new DownloadedFilesPage();
            await Navigation.PushAsync(downloadedFilesPage);
        }

    }



    public class TorrentDetails
    {
        public string? Title { get; set; }
        public string? ViewLink { get; set; }
        public string? DownloadLink { get; set; }
        public string? MagnetLink { get; set; }
    }
}
