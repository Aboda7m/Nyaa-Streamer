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
            Initialize(); // Call the initialize method

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnScreenTapped;
            VideoView.GestureRecognizers.Add(tapGestureRecognizer);

            // Start updating the progress bar
            Device.StartTimer(TimeSpan.FromMilliseconds(500), UpdateProgressBar);
        }

        // New constructor that takes a media URI as an input
        public LibVLCSharpPage(Uri mediaUri)
        {
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
            media.Parse(); // Ensure the media is parsed before accessing tracks



        }

        // Overloaded Initialize method to accept media URI
        private void Initialize(Uri mediaUri)
        {
            LibVLC = new LibVLC(enableDebugLogs: true);
            var media = new Media(LibVLC, mediaUri ?? new Uri("http://localhost:8888")); // Default to localhost if mediaUri is null
            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC) { Media = media };

            // Subscribe to MediaPlayer.MediaChanged event
            MediaPlayer.MediaChanged += OnMediaPlayerMediaChanged;
            media.Parse(); // Ensure the media is parsed before accessing tracks


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
            _isPlaying = true;
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
                        PlayPauseButton.Source = "play.png";
                    }
                    else
                    {
                        mediaPlayer.Play();
                        PlayPauseButton.Source = "pause.png";
                    }

                    _isPlaying = !_isPlaying;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPlayPauseButtonClicked: {ex.Message}");
            }
        }

        private async void OnSubtitleButtonClicked(object sender, EventArgs e)
        {
            if (MediaPlayer != null)
            {
                // Get the currently selected subtitle track ID
                int currentSpu = MediaPlayer.Spu;

                // Check if subtitles are available
                int spuCount = MediaPlayer.SpuCount;

                if (spuCount > 0)
                {
                    // Retrieve available subtitle tracks
                    var subtitleTracks = MediaPlayer.SpuDescription;

                    // Create a list to hold subtitle names
                    var subtitleOptions = new List<string>();
                    int selectedSpuId = -1; // Variable to hold selected subtitle ID

                    for (int i = 0; i < subtitleTracks.Length; i++)
                    {
                        subtitleOptions.Add(subtitleTracks[i].Name);
                    }

                    // Create an action sheet for subtitle selection
                    string subtitleChoice = await DisplayActionSheet("Select Subtitle", "Cancel", null, subtitleOptions.ToArray());

                    // Check if the user made a selection
                    if (subtitleChoice != null && subtitleChoice != "Cancel")
                    {
                        // Find the selected subtitle ID
                        selectedSpuId = Array.Find(subtitleTracks, track => track.Name == subtitleChoice).Id;

                        // Instead of Native, call the method directly on MediaPlayer
                        MediaPlayer.SetSpu(selectedSpuId); // Use MediaPlayer to set the SPU

                        // Confirm the selected subtitle
                        await DisplayAlert("Subtitle Set", $"You have set the subtitle to: {subtitleChoice}", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Subtitle Information", "No subtitle tracks available.", "OK");
                }
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

            // Load the media and print subtitle names
           
           // PrintSubtitleNames();
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

                // Update the time label with HH:mm:ss format
                TimeLabel.Text = UpdateTimeBar(); // Call UpdateTimeBar for consistent formatting

                // Stop media and reset play button if video ends
                if (mediaPlayer.Time >= mediaPlayer.Length)
                {
                    mediaPlayer.Pause();
                    _isPlaying = false;
                    PlayPauseButton.Source = "play.png"; // Reset to play button
                }
                UpdatePlayPauseButton();
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
                // Return the formatted time with hours, minutes, and seconds
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
                PlayPauseButton.Source = _isPlaying ? "pause.png" : "play.png";
            }
        }
    }
}
#endif
