using System.IO;
using Microsoft.Maui.Controls;

namespace Nyaa_Streamer
{
    public partial class DownloadedFilesPage : ContentPage
    {
        private string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"downloads");

        public DownloadedFilesPage()
        {
            InitializeComponent();
            LoadDownloadedFiles();
        }

        private void LoadDownloadedFiles()
        {
            if (Directory.Exists(downloadDirectory))
            {
                var files = Directory.GetFiles(downloadDirectory).Select(Path.GetFileName).ToList();
                DownloadedFilesListView.ItemsSource = files;
            }
            else
            {
                DownloadedFilesListView.ItemsSource = new List<string> { "No files downloaded yet." };
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
