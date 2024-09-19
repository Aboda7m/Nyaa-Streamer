using LibVLCSharp.MAUI;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class LibVLCLocalPage : ContentPage
    {
        private bool _isPlaying = false;
        private bool _isDragging = false;

        public LibVLCLocalPage(Uri mediaUri)
        {
            InitializeComponent();
            BindingContext = new LocalViewModel(mediaUri);

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((LocalViewModel)BindingContext)?.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ((LocalViewModel)BindingContext)?.OnDisappearing();
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            ((LocalViewModel)BindingContext)?.OnVideoViewInitialized();
        }

        private bool UpdateProgressBar()
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;

            if (!_isDragging && mediaPlayer != null && mediaPlayer.Media != null && mediaPlayer.Length > 0)
            {
                // Update progress based on the percentage (0-1)
                ProgressBar.Value = (double)mediaPlayer.Time / mediaPlayer.Length;

                // Update the time label
                TimeLabel.Text = $"{TimeSpan.FromMilliseconds(mediaPlayer.Time):mm\\:ss} / {TimeSpan.FromMilliseconds(mediaPlayer.Length):mm\\:ss}";
            }

            return true; // Continue updating
        }
    }
}
