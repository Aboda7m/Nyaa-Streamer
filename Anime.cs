using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;

namespace Nyaa_Streamer
{
    public class Anime
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Synopsis { get; set; }
        public int? Episodes { get; set; } // Assuming episodes are an integer
        public int Id { get; set; } // Add Id to reference the correct anime
        public double? Score { get; set; } // Added property for score
        public string AiringTime { get; set; } // Added property for airing time in GMT as a string

        // Method to convert JST time to GMT
        public static string ConvertJSTToGMT(string day, string time)
        {
            // Combine day and time for DateTime parsing
            string dateTimeString = $"{day} {time}";

            try
            {
                // Assuming the broadcast day is in English (like "Saturdays")
                DateTime airingTimeJST = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture);

                // Get the current date and combine with the time
                var today = DateTime.Now;
                DateTime airingDateTime = airingTimeJST.Date + airingTimeJST.TimeOfDay;

                // Convert 'Saturdays' to 'Saturday' for the enum
                string daySingular = day.TrimEnd('s'); // Convert 'Saturdays' to 'Saturday'

                // Adjust airingDateTime to the correct day
                while (airingDateTime.DayOfWeek != (DayOfWeek)Enum.Parse(typeof(DayOfWeek), daySingular, true))
                {
                    airingDateTime = airingDateTime.AddDays(1);
                }

                // Convert JST to GMT
                TimeZoneInfo jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                DateTime airingTimeGMT = TimeZoneInfo.ConvertTime(airingDateTime, jstZone, TimeZoneInfo.Utc);

                // Return in "HH:mm" format
                return airingTimeGMT.ToString("HH:mm");
            }
            catch (FormatException)
            {
                // Handle format exceptions and log if necessary
                return "Invalid time format"; // or handle as you see fit
            }
            catch (ArgumentException ex)
            {
                // Handle cases where the day is invalid
                return "Invalid day format: " + ex.Message; // or handle as you see fit
            }
        }



        // Method to populate anime details from API response
        public static async Task<List<Anime>> FetchAnimeDetailsAsync(string apiUrl)
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetFromJsonAsync<AnimeApiResponse>(apiUrl);

            List<Anime> animeList = new List<Anime>();
            if (response != null && response.data != null)
            {
                foreach (var animeData in response.data)
                {
                    animeList.Add(new Anime
                    {
                        Title = animeData.title,
                        ImageUrl = animeData.images.jpg.image_url,
                        Id = animeData.mal_id,
                        Synopsis = animeData.synopsis,
                        Episodes = animeData.episodes,
                        Score = animeData.score,
                        // Convert airing time from JST to GMT, if available
                        AiringTime = animeData.broadcast != null ? ConvertJSTToGMT(animeData.broadcast.day, animeData.broadcast.time) : null
                    });
                }
            }
            return animeList;
        }
    }

    public class AnimeApiResponse
    {
        public List<AnimeData> data { get; set; }
    }

    public class AnimeData
    {
        public string synopsis { get; set; }
        public string title { get; set; }
        public int mal_id { get; set; } // The ID from the API response
        public AnimeImages images { get; set; }
        public int? episodes { get; set; }
        public double? score { get; set; } // Added property for score
        public BroadcastInfo broadcast { get; set; } // Added property for broadcast info
    }

    public class BroadcastInfo
    {
        public string day { get; set; } // Day of airing
        public string time { get; set; } // Time of airing (in JST)
        public string timezone { get; set; } // Timezone information
    }

    public class AnimeImages
    {
        public AnimeImage jpg { get; set; }
    }

    public class AnimeImage
    {
        public string image_url { get; set; }
    }
}
