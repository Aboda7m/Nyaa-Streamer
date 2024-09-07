using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MonoTorrent;
using MonoTorrent.Client;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;

namespace Nyaa_Streamer
{
    public partial class TorrentManagerPage : ContentPage
    {
        public ObservableCollection<TorrentFile> TorrentFiles { get; set; }
        private TorrentManager manager;

        public TorrentManagerPage(TorrentManager manager, ObservableCollection<TorrentFile> TorrentFiles2)
        {
            InitializeComponent();
            this.manager = manager;

            // Initialize the ObservableCollection for torrent files
            TorrentFiles = TorrentFiles2;

            // Load torrent files
            LoadTorrentFiles();
            TorrentFilesListView.ItemsSource = TorrentFiles;
        }

        private async void LoadTorrentFiles()
        {
            // Ensure the manager is started and metadata is available
            //await manager.WaitForMetadataAsync();

            // Clear existing items
            //TorrentFiles.Clear();
            Debug.WriteLine("      TorrentFiles.Clear();: ");

            // Populate the list of files
            foreach (var file in manager.Files)
            {
                Debug.WriteLine("FileName: " + file.Path);
                TorrentFiles.Add(new TorrentFile
                {
                    FileName = file.Path,
                    Size = file.Length,
                    File = file
                });
                Debug.WriteLine("TorrentFiles.Add(new TorrentFile: " + file.Length);
            }
        }

        private void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Enable the stream button when a file is selected
            StreamButton.IsEnabled = e.SelectedItem != null;
        }

        private async void OnStreamButtonClicked(object sender, EventArgs e)
        {
            // Get the selected torrent file
            var selectedFile = TorrentFilesListView.SelectedItem as TorrentFile;
            if (selectedFile != null)
            {
                // Start streaming
                await StartTorrentStreamAsync(selectedFile.File);
            }
        }


        private async Task StartTorrentStreamAsync(ITorrentManagerFile selectedFile)
        {
            // Implement the streaming logic here
            // For demonstration, the following code is a placeholder
            await DisplayAlert("Streaming", $"Starting stream for file: {selectedFile.Path}", "OK");

            // Example logic for starting streaming (update as necessary)
            var streamLink = await StartHttpServer(selectedFile);
            StartRedirectServer(streamLink);

            // Start VLC or other media player
#if WINDOWS
            StartVlcProcess();
#else
            await Navigation.PushAsync(new LibVLCSharpPage());
#endif
        }


        private async Task<string> StartHttpServer(ITorrentManagerFile selectedFile)
        {
            try
            {
                // Create the HTTP stream
                var httpStream = await manager.StreamProvider.CreateHttpStreamAsync(selectedFile, prebuffer: true);

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
            // Start the redirect server
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            Debug.WriteLine("Redirect server started at http://localhost:8888/");

            Task.Run(() =>
            {
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
            });
        }

        private string GetVlcPathFromRegistry()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("InstallDir");
                        if (value != null)
                        {
                            return Path.Combine(value.ToString(), "vlc.exe");
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
                var vlcPath = GetVlcPathFromRegistry();

                if (!string.IsNullOrEmpty(vlcPath))
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = vlcPath,
                            Arguments = "http://localhost:8888/",
                            UseShellExecute = true
                        }
                    };
                    process.Start();
                }
                else
                {
                    Debug.WriteLine("VLC not found in the registry.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting VLC: {ex.Message}");
            }
        }
    }

    public class TorrentFile
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public MonoTorrent.ITorrentManagerFile File { get; set; } // Hold the actual MonoTorrent.TorrentFile object
    }

}
