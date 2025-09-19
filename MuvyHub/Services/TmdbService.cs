using MuvyHub.Models;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MuvyHub.Services
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Tmdb:ApiKey"] ?? throw new ArgumentNullException("TMDB API Key not found in configuration.");

            _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            var url = endpoint.Contains("?") ? $"{endpoint}&api_key={_apiKey}" : $"{endpoint}?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(content, options);
        }

        private async Task<TmdbPagedResponse<TmdbResult>?> GetAndSetMediaType(string endpoint, string mediaType)
        {
            var response = await GetAsync<TmdbPagedResponse<TmdbResult>>(endpoint);
            if (response?.Results != null)
            {
                foreach (var item in response.Results)
                {
                    item.MediaType = mediaType;
                }
            }
            return response;
        }

        public async Task<TmdbPagedResponse<TmdbResult>?> GetPopularMoviesAsync()
            => await GetAndSetMediaType("movie/popular", "movie");

        public async Task<TmdbPagedResponse<TmdbResult>?> GetTopRatedMoviesAsync()
            => await GetAndSetMediaType("movie/top_rated", "movie");

        public async Task<TmdbPagedResponse<TmdbResult>?> GetPopularTvShowsAsync()
            => await GetAndSetMediaType("tv/popular", "tv");

        public async Task<TmdbPagedResponse<TmdbResult>?> GetTopRatedTvShowsAsync()
            => await GetAndSetMediaType("tv/top_rated", "tv");

        public async Task<TmdbResult?> GetDetailsAsync(string mediaType, int id)
        {
            var details = await GetAsync<TmdbResult>($"{mediaType}/{id}");
            if (details != null)
            {
                details.MediaType = mediaType;
            }
            return details;
        }

        public async Task<TmdbVideoResponse?> GetVideosAsync(string mediaType, int id)
        {
            return await GetAsync<TmdbVideoResponse>($"{mediaType}/{id}/videos");
        }

        public async Task<TmdbPagedResponse<TmdbResult>?> SearchAsync(string query)
        {
            return await GetAsync<TmdbPagedResponse<TmdbResult>>($"search/multi?query={Uri.EscapeDataString(query)}");
        }
    }
}
