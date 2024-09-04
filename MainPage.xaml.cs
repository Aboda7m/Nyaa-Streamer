using HtmlAgilityPack;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Streaming;
using Microsoft.Maui.Controls;
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
        private Dictionary<string, string> resultsDictionary = new Dictionary<string, string>();
        private string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"downloads");
        private ClientEngine engine;
        private TorrentManager? manager;

        public MainPage()
        {
            InitializeComponent();
            Directory.CreateDirectory(downloadDirectory);

            var engineSettings = new EngineSettingsBuilder()
            {
                CacheDirectory = Path.Combine(downloadDirectory, "cache"),
                DiskCacheBytes = 512 * 1024 * 1024 // Increased cache size for better performance
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
                ProceedBtn.IsEnabled = true;
            }
        }

        private async void OnProceedButtonClicked(object sender, EventArgs e)
        {
            var selectedResult = ResultsListView.SelectedItem as string;
            if (selectedResult != null && resultsDictionary.TryGetValue(selectedResult, out var url))
            {
                var torrentDetails = await GetTorrentDetailsAsync(url);
                if (torrentDetails != null && !string.IsNullOrEmpty(torrentDetails.MagnetLink))
                {
                    await StartTorrentDownloadAndStreamAsync(torrentDetails.MagnetLink);
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

        private async Task StartTorrentDownloadAndStreamAsync(string magnetLink)
        {
            try
            {
                Debug.WriteLine($"Parsing magnet link: {magnetLink}");
                var magnet = MagnetLink.Parse(magnetLink);
                Debug.WriteLine($"Magnet link parsed: {magnet}");

                Debug.WriteLine("Adding torrent for streaming...");
                manager = await engine.AddStreamingAsync(magnet, downloadDirectory);
                Debug.WriteLine("Torrent added for streaming.");

                Debug.WriteLine("Starting torrent...");
                await manager.StartAsync();
                Debug.WriteLine("Torrent started.");

                Debug.WriteLine("Waiting for metadata...");
                await manager.WaitForMetadataAsync();
                Debug.WriteLine("Metadata received.");

                // Start the HTTP server immediately
                StartHttpServer();

                // Redirect to the media player page
                await Task.Delay(1000); // Short delay to ensure HTTP server is up
                await Navigation.PushAsync(new MediaPlayerPage("http://localhost:8888/"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void StartHttpServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();

            Debug.WriteLine("HTTP server started at http://localhost:8888/");

            Task.Run(async () =>
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();
                    var request = context.Request;
                    var response = context.Response;

                    Debug.WriteLine($"Received request for {request.Url.AbsolutePath}");

                    if (request.Url.AbsolutePath == "/")
                    {
                        try
                        {
                            await HandleFileStreamingAsync(response, request);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Streaming error: {ex.Message}");
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            response.ContentType = "text/plain";
                            using (var writer = new StreamWriter(response.OutputStream))
                            {
                                await writer.WriteAsync("An error occurred while streaming the file.");
                            }
                        }
                        finally
                        {
                            response.OutputStream.Close();
                        }
                    }
                }
            });
        }

        private async Task HandleFileStreamingAsync(HttpListenerResponse response, HttpListenerRequest request)
        {
            try
            {
                // Default range to stream the entire file
                long start = 0;
                long end = manager.Files.First().Length - 1;

                // Parse the range header if present
                var rangeHeader = request.Headers["Range"];
                if (rangeHeader != null && rangeHeader.StartsWith("bytes="))
                {
                    var rangeParts = rangeHeader.Substring("bytes=".Length).Split('-');
                    if (rangeParts.Length > 0 && !string.IsNullOrEmpty(rangeParts[0]))
                    {
                        start = long.Parse(rangeParts[0]);
                    }
                    if (rangeParts.Length > 1 && !string.IsNullOrEmpty(rangeParts[1]))
                    {
                        end = long.Parse(rangeParts[1]);
                    }
                }

                // Validate and adjust the range to be within the file bounds
                start = Math.Max(start, 0);
                end = Math.Min(end, manager.Files.First().Length - 1);

                // Set the response headers for partial content
                response.StatusCode = (int)HttpStatusCode.PartialContent;
                response.ContentType = "video/mp4";
                response.Headers.Add("Accept-Ranges", "bytes");
                response.Headers.Add("Content-Range", $"bytes {start}-{end}/{manager.Files.First().Length}");
                response.ContentLength64 = end - start + 1;

                // Create a stream for the requested range
                using (var stream = await manager.StreamProvider.CreateStreamAsync(manager.Files.First(), prebuffer: true))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await response.OutputStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File streaming error: {ex.Message}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.ContentType = "text/plain";
                using (var writer = new StreamWriter(response.OutputStream))
                {
                    await writer.WriteAsync("An error occurred while streaming the file.");
                }
            }
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
