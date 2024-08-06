using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
	public static Movie MapToMovie(this CreateMovieRequest request)
	{
		return new Movie
		{
			Id = Guid.NewGuid(),
			Title = request.Title,
			YearOfRelease = request.YearOfRelease,
			Genres = request.Genres.ToList(),
		};
	}

	public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
	{
		return new Movie
		{
			Id = id,
			Title = request.Title,
			YearOfRelease = request.YearOfRelease,
			Genres = request.Genres.ToList(),
		};
	}

	public static MovieResponse MapToResponse(this Movie movie)
	{
		return new MovieResponse
		{
			Id = movie.Id,
			Title = movie.Title,
			YearOfRelease = movie.YearOfRelease,
			Slug = movie.Slug,
			Genres = movie.Genres,
		};
	}
	
	public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, int page, int pageSize, int totalCount)
	{
		return new MoviesResponse
		{
			Items = movies.Select(x => x.MapToResponse()),
			Page = page,
			PageSize = pageSize,
			Total = totalCount
		};
	}

	public static GetAllMoviesOptions MapToOptions(this GetAllMovieRequest request)
	{
		return new GetAllMoviesOptions
		{
			Title = request.Title,
			Year = request.Year,
			SortField = request.SortBy?.Trim('+', '-'),
			SortOrder = request.SortBy is null ? SortOrder.Unsorted : request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
			Page = request.Page,
			PageSize = request.PageSize
		};
	}

	public static GetAllMoviesOptions WithUserId(this GetAllMoviesOptions options, Guid? userId)
	{
		options.UserId = userId;
		return options;
	}
}
