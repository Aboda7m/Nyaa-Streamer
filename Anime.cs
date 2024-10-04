namespace Nyaa_Streamer
{
    public class Anime
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Synopsis { get; set; }
        public int? Episodes { get; set; } // Assuming episodes are an integer
        public int Id { get; set; } // Add Id to reference the correct anime
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
