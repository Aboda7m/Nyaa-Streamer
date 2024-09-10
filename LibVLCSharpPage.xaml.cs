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
        private bool _isFullscreen = false;

        public LibVLCSharpPage()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

            var panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += OnPanUpdated;
            VideoView.GestureRecognizers.Add(panGestureRecognizer);
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
            if (_isPlaying)
            {
                ((MainViewModel)BindingContext).MediaPlayer.Pause();
                PlayPauseButton.Text = "▶";
            }
            else
            {
                ((MainViewModel)BindingContext).MediaPlayer.Play();
                PlayPauseButton.Text = "⏸";
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

        private void OnFullscreenButtonClicked(object sender, EventArgs e)
        {
            // Empty implementation for fullscreen button
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Handle the panning/dragging events
                    if (e.TotalX > 0 && Math.Abs(e.TotalX) > Math.Abs(e.TotalY))
                    {
                        // Right swipe: Seek forward
                        mediaPlayer.Time += 5000;
                    }
                    else if (e.TotalX < 0 && Math.Abs(e.TotalX) > Math.Abs(e.TotalY))
                    {
                        // Left swipe: Seek backward
                        mediaPlayer.Time -= 5000;
                    }
                    else if (e.TotalY < 0)
                    {
                        // Up swipe: Increase volume or brightness
                        if (e.TotalX < VideoView.Width / 2)
                        {
                            mediaPlayer.Volume += 10; // Adjust this as needed
                        }
                        else
                        {
                            // Implement brightness increase here (if possible)
                        }
                    }
                    else if (e.TotalY > 0)
                    {
                        // Down swipe: Decrease volume or brightness
                        if (e.TotalX < VideoView.Width / 2)
                        {
                            mediaPlayer.Volume -= 10; // Adjust this as needed
                        }
                        else
                        {
                            // Implement brightness decrease here (if possible)
                        }
                    }
                    break;
            }
        }
    }
}
#endif
