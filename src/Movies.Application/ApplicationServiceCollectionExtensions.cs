using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IMovieRepository, MovieRepository>();
		return services;
	}
	public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
	{
		services.AddSingleton<DbInitializer>();
		services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
		return services;
	}
}
