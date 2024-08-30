# Nyaa Streamer

Nyaa Streamer is a .NET MAUI mobile application designed for searching and streaming anime torrents from the Nyaa.si website. The application allows users to search for anime titles, view search results, and handle torrent links for streaming.

## Features

- **Search Functionality**: 
  - Enter anime names to search on Nyaa.si.
  - Display up to 10 results with their titles.
- **Result Handling**: 
  - Select a result to enable the "Proceed" button.
  - Open the selected URL in the default browser.
- **Torrent Support**: 
  - Handle both magnet links and torrent file URLs.
- **Error Handling**: 
  - Inform users of issues with fetching results or opening URLs.

## Technologies

- **Framework**: .NET MAUI
- **HTML Parsing**: HtmlAgilityPack
- **HTTP Requests**: System.Net.Http
- **Torrent Handling**: MonoTorrent
- **URL Launcher**: Microsoft.Maui.Essentials.Launcher

## Installation

1. Clone the repository:

   ```
   git clone https://github.com/Aboda7m/Nyaa-Streamer.git
   ```

2. Navigate to the project directory:

   ```
   cd Nyaa-Streamer
   ```

3. Restore dependencies:

   ```
   dotnet restore
   ```

4. Build the project:

   ```
   dotnet build
   ```

5. Run the application:

   ```
   dotnet run
   ```

## Usage

1. Open the application.
2. Enter an anime name in the search field and click the search button.
3. View the search results displayed in the list.
4. Select a result to enable the "Proceed" button.
5. Click "Proceed" to open the selected URL in the default browser.

## Error Handling

- **HTTP Requests**: 
  - Handle exceptions and display error messages for failed requests.
- **URL Opening**: 
  - Validate URL formats before attempting to open them.
- **General Errors**: 
  - Display alerts for unexpected errors and issues.

## Contributing

1. Fork the repository.
2. Create a feature branch:

   ```
   git checkout -b feature/your-feature
   ```

3. Commit your changes:

   ```
   git commit -am 'Add new feature'
   ```

4. Push to the branch:

   ```
   git push origin feature/your-feature
   ```

5. Open a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- **HtmlAgilityPack**: For HTML parsing.
- **MonoTorrent**: For torrent handling.
- **.NET MAUI**: For cross-platform development.

