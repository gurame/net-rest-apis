namespace Movies.Api.Auth;

public static class IdentityExtensions
{
	public static Guid? GetUserId(this HttpContext httpContext)
	{
		var userId = httpContext.User.Claims.SingleOrDefault(x => x.Type == "userId");
		if (Guid.TryParse(userId?.Value, out var result))
		{
			return result;
		}
		return null;
	}
}
