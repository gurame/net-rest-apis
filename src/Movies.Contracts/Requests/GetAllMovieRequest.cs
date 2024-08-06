namespace Movies.Contracts.Requests;

public class GetAllMovieRequest
{
	public required string? Title { get; set; }
	public required int? Year { get; set; }
}
