#if WINDOWS
#else
using LibVLCSharp.Shared;
using LibVLCSharp.MAUI;
using System.Diagnostics;

namespace Nyaa_Streamer
{
    public partial class LibVLCSharpPage : ContentPage
    {
        private bool _isPlaying = false;
        private bool _isDragging = false;

        public LibVLCSharpPage()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

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
            ((MainViewModel)BindingContext).OnDisappearing();
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            ((MainViewModel)BindingContext).OnVideoViewInitialized();
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
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;

            if (_isPlaying)
            {
                mediaPlayer?.Pause();
                //PlayPauseButton.Text = "▶";
                PlayPauseButton.Source = "Play.png";
            }
            else
            {
                mediaPlayer?.Play();
                PlayPauseButton.Source = "Pause.png";
            }

            _isPlaying = !_isPlaying;
        }

        private void OnSeekBackwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time -= 10000; // Seek backward 10 seconds
        }

        private void OnSeekForwardClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time += 10000; // Seek forward 10 seconds
        }

        private void OnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time += 90000; // Skip forward 90 seconds
        }

        private void OnUnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time -= 90000; // Skip backward 90 seconds
        }

        private void OnProgressBarDragStarted(object sender, EventArgs e)
        {
            _isDragging = true; // Pause progress updates during drag
        }

        private void OnProgressBarDragCompleted(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;

            if (mediaPlayer?.Media != null)
            {
                long newTime = (long)(ProgressBar.Value * mediaPlayer.Length);
                mediaPlayer.Time = newTime;
            }

            _isDragging = false; // Resume progress updates after drag
        }

        private bool UpdateProgressBar()
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;

            if (!_isDragging && mediaPlayer?.Media != null)
            {
                ProgressBar.Maximum = 1;
                double currentValue = (double)mediaPlayer.Time / mediaPlayer.Length;
                if (ProgressBar.Value != currentValue)
                {
                    ProgressBar.Value = currentValue;
                }
            }

            // Return true to ensure the timer continues updating the progress bar for testing
            return true;
        }

    }
}
#endif
