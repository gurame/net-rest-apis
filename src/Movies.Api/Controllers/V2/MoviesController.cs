using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
public class MoviesController : ControllerBase
{
	private readonly IMovieService _movieService;
	private readonly ILogger<MoviesController> _logger;
    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
	[HttpGet(ApiEndpoints.Movies.Get)]
	public async Task<IActionResult> Get([FromServices]LinkGenerator linkGenerator, [FromRoute] string idOrSlug)
	{
		var movie = Guid.TryParse(idOrSlug, out var id)
			? await _movieService.GetByIdAsync(id)
			: await _movieService.GetBySlugAsync(idOrSlug);

		if (movie == null)
		{
			return NotFound();
		}
		var response = movie.MapToResponse();
		var movieObj = new { id = response.Id };
		response.Links.Add(new Link
		{
			Href = linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { idOrSlug = movieObj.id })!,
			Rel = "self",
			Type = "GET"
		});
		return Ok(response);
	}

	[AllowAnonymous]
	[HttpGet(ApiEndpoints.Movies.Genres)]
	[Produces("application/json", "application/xml")]
	[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept, Accept-Encoding")]
	[MapToApiVersion(1.0)]
	public async Task<IActionResult> GetGenres()
	{
		_logger.LogInformation("Getting genres");
		// create a list of strings of genres
		var genres = await Task.Run(()=> new List<MovieGenreResponse> { 
			new() { Name = "Action" },
			new() { Name = "Comedy" },
			new() { Name = "Drama" },
			new() { Name = "Horror" },
			new() { Name = "Sci-Fi" }
		});
		return Ok(genres);
	}
}
