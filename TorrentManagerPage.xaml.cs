using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MonoTorrent;
using MonoTorrent.Client;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Nyaa_Streamer
{
    public partial class TorrentManagerPage : ContentPage
    {
        public ObservableCollection<TorrentFile> TorrentFiles { get; set; }
        private List<TorrentManager> managers; // List to hold multiple TorrentManager instances
        private HttpListener redirectListener; // HTTP redirect listener
        private string currentStreamUrl; // URL of the currently streaming file
        private bool isUpdatingProgress = false;
        private TorrentFile previousFile;

        public TorrentManagerPage(List<TorrentManager> managers)
        {
            InitializeComponent();
            this.managers = managers;

            TorrentFiles = new ObservableCollection<TorrentFile>();
            LoadTorrentFiles();
            TorrentFilesListView.ItemsSource = TorrentFiles;
        }

        private async void LoadTorrentFiles()
        {
            TorrentFiles.Clear();
            Debug.WriteLine("TorrentFiles.Clear();");

            foreach (var manager in managers)
            {
                try
                {
                    await manager.WaitForMetadataAsync();

                    foreach (var file in manager.Files)
                    {
                        Debug.WriteLine("FileName: " + file.Path);
                        TorrentFiles.Add(new TorrentFile
                        {
                            FileName = file.Path,
                            Size = file.Length,
                            File = file,
                            SizeString = FormatBytes(file.Length),
                            BytesDownloaded= file.BytesDownloaded(),
                            DownloadedString = FormatBytes(file.BytesDownloaded())

                        });
                        Debug.WriteLine("TorrentFiles.Add(new TorrentFile: " + file.Length);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load files from manager: {ex.Message}");
                }
            }
        }

        private void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
        {
            StreamButton.IsEnabled = e.SelectedItem != null;

            if (e.SelectedItem != null)
            {
                // Get the selected file
                var selectedFile = e.SelectedItem as TorrentFile;

                // Reset progress for the previous file
                if (previousFile != null)
                {
                    previousFile.BytesDownloaded = 0;
                    previousFile.DownloadedString = FormatBytes(0);
                }

                // Update the new selected file
                previousFile = selectedFile;

                // Reset the progress bar
                DownloadProgressBar.Progress = 0;
                DownloadProgressBar.IsVisible = true;
                ProgressContainer.IsVisible = true;

                // Update the selected file's downloaded information
                selectedFile.BytesDownloaded = selectedFile.File.BytesDownloaded();
                selectedFile.DownloadedString = FormatBytes(selectedFile.BytesDownloaded);

                // Start updating the progress bar
                StartProgressUpdate(selectedFile);
            }
            else
            {
                DownloadProgressBar.IsVisible = false;
                ProgressContainer.IsVisible = false;
            }
        }

        private async void StartProgressUpdate(TorrentFile selectedFile)
        {
            if (isUpdatingProgress)
                return;

            isUpdatingProgress = true;

            while (isUpdatingProgress)
            {
                var manager = managers.FirstOrDefault(m => m.Files.Contains(selectedFile.File));
                if (manager != null)
                {
                    // Update the file's downloaded bytes
                    selectedFile.BytesDownloaded = selectedFile.File.BytesDownloaded();
                    selectedFile.DownloadedString = FormatBytes(selectedFile.BytesDownloaded);

                    // Calculate the progress as a percentage
                    double progress = (double)selectedFile.BytesDownloaded / selectedFile.Size;

                    // Update the progress bar and text
                    DownloadProgressBar.Progress = progress;
                    DownloadProgressText.Text = $"{selectedFile.DownloadedString} / {selectedFile.SizeString} downloaded";

                    Debug.WriteLine($"Progress: {progress:P}");

                    // Stop updating if the file is fully downloaded
                    if (selectedFile.BytesDownloaded >= selectedFile.Size)
                    {
                        isUpdatingProgress = false;
                        break;
                    }
                }

                await Task.Delay(1000);
            }
        }


        private async void OnStreamButtonClicked(object sender, EventArgs e)
        {
            var selectedFile = TorrentFilesListView.SelectedItem as TorrentFile;
            if (selectedFile != null)
            {
                var manager = managers.FirstOrDefault(m => m.Files.Contains(selectedFile.File));
                if (manager != null)
                {
                    // Dispose of old stream if exists
                    DisposeOldStream();

                    // Start streaming
                    await StartTorrentStreamAsync(manager, selectedFile.File);
                }
            }
        }

        private async Task StartTorrentStreamAsync(TorrentManager manager, ITorrentManagerFile selectedFile)
        {
            await DisplayAlert("Streaming", $"Starting stream for file: {selectedFile.Path}", "OK");

            var httpStream = await StartHttpServer(manager, selectedFile);
            var streamLink = httpStream.FullUri;
            if (streamLink != null)
            {
                currentStreamUrl = streamLink;
                StartRedirectServer(streamLink);
#if WINDOWS
                StartVlcProcess();
#else
                await Navigation.PushAsync(new LibVLCSharpPage(httpStream));
                //httpStream.Dispose();
#endif
            }
        }

        private async Task<MonoTorrent.Streaming.IHttpStream> StartHttpServer(TorrentManager manager, ITorrentManagerFile selectedFile)
        {
            try
            {
                var httpStream = await manager.StreamProvider.CreateHttpStreamAsync(selectedFile, prebuffer: true);
                Debug.WriteLine("Streaming media from: " + httpStream.FullUri);
                return httpStream;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAILED TO CREATE HTTP STREAM: {ex.Message}");
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                return null;
            }
        }

        private void StartRedirectServer(string targetUrl)
        {
            if (redirectListener != null)
            {
                redirectListener.Stop();
                redirectListener = null;
            }

            redirectListener = new HttpListener();
            redirectListener.Prefixes.Add("http://localhost:8888/");
            redirectListener.Start();
            Debug.WriteLine("Redirect server started at http://localhost:8888/");

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var context = redirectListener.GetContext();
                        var response = context.Response;
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

        private void DisposeOldStream()
        {
            if (redirectListener != null)
            {
                redirectListener.Stop();
                redirectListener = null;
                Debug.WriteLine("Old stream disposed.");
            }
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

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }
    }

    public class TorrentFile
    {
        public string FileName { get; set; }
        public long Size { get; set; }
        public string SizeString { get; set; }
        public long BytesDownloaded { get; set; } // Track the downloaded bytes
        public string DownloadedString { get; set; }
        // => $"{FormatBytes(BytesDownloaded)} / {SizeString} downloaded"; // Format download progress
        public MonoTorrent.ITorrentManagerFile File { get; set; }
    }
}
