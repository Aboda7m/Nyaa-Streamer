#if WINDOWS
#else
using LibVLCSharp.Shared;
using LibVLCSharp.MAUI;
using System.Diagnostics;
using MonoTorrent.Streaming;


namespace Nyaa_Streamer
{
    public partial class LibVLCSharpPage : ContentPage
    {
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private bool _isPageDisappearing = false;
        private IHttpStream _httpStream;

        public LibVLCSharpPage(IHttpStream httpStream)
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);
            _httpStream = httpStream;

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((MainViewModel)BindingContext).OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isPageDisappearing = true;
            DisposeMediaPlayer();
            _httpStream.Dispose();

        }

        private void DisposeMediaPlayer()
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                try
                {
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
            ((MainViewModel)BindingContext)?.OnVideoViewInitialized();
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
            try
            {
                var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;

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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPlayPauseButtonClicked: {ex.Message}");
            }
        }

        private void OnSeekBackwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                mediaPlayer.Time -= 10000; // Seek backward 10 seconds
            }
        }

        private void OnSeekForwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                mediaPlayer.Time += 10000; // Seek forward 10 seconds
            }
        }

        private void OnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                mediaPlayer.Time += 90000; // Skip forward 90 seconds
            }
        }

        private void OnUnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer != null)
            {
                mediaPlayer.Time -= 90000; // Skip backward 90 seconds
            }
        }

        private void OnProgressBarDragStarted(object sender, EventArgs e)
        {
            _isDragging = true; // Pause progress updates during drag
        }

        private void OnProgressBarDragCompleted(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;

            if (mediaPlayer?.Media != null)
            {
                long newTime = (long)(ProgressBar.Value * mediaPlayer.Length);
                mediaPlayer.Time = newTime;
            }

            _isDragging = false; // Resume progress updates after drag
        }

        private bool UpdateProgressBar()
        {
            if (_isPageDisappearing)
                return false; // Stop updating if page is disappearing

            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;

            if (!_isDragging && mediaPlayer != null && mediaPlayer.Media != null && mediaPlayer.Length > 0)
            {
                // Update progress based on the percentage (0-1)
                ProgressBar.Value = (double)mediaPlayer.Time / mediaPlayer.Length;

                // Update the time label
                TimeLabel.Text = $"{TimeSpan.FromMilliseconds(mediaPlayer.Time):mm\\:ss} / {TimeSpan.FromMilliseconds(mediaPlayer.Length):mm\\:ss}";
            }

            return true; // Continue updating
        }

        private string UpdateTimeBar()
        {
            var mediaPlayer = ((MainViewModel)BindingContext)?.MediaPlayer;
            if (mediaPlayer == null)
                return "";

            int currentTime = (int)mediaPlayer.Time;
            int maxTime = (int)mediaPlayer.Length;

            string FormatTime(int timeInMilliseconds)
            {
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeInMilliseconds);
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }

            string currentFormattedTime = FormatTime(currentTime);
            string maxFormattedTime = FormatTime(maxTime);

            return $"{currentFormattedTime} / {maxFormattedTime}";
        }
    }
}
#endif
