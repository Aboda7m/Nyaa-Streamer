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
        private string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"temp\downloads");
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



                //MainThread.BeginInvokeOnMainThread(() => DownloadProgress.IsVisible = true);
                DownloadProgressBar.IsVisible = true;
                DownloadPercentageLabel.IsVisible = true;   

                StartHttpServer(manager);

                // Loop to check download progress
                double progress = 0;
                while (progress < 2)
                {
                    progress = (double)manager.Progress;
                    Debug.WriteLine($"Current download progress: {progress}%");
                    MainThread.BeginInvokeOnMainThread(() => DownloadProgressBar.Progress = progress/100);
                    MainThread.BeginInvokeOnMainThread(() => DownloadPercentageLabel.Text = progress+"%");
                    await Task.Delay(1000); // Wait for 1 second before checking again
                }

                Debug.WriteLine("Minimum 10% downloaded. Starting media player...");
                await Task.Delay(3000);
                await Navigation.PushAsync(new MediaPlayerPage("http://localhost:8888/"));

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
                                await HandleFileStreamingAsync(largestFile, response, request);
                                Debug.WriteLine("File streaming handled.");
                            }
                            else
                            {
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                response.ContentType = "text/plain";
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



        private async Task HandleFileStreamingAsync(ITorrentManagerFile torrentFile, HttpListenerResponse response, HttpListenerRequest request)
        {
            const int maxRetries = 3;
            const int delayBetweenRetries = 1000; // 1 second

            Debug.WriteLine($"Handling file streaming for {torrentFile.FullPath}");

            try
            {
                // Parse the range header if present
                long start = 0;
                long end = torrentFile.Length - 1;
                if (request.Headers["Range"] != null)
                {
                    var rangeHeader = request.Headers["Range"];
                    var range = rangeHeader.Replace("bytes=", "").Split('-');
                    start = long.Parse(range[0]);
                    end = range.Length > 1 && !string.IsNullOrEmpty(range[1]) ? long.Parse(range[1]) : end;
                }

                // Ensure the start and end are within the file length
                start = Math.Max(start, 0);
                end = Math.Min(end, torrentFile.Length - 1);

                // Set response status code and headers
                response.StatusCode = (int)HttpStatusCode.PartialContent;
                response.ContentType = "video/mp4"; // Adjust content type as needed
                response.Headers.Add("Accept-Ranges", "bytes");
                response.Headers.Add("Content-Range", $"bytes {start}-{end}/{torrentFile.Length}");
                response.ContentLength64 = end - start + 1;

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        Debug.WriteLine($"Attempt {attempt + 1} to create stream for {torrentFile.FullPath}");
                        using (var stream = await manager!.StreamProvider.CreateStreamAsync(torrentFile, prebuffer: true))
                        {
                            Debug.WriteLine("Stream created successfully.");
                            // Seek to the start position
                            stream.Seek(start, SeekOrigin.Begin);

                            // Copy the required range of bytes to the response
                            var buffer = new byte[8192];
                            long bytesToRead = end - start + 1;
                            int bytesRead;

                            while (bytesToRead > 0 && (bytesRead = await stream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, bytesToRead))) > 0)
                            {
                                await response.OutputStream.WriteAsync(buffer, 0, bytesRead);
                                bytesToRead -= bytesRead;
                            }

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
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while streaming: {ex.Message}");
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


        private class TorrentDetails
        {
            public string Title { get; set; }
            public string ViewLink { get; set; }
            public string DownloadLink { get; set; }
            public string MagnetLink { get; set; }
        }
    }
}
