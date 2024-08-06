using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"
                insert into movies (id, title, slug, yearofrelease)
                values (@Id, @Title, @Slug, @YearOfRelease)
            ", movie));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    @"insert into genres (movie, name)
                      values (@Movie, @Name)", new { Movie = movie.Id, Name = genre }));
            }
        }
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            @"delete from genres where movie = @Movie", new { Movie = id }));

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"delete from movies where id = @Id", new { Id = id }));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            @"select count(1) from movies where id=@Id", new { Id = id }));   
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            orderClause = $"""
                , m.{options.SortField} 
                order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
            """;
        }

        var movies = await connection.QueryAsync(new CommandDefinition(
            @$"select m.*, string_agg(g.name, ',') as genres 
            from movies m
            inner join genres g on m.id = g.movie
            where (@Title is null or m.title like ('%' || @Title || '%'))
            and (@Year is null or m.yearofrelease = @Year)
            group by id {orderClause}
            limit @PageSize 
            offset @PageOffset
            ", new { 
                Title = options.Title, 
                Year = options.Year,
                PageSize = options.PageSize,
                PageOffset = options.PageSize * (options.Page - 1)
            }));

        return movies.Select(movie => new Movie
        {
            Id = movie.id,
            Title = movie.title,
            YearOfRelease = movie.yearofrelease,
            Genres = Enumerable.ToList(movie.genres.Split(","))
        });
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition(
            @"select id, title, slug, yearofrelease from movies where id = @Id", new { Id = id }));
        if (movie == null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition(
            @"select name from genres where movie = @Movie", new { Movie = id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition(
            @"select id, title, slug, yearofrelease from movies where slug = @Slug", new { Slug = slug }));
        if (movie == null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition(
            @"select name from genres where movie = @Movie", new { Movie = movie.Id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
                @"delete from genres where movie = @Movie", new { Movie = movie.Id }));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                @"insert into genres (movie, name)
                    values (@Movie, @Name)", new { Movie = movie.Id, Name = genre }));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            @"
                update movies
                set title = @Title, slug = @Slug, yearofrelease = @YearOfRelease
                where id = @Id
            ", movie));

        transaction.Commit();
        
        return result > 0;
    }
    public async Task<int> GetCountAsync(string? title, int? year)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            @"select count(1) from movies where (@Title is null or title like ('%' || @Title || '%')) and (@Year is null or yearofrelease = @Year)", new { Title = title, Year = year }));
    }
}
