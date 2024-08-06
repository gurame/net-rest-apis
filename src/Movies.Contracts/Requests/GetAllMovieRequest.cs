namespace Movies.Contracts.Requests;
public class GetAllMovieRequest : PagedRequest
{
	public required string? Title { get; set; }
	public required int? Year { get; set; }
	public required string? SortBy { get; set; }
}
