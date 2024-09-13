Certainly! Here is the updated README.md with the code block characters replaced as requested:

# Nyaa Streamer

Nyaa Streamer is a media streaming application built with .NET MAUI and MonoTorrent, designed to stream video files from torrents using a custom-built video player. The application supports both Android and iOS platforms, leveraging the LibVLCSharp library for video playback.

## Overview

The project includes the following main components:

1. **Main Page**
2. **Torrent Manager Page**
3. **Video Player Page**

### Main Page

The `MainPage.xaml` serves as the entry point of the application. It contains a simple user interface with a button to navigate to the torrent manager page.

**Key Features:**
- **Navigation Button:** Allows users to navigate to the `TorrentManagerPage`.

### Torrent Manager Page

The `TorrentManagerPage.xaml` provides an interface for managing torrent files. It displays a list of torrent files and includes a button to start streaming the selected file.

**Key Features:**
- **ListView:** Displays the list of torrent files with their names and sizes.
- **Start Stream Button:** Initiates streaming for the selected file.

**Code Behind (TorrentManagerPage.xaml.cs):**
- **LoadTorrentFiles():** Loads the list of torrent files from the manager and updates the ListView.
- **OnFileSelected():** Enables the stream button when a file is selected.
- **OnStreamButtonClicked():** Starts the streaming process for the selected file.
- **StartTorrentStreamAsync():** Handles the streaming logic, including starting an HTTP server and redirecting to a media player.
- **StartHttpServer():** Creates an HTTP stream for the selected torrent file.
- **StartRedirectServer():** Redirects the HTTP stream to a local server.
- **StartVlcProcess():** Launches VLC player with the streaming URL.

### Video Player Page

The `LibVLCSharpPage.xaml` is the custom video player page that uses the LibVLCSharp library for video playback.

**Key Features:**
- **VideoView:** Displays the video content.
- **Custom Control Bar:** Includes buttons for play/pause, seek forward/backward, skip openings, and display of playback time.

**Control Elements:**
- **ProgressBar:** Allows users to seek through the video.
- **PlayPauseButton:** Toggles play/pause state.
- **SeekBackwardButton:** Moves the video backward by a set duration.
- **SeekForwardButton:** Moves the video forward by a set duration.
- **SkipOpeningButton:** Skips the current video opening.
- **UnSkipOpeningButton:** Reverts the skip if the opening was missed.

## Setup and Build

To build and run the application:

1. **Clone the Repository:**
   ``` 
   git clone https://github.com/Aboda7m/Nyaa-Streamer
   ```
2. **Navigate to the Project Directory:**
   ``` 
   cd Nyaa-Streamer
   ```
3. **Restore Dependencies:**
   ``` 
   dotnet restore
   ```
4. **Build the Project:**
   ``` 
   dotnet build
   ```
5. **Run the Application:**
   ``` 
   dotnet run
   ```

## Contributions

Feel free to contribute to the project by submitting pull requests or opening issues. Your feedback and contributions are welcome!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.