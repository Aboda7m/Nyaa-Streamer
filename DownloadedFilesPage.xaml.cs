using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.IO;

namespace Nyaa_Streamer
{
    public partial class DownloadedFilesPage : ContentPage
    {
        private string selectedFilePath;

        public DownloadedFilesPage()
        {
            InitializeComponent();
            LoadDownloadedFiles();
        }

        private void LoadDownloadedFiles()
        {
            // Assuming your download directory is the same as in MainPage
            var downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");
            var files = Directory.GetFiles(downloadDirectory);
            FilesListView.ItemsSource = files; // Bind to the ListView
        }

        private void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                selectedFilePath = e.SelectedItem.ToString(); // Store the selected file path
                // Optionally handle UI updates for selection
            }
        }

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                await DisplayAlert("Selected File", selectedFilePath, "OK");
            }
            else
            {
                await DisplayAlert("Error", "No file selected.", "OK");
            }
        }

        private async void OnPlayButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {

#if WINDOWS
#else
                // Navigate to the local video page and play the selected file
                var mediaUri = new Uri(selectedFilePath);
                await Navigation.PushAsync(new LibVLCSharpPage(mediaUri));
#endif
            }
            else
            {
                await DisplayAlert("Error", "No file selected to play.", "OK");
            }
        }
    }
}
