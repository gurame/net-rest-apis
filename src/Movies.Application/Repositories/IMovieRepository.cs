using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
	Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options);
	Task<Movie?> GetByIdAsync(Guid id);
	Task<Movie?> GetBySlugAsync(string slug);
	Task<bool> CreateAsync(Movie movie);
	Task<bool> UpdateAsync(Movie movie);
	Task<bool> DeleteByIdAsync(Guid id);
	Task<bool> ExistsAsync(Guid id);
	Task<int> GetCountAsync(string? title, int? year);
}
