# Nyaa Streamer

Nyaa Streamer is a media streaming application built with .NET MAUI and MonoTorrent, designed to stream video files from torrents using a custom-built video player. The application supports both Android and iOS platforms, leveraging the LibVLCSharp library for video playback.

## Overview

The project includes the following main components:

1. **Main Page**
2. **Torrent Manager Page**
3. **Video Player Page**
4. **Trending Anime Page**  <!-- New Page Added -->
5. **Search Page**  <!-- New Page Added -->
6. **Weekly Schedule Page**  <!-- New Page Added -->
7. **Favorite Anime Page**  <!-- New Page Added (WIP) -->
8. **Downloaded Files Page**  <!-- New Page Added -->
9. **Anime Details Page**  <!-- New Page Added -->
10. **Watchlist Page**  <!-- New Page Added (WIP) -->
11. **Base Day Page**  <!-- Refactored for Weekly Pages -->
12. **Daily Pages (Monday to Friday)**  <!-- New Pages Added -->

### Main Page

The `MainPage.xaml` serves as the entry point of the application. It contains a simple user interface with a button to navigate to the torrent manager page.

**Key Features:**
- **Navigation Button:** Allows users to navigate to the `TorrentManagerPage`.

### Torrent Manager Page

The `TorrentManagerPage.xaml` provides an interface for managing torrent files. It displays a list of torrent files and includes a button to start streaming the selected file.

**Key Features:**
- **ListView:** Displays the list of torrent files with their names and sizes.
- **Start Stream Button:** Initiates streaming for the selected file.
- **Double-Click Prevention:** Implements a flag to prevent double-clicking on the 'Start Stream' button.

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
- **Subtitle Selection:** Users can select subtitles while streaming.

**Control Elements:**
- **ProgressBar:** Allows users to seek through the video.
- **PlayPauseButton:** Toggles play/pause state.
- **SeekBackwardButton:** Moves the video backward by a set duration.
- **SeekForwardButton:** Moves the video forward by a set duration.
- **SkipOpeningButton:** Skips the current video opening.
- **UnSkipOpeningButton:** Reverts the skip if the opening was missed.

### Trending Anime Page  <!-- New Page Section -->

The `TrendingAnimePage.xaml` displays a list of trending anime titles. Users can view images and titles of popular anime, along with an entry field for additional arguments (like sub-groups or file formats).

**Key Features:**
- **CollectionView:** Displays a list of trending anime with images and titles.
- **Watch/Download Button:** Initiates the watch/download process for the selected anime.

### Search Page  <!-- New Page Section -->

The `SearchPage.xaml` allows users to search for specific anime titles. This page provides a search interface and displays results based on user queries.

**Key Features:**
- **Search Bar:** Enables users to input their desired anime title.
- **Results Display:** Shows the list of anime that match the search criteria.

### Anime Details Page  <!-- New Page Section -->

The `AnimeDetailsPage.xaml` provides detailed information about selected anime, including a synopsis and an option to expand the synopsis.

**Key Features:**
- **Synopsis Display:** Shows a brief synopsis of the anime.
- **Show More Feature:** Expands the synopsis for more information.

### Weekly Schedule Page  <!-- New Page Section -->

The `WeeklySchedulePage.xaml` displays a daily anime schedule for the week (Sunday to Saturday).

**Key Features:**
- **Daily Schedule:** Allows users to view the anime schedule for each day of the week.

### Favorite Anime Page  <!-- New Page Section -->

The `FavoriteAnimePage.xaml` allows users to manage their favorite anime titles.

**Key Features:**
- **Favorites List:** Displays a list of user-selected favorite anime titles.
- **(WIP)**: This feature is still under development.

### Downloaded Files Page  <!-- New Page Section -->

The `DownloadedFilesPage.xaml` provides an interface to manage downloaded files.

**Key Features:**
- **Downloaded Files List:** Displays a list of files downloaded by the user.

### Watchlist Page  <!-- New Page Section -->

The `WatchlistPage.xaml` allows users to maintain a list of anime they plan to watch.

**Key Features:**
- **Watchlist:** Users can add anime titles they wish to watch.
- **(WIP)**: This feature is still under development.

### Base Day Page  <!-- New Page Section -->

The `BaseDayPage.xaml` has been refactored to support the weekly anime schedule, consolidating the functionality for each day's page.

**Key Features:**
- **Dynamic Page Handling:** The structure allows for managing multiple daily pages efficiently.

### Daily Pages (Monday to Friday)  <!-- New Pages Section -->

- **MondayPage.xaml**
- **TuesdayPage.xaml**
- **WednesdayPage.xaml**
- **ThursdayPage.xaml**
- **FridayPage.xaml**
- **SaturdayPage.xaml**
- **SundayPage.xaml**

These pages display the anime schedule for each respective day of the week.

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
