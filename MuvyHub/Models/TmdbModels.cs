
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MuvyHub.Models
{
    public class TmdbResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("genres")]
        public List<Genre> Genres { get; set; } = new List<Genre>();

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        public string DisplayTitle => Title ?? Name ?? "No Title";

        public string FullPosterPath => PosterPath != null ? $"https://image.tmdb.org/t/p/w500{PosterPath}" : "https://placehold.co/500x750/222/FFF?text=No+Image";

        public string FullBackdropPath => BackdropPath != null ? $"https://image.tmdb.org/t/p/w1280{BackdropPath}" : "https://placehold.co/1280x720/000/FFF?text=No+Backdrop";
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
    public class TmdbPagedResponse<T>
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new List<T>();
    }

    public class TmdbVideoResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("results")]
        public List<TmdbVideoResult> Results { get; set; } = new List<TmdbVideoResult>();
    }

    public class TmdbVideoResult
    {
        [JsonPropertyName("iso_639_1")]
        public string? Language { get; set; }

        [JsonPropertyName("iso_3166_1")]
        public string? Country { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("site")]
        public string? Site { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("official")]
        public bool Official { get; set; }

        [JsonPropertyName("published_at")]
        public string? PublishedAt { get; set; }
    }
}
