using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Nyaa_Streamer
{
    public class Anime
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Synopsis { get; set; }
        public int? Episodes { get; set; }
        public int Id { get; set; }
        public double? Score { get; set; }
        public string AiringTime { get; set; } // Original airing time (JST)
        public string AiringTimeLocal { get; set; } // Local time converted from JST
        public string AiringTimeGMT { get; set; } // GMT time converted from JST

        // Method to convert JST time to Local time
        public static string ConvertJSTToLocal(string day, string time)
        {
            try
            {
                DateTime airingTimeJST = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture);
                DateTime airingDateTime = airingTimeJST.Date + airingTimeJST.TimeOfDay;

                // Get the correct day of the week
                string daySingular = day.TrimEnd('s'); // Convert 'Saturdays' to 'Saturday'

                // Adjust airingDateTime to the correct day
                while (airingDateTime.DayOfWeek != (DayOfWeek)Enum.Parse(typeof(DayOfWeek), daySingular, true))
                {
                    airingDateTime = airingDateTime.AddDays(1);
                }

                // Convert JST to Local Time
                TimeZoneInfo jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                DateTime localDateTime = TimeZoneInfo.ConvertTime(airingDateTime, jstZone, TimeZoneInfo.Local);

                // Get the day of the week in the local timezone
                string localDay = localDateTime.ToString("dddd", CultureInfo.InvariantCulture);
                string timezoneName = TimeZoneInfo.Local.StandardName; // Get the local timezone name

                return $"{localDay} {localDateTime:HH:mm} ({timezoneName})"; // Format the output
            }
            catch (FormatException)
            {
                return "Invalid time format";
            }
            catch (ArgumentException ex)
            {
                return "Invalid day format: " + ex.Message;
            }
        }

        // Method to convert JST time to GMT
        public static string ConvertJSTToGMT(string day, string time)
        {
            try
            {
                DateTime airingTimeJST = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture);
                DateTime airingDateTime = airingTimeJST.Date + airingTimeJST.TimeOfDay;

                // Get the correct day of the week
                string daySingular = day.TrimEnd('s'); // Convert 'Saturdays' to 'Saturday'

                // Adjust airingDateTime to the correct day
                while (airingDateTime.DayOfWeek != (DayOfWeek)Enum.Parse(typeof(DayOfWeek), daySingular, true))
                {
                    airingDateTime = airingDateTime.AddDays(1);
                }

                // Convert JST to GMT
                TimeZoneInfo jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                DateTime airingTimeGMT = TimeZoneInfo.ConvertTime(airingDateTime, jstZone, TimeZoneInfo.Utc);

                // Get the day in GMT
                string gmtDay = airingTimeGMT.ToString("dddd", CultureInfo.InvariantCulture);
                return $"{gmtDay} {airingTimeGMT:HH:mm} GMT"; // Format the output
            }
            catch (FormatException)
            {
                return "Invalid time format";
            }
            catch (ArgumentException ex)
            {
                return "Invalid day format: " + ex.Message;
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
                    var anime = new Anime
                    {
                        Title = animeData.title,
                        ImageUrl = animeData.images.jpg.image_url,
                        Id = animeData.mal_id,
                        Synopsis = animeData.synopsis,
                        Episodes = animeData.episodes,
                        Score = animeData.score
                    };

                    // Convert airing time from JST to Local and GMT
                    if (animeData.broadcast != null)
                    {
                        anime.AiringTime = $"{animeData.broadcast.day} {animeData.broadcast.time} (JST)";
                        anime.AiringTimeLocal = ConvertJSTToLocal(animeData.broadcast.day, animeData.broadcast.time);
                        anime.AiringTimeGMT = ConvertJSTToGMT(animeData.broadcast.day, animeData.broadcast.time); // Added GMT conversion
                    }

                    animeList.Add(anime);
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
        public int mal_id { get; set; }
        public AnimeImages images { get; set; }
        public int? episodes { get; set; }
        public double? score { get; set; }
        public BroadcastInfo broadcast { get; set; }
    }

    public class BroadcastInfo
    {
        public string day { get; set; }
        public string time { get; set; }
        public string timezone { get; set; }
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
