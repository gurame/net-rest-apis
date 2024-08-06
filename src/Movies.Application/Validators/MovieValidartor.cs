﻿using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;
public class MovieValidartor : AbstractValidator<Movie>
{
	private readonly IMovieRepository _movieRepository;

    public MovieValidartor(IMovieRepository movieRepository)
    {
		_movieRepository = movieRepository;

        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.Now.Year);
        RuleFor(x => x.Genres).NotEmpty();
        RuleFor(x => x.Slug).MustAsync(ValidateSlug)
			.WithMessage("Slug already exists");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);
		if (existingMovie is not null)
		{
			return existingMovie.Id == movie.Id;
		}
		return existingMovie is null;
    }
}
