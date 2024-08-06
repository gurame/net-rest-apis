using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;
public class DatabaseHealthCheck : IHealthCheck
{
	public const string Name = "DatabaseHealthCheck";
	private readonly IDbConnectionFactory _dbConnectionFactory;
	private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
		{
			_ = await _dbConnectionFactory.CreateConnectionAsync();
			return HealthCheckResult.Healthy();
		}
		catch (Exception e)
		{
			const string errorMessage = "Database is not available: {error}";
			_logger.LogError(errorMessage, e);
			return HealthCheckResult.Unhealthy(errorMessage, e);
		}
    }
}
