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
using System.ComponentModel;

namespace Nyaa_Streamer
{
    public partial class TorrentManagerPage : ContentPage
    {
        public ObservableCollection<TorrentFile> TorrentFiles { get; set; }
        private List<TorrentManager> managers;
        private HttpListener redirectListener;
        private string currentStreamUrl;
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
                        TorrentFiles.Add(new TorrentFile
                        {
                            FileName = file.Path,
                            Size = file.Length,
                            File = file,
                            SizeString = FormatBytes(file.Length),
                            BytesDownloaded = file.BytesDownloaded(),
                            DownloadedString = FormatBytes(file.BytesDownloaded())
                        });
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

        // Other methods remain unchanged...
    }

    public class TorrentFile : INotifyPropertyChanged
    {
        private long bytesDownloaded;

        public string FileName { get; set; }
        public long Size { get; set; }
        public string SizeString { get; set; }

        public long BytesDownloaded
        {
            get => bytesDownloaded;
            set
            {
                bytesDownloaded = value;
                OnPropertyChanged(nameof(BytesDownloaded));
                OnPropertyChanged(nameof(DownloadedString)); // Update string when bytes change
            }
        }

        public string DownloadedString => FormatBytes(BytesDownloaded);
        public MonoTorrent.ITorrentManagerFile File { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
