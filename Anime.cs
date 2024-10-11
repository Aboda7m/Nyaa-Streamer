using System;
using System.Collections.Generic;
using System.Globalization;

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

            // Assuming the broadcast day is in English (like "Saturdays")
            DateTime airingTimeJST = DateTime.ParseExact(dateTimeString, "dddd HH:mm", CultureInfo.InvariantCulture);

            // Convert JST to GMT
            TimeZoneInfo jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTime airingTimeGMT = TimeZoneInfo.ConvertTime(airingTimeJST, jstZone, TimeZoneInfo.Utc);

            // Return in "HH:mm" format
            return airingTimeGMT.ToString("HH:mm");
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
