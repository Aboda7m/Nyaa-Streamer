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
        private string downloadDirectory = @"F:\Dev\temp\downloads";
        private ClientEngine engine;
        private TorrentManager? manager;

        public MainPage()
        {
            InitializeComponent();
            Directory.CreateDirectory(downloadDirectory);

            var engineSettings = new EngineSettingsBuilder()
            {
                CacheDirectory = Path.Combine(downloadDirectory, "cache"),
                DiskCacheBytes = 256 * 1024 * 1024
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

                    // Extract torrent details
                    torrentDetails.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim();
                    torrentDetails.ViewLink = url;
                    torrentDetails.DownloadLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '.torrent')]")?.GetAttributeValue("href", string.Empty);
                    torrentDetails.MagnetLink = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, 'magnet:')]")?.GetAttributeValue("href", string.Empty);
                    if (torrentDetails.MagnetLink != null)
                    {
                        // Replace &amp; with & to ensure proper format
                        torrentDetails.MagnetLink = torrentDetails.MagnetLink.Replace("&amp;", "&");
                    }
                    //torrentDetails.MagnetLink = torrentDetails.MagnetLink;

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

        private async Task StartTorrentDownloadAndStreamAsync(string magnetLink)
        {
            try
            {
                Debug.WriteLine($"Parsing magnet link: {magnetLink}");
                var magnet = MagnetLink.Parse(magnetLink);
                //var magnet = MagnetLink.Parse("magnetLink");
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

                

                StartHttpServer(manager);

                // DisplayAlert("Streaming", "Streaming started. You can now open the stream in VLC.", "OK");
                
                mediaElement.Source = new Uri("http://localhost:8888/");
                mediaElement.Play();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private void StartHttpServer(TorrentManager manager)
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
                            Debug.WriteLine("Waiting for metadata...");
                            await manager.WaitForMetadataAsync();
                            Debug.WriteLine("Metadata received.");

                            var largestFile = manager.Files.OrderByDescending(f => f.Length).FirstOrDefault();
                            Debug.WriteLine(largestFile != null ? $"Largest file selected: {largestFile.FullPath}" : "No files found in torrent.");

                            if (largestFile != null)
                            {
                                Debug.WriteLine("Handling file streaming...");
                                await HandleFileStreamingAsync(largestFile, response);
                                Debug.WriteLine("File streaming handled.");
                            }
                            else
                            {
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                using (var writer = new StreamWriter(response.OutputStream))
                                {
                                    await writer.WriteAsync("No files found in the torrent.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"An error occurred while streaming: {ex.Message}");
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
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

        private async Task HandleFileStreamingAsync(ITorrentManagerFile torrentFile, HttpListenerResponse response)
        {
            const int maxRetries = 3;
            const int delayBetweenRetries = 1000; // 1 second

            Debug.WriteLine($"Handling file streaming for {torrentFile.FullPath}");

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    Debug.WriteLine($"Attempt {attempt + 1} to create stream for {torrentFile.FullPath}");
                    using (var stream = await manager!.StreamProvider.CreateStreamAsync(torrentFile, prebuffer: true))
                    {
                        Debug.WriteLine("Stream created successfully.");
                        response.ContentType = "video/mp4"; // Adjust content type as needed
                        await stream.CopyToAsync(response.OutputStream);
                        Debug.WriteLine("File streamed successfully.");
                    }
                    return; // Successfully completed, exit the method
                }
                catch (IOException ioEx)
                {
                    Debug.WriteLine($"File I/O error (attempt {attempt + 1}): {ioEx.Message}");

                    if (attempt == maxRetries - 1)
                    {
                        Debug.WriteLine($"Max retries reached for {torrentFile.FullPath}. Throwing exception.");
                        throw; // Re-throw the exception if it's the last attempt
                    }

                    // Wait before retrying
                    await Task.Delay(delayBetweenRetries);
                }
            }
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
