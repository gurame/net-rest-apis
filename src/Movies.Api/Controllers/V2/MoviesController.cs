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
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
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
}
