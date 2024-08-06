using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
	private readonly IDbConnectionFactory _dbConnectionFactory;
	public DbInitializer(IDbConnectionFactory dbConnectionFactory)
	{
		_dbConnectionFactory = dbConnectionFactory;
	}

	public async Task InitializeAsync()
	{
		using var connection = await _dbConnectionFactory.CreateConnectionAsync();
		await connection.ExecuteAsync(@"
			create table if not exists movies
			(
				id UUID PRIMARY KEY,
				title TEXT NOT NULL,
				slug TEXT NOT NULL,
				yearofrelease INTEGER NOT NULL
			)
		");
		await connection.ExecuteAsync(@"
			create unique index concurrently if not exists idx_movies_slug
			on movies
			using btree (slug)
		");
		await connection.ExecuteAsync(@"
			create table if not exists genres
			(
				movie UUID references movies(id),
				name TEXT NOT NULL
			)
		");
	}
}
