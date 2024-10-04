namespace Nyaa_Streamer
{
    public class Anime
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Synopsis { get; set; } // Add this if you need it for both pages
    }


    public class AnimeApiResponse
    {
        public AnimeData[] data { get; set; }
    }

    public class AnimeData
    {
        public string title { get; set; }
        public AnimeImages images { get; set; }
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
