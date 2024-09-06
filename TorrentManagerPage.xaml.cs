using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MonoTorrent;
using MonoTorrent.Client;
using HtmlAgilityPack;
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
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using MonoTorrent.Client;
using System.Linq;

namespace Nyaa_Streamer
{
    public partial class TorrentManagerPage : ContentPage
    {
        public ObservableCollection<Torrent> Torrents { get; set; }
        private Dictionary<string, Torrent> torrentsDictionary;

        public TorrentManagerPage(Dictionary<string, Torrent> torrentsDictionary)
        {
            InitializeComponent();
            this.torrentsDictionary = torrentsDictionary;
            Torrents = new ObservableCollection<Torrent>(torrentsDictionary.Values);
            TorrentsListView.ItemsSource = Torrents;
        }

        private void OnTorrentSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Enable the stream button when a torrent is selected
            StreamButton.IsEnabled = e.SelectedItem != null;
        }

        private async void OnStreamButtonClicked(object sender, EventArgs e)
        {
            // Get the selected torrent
            var selectedTorrent = TorrentsListView.SelectedItem as Torrent;
            if (selectedTorrent != null)
            {
                // Extract the magnet link or URL from the torrent
                string magnetLink = selectedTorrent.Url; // Update this if you store magnet links differently

                // Start streaming (this method should be implemented as needed)
                await StartTorrentStreamAsync(magnetLink);
            }
        }

        private async Task StartTorrentStreamAsync(string magnetLink)
        {
            // Implement the streaming logic here
            // For demonstration, the following code is a placeholder
            // Replace with actual streaming logic
            await DisplayAlert("Streaming", $"Starting stream for magnet link: {magnetLink}", "OK");

            // Example logic for starting streaming (update as necessary)
            var streamLink = await StartHttpServer(magnetLink);
            StartRedirectServer(streamLink);

            // Start VLC or other media player
            StartVlcProcess();
        }

        private async Task<string> StartHttpServer(string magnetLink)
        {
            // Example implementation; replace with actual logic
            return $"http://localhost:8888/{magnetLink}";
        }

        private void StartRedirectServer(string targetUrl)
        {
            // Start the redirect server
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
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("InstallDir");
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

    public class Torrent
    {
        public string Title { get; set; }
        public string Url { get; set; } // Assuming this is where the magnet link or URL is stored
    }
}
