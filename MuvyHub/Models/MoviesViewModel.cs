using System.Collections.Generic;

namespace MuvyHub.Models
{
    public class MoviesViewModel
    {
        public List<TmdbResult> PopularMovies { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> TopRatedMovies { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> PopularTvShows { get; set; } = new List<TmdbResult>();
        public List<TmdbResult> TopRatedTvShows { get; set; } = new List<TmdbResult>();
    }
}
