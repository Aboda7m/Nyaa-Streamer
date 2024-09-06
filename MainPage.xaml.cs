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
using Microsoft.Win32;


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
                DiskCacheBytes = 512 * 1024 * 1024 ,// Increased cache size for better performance
                HttpStreamingPrefix = "http://localhost:8889/"
            }.ToSettings();

            engine = new ClientEngine(engineSettings);
            Debug.WriteLine("ClientEngine initialized.");

            //StartVlcProcess(); // Call the method to start VLC
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
                var streamlink = await StartHttpServer(manager);
                Task.Run(() => StartRedirectServer(streamlink));
                // Redirect to the media player page
                await Task.Delay(1000); // Short delay to ensure HTTP server is up
                                        //await Navigation.PushAsync(new MediaPlayerPage("http://localhost:8888/"));
                                        //await Navigation.PushAsync(new webViewPage("http://localhost:8888/"));
                                        //StartVlcProcess(); // Call the method to start VLC

                #if WINDOWS
                StartVlcProcess(); // Call the method to start VLC
                #else
                Debug.WriteLine("not windows");
                #endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<string> StartHttpServer(TorrentManager manager)
        {
            try
            {
                // Create the HTTP stream
                var httpStream = await manager.StreamProvider.CreateHttpStreamAsync(manager.Files.First(), prebuffer: true);

                // Log the URI to debug
                Debug.WriteLine("Streaming media from: " + httpStream.FullUri);

                // Wait for the HTTP stream to be ready
                //await Task.Delay(10000); // Short delay to ensure HTTP server is up

                // Redirect to the media player page
                // This will need to be your actual implementation, e.g., setting the source of a WebView
                //await Navigation.PushAsync(new webViewPage(httpStream.FullUri));
                //await Navigation.PushAsync(new MediaPlayerPage(httpStream.FullUri));
                //await Navigation.PushAsync(new webViewPage(httpStream.FullUri));

                return httpStream.FullUri;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAILED TO CREATE HTTP STREAM: {ex.Message}");
                Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                return null;
            }
        }

        private void StartRedirectServer(string targetUrl)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            Debug.WriteLine("Redirect server started at http://localhost:8888/");

            while (true)
            {
                try
                {
                    var context = listener.GetContext();
                    var response = context.Response;

                    // Redirect to the target URL
                    response.StatusCode = (int)HttpStatusCode.Redirect;
                    response.RedirectLocation = targetUrl;
                    response.Close();
                }
                catch (HttpListenerException ex)
                {
                    Debug.WriteLine($"HTTP Listener Exception: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"General Exception: {ex.Message}");
                }
            }
        }


        private string GetVlcPathFromRegistry()
        {
            try
            {
                // Open the registry key where VLC is installed
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC"))
                {
                    if (key != null)
                    {
                        // Get the installation path
                        object value = key.GetValue("InstallDir");
                        if (value != null)
                        {
                            return System.IO.Path.Combine(value.ToString(), "vlc.exe");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading registry: {ex.Message}");
            }

            return null;
        }
        private void StartVlcProcess()
        {
            try
            {
                // Lookup VLC path in registry
                string vlcPath = GetVlcPathFromRegistry();

                if (!string.IsNullOrEmpty(vlcPath))
                {
                    // Create a new process
                    Process process = new Process();
                    process.StartInfo.FileName = vlcPath;
                    process.StartInfo.Arguments = "http://localhost:8888/";  // Add any command-line arguments if needed
                    process.StartInfo.UseShellExecute = true; // Use shell execute to start the process

                    // Start the process
                    process.Start();
                }
                else
                {
                    Debug.WriteLine("VLC not found in the registry.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if VLC cannot be started
                Debug.WriteLine($"Error starting VLC: {ex.Message}");
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
