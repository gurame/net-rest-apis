using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
	private static readonly string[] AcceptableSortFields = ["title", "yearofrelease"];

	public GetAllMoviesOptionsValidator()
	{
		RuleFor(x => x.Year).LessThanOrEqualTo(DateTime.Now.Year);
		RuleFor(x => x.SortField).Must(x => x is null || AcceptableSortFields.Contains(x))
			.WithMessage($"Sort field must be one of {string.Join(", ", AcceptableSortFields)}");
	}
}
