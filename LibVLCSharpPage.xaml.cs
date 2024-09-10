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

        public LibVLCSharpPage()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);
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

        private void OnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time += 90000; // Skip forward 90 seconds
        }

        private void OnUnSkipOpeningClicked(object sender, EventArgs e)
        {
            var mediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
            mediaPlayer.Time -= 90000; // Skip forward 90 seconds
        }
    }
}
#endif
