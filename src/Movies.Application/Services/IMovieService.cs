using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
	Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options);
	Task<Movie?> GetByIdAsync(Guid id);
	Task<Movie?> GetBySlugAsync(string slug);
	Task<bool> CreateAsync(Movie movie);
	Task<Movie?> UpdateAsync(Movie movie);
	Task<bool> DeleteByIdAsync(Guid id);
}
