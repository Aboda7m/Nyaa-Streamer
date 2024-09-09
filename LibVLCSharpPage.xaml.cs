#if WINDOWS
#else
using LibVLCSharp.Shared;
using LibVLCSharp.MAUI;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Nyaa_Streamer
{
    public partial class LibVLCSharpPage : ContentPage
    {
        private bool _isPlaying = true; // Assume video starts playing
        private const int AnimationDuration = 500; // Duration for fade-in/out animation
        private const int ControlBarTimeout = 3000; // Time in milliseconds to hide the control bar

        public LibVLCSharpPage()
        {
            InitializeComponent();
            Debug.WriteLine("public LibVLCSharpPage()\r\n        {\r\n            InitializeComponent();");
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

        private void OnPlayPauseClicked(object sender, EventArgs e)
        {
            _isPlaying = !_isPlaying;

            if (_isPlaying)
            {
                VideoView.MediaPlayer.Play();
                ((Button)sender).Text = "Pause";
            }
            else
            {
                VideoView.MediaPlayer.Pause();
                ((Button)sender).Text = "Play";
            }

            // Hide the control bar after a delay
            HideControlBarWithDelay();
        }

        private async void OnScreenTapped(object sender, EventArgs e)
        {
            // Show the control bar
            await ControlBar.FadeTo(1, AnimationDuration);

            // Hide the control bar after a delay
            HideControlBarWithDelay();
        }

        private async void HideControlBarWithDelay()
        {
            // Wait for the specified timeout
            await Task.Delay(ControlBarTimeout);

            // Fade out the control bar
            await ControlBar.FadeTo(0, AnimationDuration);
        }
    }
}
#endif
