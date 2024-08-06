﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Api.Auth;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Asp.Versioning;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
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
		response.Links.Add(new Link
		{
			Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: new { id = movieObj.id })!,
			Rel = "self",
			Type = "PUT"
		});
		response.Links.Add(new Link
		{
			Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { id = movieObj.id })!,
			Rel = "self",
			Type = "DELETE"
		});

		return Ok(response);
	}

	[Authorize(AuthConstants.TrustedMemberPolicyName)]
	[HttpGet(ApiEndpoints.Movies.GetAll)]
	public async Task<IActionResult> GetAll([FromQuery]GetAllMovieRequest request)
	{
		var userId = HttpContext.GetUserId();
		var options = request.MapToOptions()
			.WithUserId(userId);

		var movies = await _movieService.GetAllAsync(options);
		var moviesCount = await _movieService.GetCountAsync(request.Title, request.Year);

		var response = movies.MapToResponse(request.Page, request.PageSize, moviesCount);
		return Ok(response);
	}
	
	[Authorize(AuthConstants.AdminUserPolicyName)]
	[HttpPost(ApiEndpoints.Movies.Create)]
	public async Task<IActionResult> Create([FromBody]CreateMovieRequest request)
	{
		var movie = request.MapToMovie();
		await _movieService.CreateAsync(movie);
		var response = movie.MapToResponse();
		return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
	}

	[Authorize(AuthConstants.AdminUserPolicyName)]
	[HttpPut(ApiEndpoints.Movies.Update)]
	public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody]UpdateMovieRequest request)
	{
		var movie = request.MapToMovie(id);
		var updatedMovie = await _movieService.UpdateAsync(movie);
		if (updatedMovie == null)
		{
			return NotFound();
		}
		var response = updatedMovie.MapToResponse();
		return Ok(response);
	}

	[Authorize(AuthConstants.AdminUserPolicyName)]
	[HttpDelete(ApiEndpoints.Movies.Delete)]
	public async Task<IActionResult> Delete([FromRoute] Guid id)
	{
		var deleted = await _movieService.DeleteByIdAsync(id);
		if (!deleted)
		{
			return NotFound();
		}
		return NoContent();
	}
}
