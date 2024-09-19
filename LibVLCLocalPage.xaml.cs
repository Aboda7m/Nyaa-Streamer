#if WINDOWS
#else
using LibVLCSharp.MAUI;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class LibVLCLocalPage : ContentPage
    {
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private bool _isPageDisappearing = false;

        public LibVLCLocalPage(Uri mediaUri)
        {
            InitializeComponent();
            BindingContext = new LocalViewModel(mediaUri);

            // Add tap gesture recognizer for touch controls
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;

            // Check if the media player is playing and update the button state accordingly
            if (mediaPlayer != null)
            {
                _isPlaying = mediaPlayer.IsPlaying;
                PlayPauseButton.Source = _isPlaying ? "Pause.png" : "Play.png";
            }

            ((LocalViewModel)BindingContext)?.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isPageDisappearing = true;
            DisposeMediaPlayer();
        }

        private void DisposeMediaPlayer()
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null && mediaPlayer.IsPlaying)
            {
                try
                {
                    mediaPlayer.Stop(); // Ensure it stops before disposing
                    mediaPlayer.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing MediaPlayer: {ex.Message}");
                }
            }
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            ((LocalViewModel)BindingContext)?.OnVideoViewInitialized();
        }

        private void OnScreenTapped(object sender, EventArgs e)
        {
            ControlBar.IsVisible = !ControlBar.IsVisible;

            if (ControlBar.IsVisible)
            {
                Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                {
                    ControlBar.IsVisible = false;
                    return false; // Stops the timer
                });
            }
        }

        private void OnPlayPauseButtonClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                if (_isPlaying)
                {
                    mediaPlayer.Pause();
                    PlayPauseButton.Source = "Play.png";
                }
                else
                {
                    mediaPlayer.Play();
                    PlayPauseButton.Source = "Pause.png";
                }

                _isPlaying = !_isPlaying;
            }
        }

        private void OnSeekBackwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                // Prevent time from going below 0
                if (mediaPlayer.Time <= 10000)
                {
                    mediaPlayer.Time = 0; // Set time to 0 if less than 10 seconds
                }
                else
                {
                    mediaPlayer.Time -= 10000; // Seek backward 10 seconds
                }
            }
        }

        private void OnSeekForwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                // Prevent seeking beyond video length
                if (mediaPlayer.Time + 10000 > mediaPlayer.Length)
                {
                    mediaPlayer.Time = mediaPlayer.Length; // Set time to video length
                }
                else
                {
                    mediaPlayer.Time += 10000; // Seek forward 10 seconds
                }
            }
        }

        private void OnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                // Prevent skipping beyond video length
                if (mediaPlayer.Time + 90000 > mediaPlayer.Length)
                {
                    mediaPlayer.Time = mediaPlayer.Length; // Set time to video length
                }
                else
                {
                    mediaPlayer.Time += 90000; // Skip forward 90 seconds
                }
            }
        }

        private void OnUnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                // Prevent time from going below 0
                if (mediaPlayer.Time <= 90000)
                {
                    mediaPlayer.Time = 0; // Set time to 0 if less than 90 seconds
                }
                else
                {
                    mediaPlayer.Time -= 90000; // Unskip back 90 seconds
                }
            }
        }

        private void OnProgressBarDragCompleted(object sender, EventArgs e)
        {
            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null && !_isDragging)
            {
                mediaPlayer.Time = (long)(ProgressBar.Value * mediaPlayer.Length);
            }
        }

        private bool UpdateProgressBar()
        {
            if (_isPageDisappearing) return false; // Stop updating if page is disappearing

            var mediaPlayer = ((LocalViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null && !mediaPlayer.IsPlaying)
            {
                ProgressBar.Value = mediaPlayer.Time / (double)mediaPlayer.Length;
                TimeLabel.Text = $"{mediaPlayer.Time / 60000}:{(mediaPlayer.Time % 60000) / 1000:D2} / " +
                                 $"{mediaPlayer.Length / 60000}:{(mediaPlayer.Length % 60000) / 1000:D2}";
            }

            return true; // Keep the timer running
        }
    }
}
#endif
