#if WINDOWS
// Windows-specific code can be added here
#else
using LibVLCSharp.Shared;
using LibVLCSharp.MAUI;
using System.Diagnostics;
using MonoTorrent.Streaming;
using System.ComponentModel;
using LibVLCSharp;
using Microsoft.Maui.Controls;
using Android.Media;

namespace Nyaa_Streamer
{
    public partial class LibVLCSharpPage : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private bool _isPageDisappearing = false;
        private IHttpStream _httpStream;
        private LibVLC LibVLC { get; set; }
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;

        public LibVLCSharp.Shared.MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        private bool IsLoaded { get; set; }
        private bool IsVideoViewInitialized { get; set; }

        public LibVLCSharpPage(IHttpStream httpStream)
        {
            InitializeComponent();
            _httpStream = httpStream;

            Initialize();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }

        // New constructor that takes a media URI as an input
        public LibVLCSharpPage(Uri mediaUri)  // Calls the existing constructor
        {
            //MediaUri = mediaUri
            InitializeComponent();
            Initialize(mediaUri);
            Debug.WriteLine($"LibVLCSharpPage initialized with URI: {mediaUri}");
                var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }


        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Initialize()
        {
            LibVLC = new LibVLC(enableDebugLogs: true);
            var media = new Media(LibVLC, new Uri("http://localhost:8888"));
            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC) { Media = media };

            // Subscribe to MediaPlayer.MediaChanged event
            MediaPlayer.MediaChanged += OnMediaPlayerMediaChanged;
        }

        // Overloaded Initialize method to accept media URI
        private void Initialize(Uri mediaUri )
        {
            LibVLC = new LibVLC(enableDebugLogs: true);
            var media = new Media(LibVLC, mediaUri ?? new Uri("http://localhost:8888")); // Default to localhost if mediaUri is null
            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC) { Media = media };

            // Subscribe to MediaPlayer.MediaChanged event
            MediaPlayer.MediaChanged += OnMediaPlayerMediaChanged;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsLoaded = true;

            // Bind the MediaPlayer to the VideoView before playing
            if (!IsVideoViewInitialized)
            {
                VideoView.MediaPlayer = MediaPlayer;
                IsVideoViewInitialized = true;
            }

            Play();

            // Update the button state based on media player status
            UpdatePlayPauseButton();
        }

        private void OnMediaPlayerMediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
        {
            Play(); // Start playback when the media is changed
        }

        private void Play()
        {
            if (IsLoaded && IsVideoViewInitialized && MediaPlayer?.Media != null)
            {
                MediaPlayer.Play(); // Ensure MediaPlayer is valid
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isPageDisappearing = true;
            DisposeMediaPlayer();
            _httpStream?.Dispose();
        }

        private void DisposeMediaPlayer()
        {
            var mediaPlayer = MediaPlayer;
            if (mediaPlayer?.IsPlaying == true)
            {
                try
                {
                    mediaPlayer.Stop(); // Ensure it stops before disposing
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping MediaPlayer: {ex.Message}");
                }
                finally
                {
                    mediaPlayer.Dispose();
                }
            }
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
            IsVideoViewInitialized = true;
            Play();
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
                var mediaPlayer = MediaPlayer;

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
            AdjustMediaTime(-10000); // Seek backward 10 seconds
        }

        private void OnSeekForwardClicked(object sender, EventArgs e)
        {
            AdjustMediaTime(10000); // Seek forward 10 seconds
        }

        private void OnSkipOpeningClicked(object sender, EventArgs e)
        {
            AdjustMediaTime(90000); // Skip forward 90 seconds
        }

        private void OnUnSkipOpeningClicked(object sender, EventArgs e)
        {
            AdjustMediaTime(-90000); // Skip backward 90 seconds
        }

        private void AdjustMediaTime(long offset)
        {
            var mediaPlayer = MediaPlayer;
            if (mediaPlayer != null)
            {
                long newTime = mediaPlayer.Time + offset;

                // Prevent time from going below 0 or exceeding video length
                if (newTime < 0)
                    newTime = 0;
                else if (newTime > mediaPlayer.Length)
                    newTime = mediaPlayer.Length;

                mediaPlayer.Time = newTime;
            }
        }

        private void OnProgressBarDragStarted(object sender, EventArgs e)
        {
            _isDragging = true; // Pause progress updates during drag
        }

        private void OnProgressBarDragCompleted(object sender, EventArgs e)
        {
            var mediaPlayer = MediaPlayer;

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

            var mediaPlayer = MediaPlayer;

            if (!_isDragging && mediaPlayer?.Media != null && mediaPlayer.Length > 0)
            {
                // Update progress based on the percentage (0-1)
                ProgressBar.Value = (double)mediaPlayer.Time / mediaPlayer.Length;

                // Update the time label
                TimeLabel.Text = $"{TimeSpan.FromMilliseconds(mediaPlayer.Time):mm\\:ss} / {TimeSpan.FromMilliseconds(mediaPlayer.Length):mm\\:ss}";

                // Stop media and reset play button if video ends
                if (mediaPlayer.Time >= mediaPlayer.Length)
                {
                    mediaPlayer.Pause();
                    _isPlaying = false;
                    PlayPauseButton.Source = "Play.png"; // Reset to play button
                }
            }

            return true; // Continue updating
        }

        private string UpdateTimeBar()
        {
            var mediaPlayer = MediaPlayer;
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

        private void UpdatePlayPauseButton()
        {
            if (MediaPlayer != null)
            {
                _isPlaying = MediaPlayer.IsPlaying;
                PlayPauseButton.Source = _isPlaying ? "Pause.png" : "Play.png";
            }
        }
    }
}
#endif
