using LiveScoreboard.Interfaces;
using LiveScoreboard.Repo;
using LiveScoreboard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveScoreboard.Extensions;

/// <summary>
/// Provides extension methods for the IServiceCollection interface to facilitate the registration of
/// Live Football World Cup Scoreboard services and configurations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the necessary services for the Live Football World Cup Scoreboard library to the specified IServiceCollection.
    /// This includes setting up logging, the scoreboard service, and the fixture repository.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection, allowing for chaining of multiple calls.</returns>
    public static IServiceCollection AddLiveScoreboard(this IServiceCollection services)
    {
        // Register the ILogger service with default configurations.
        // Note: The host application can override this by configuring logging before or after calling this method.
        services.AddLogging(configure => configure.AddConsole());

        // Register IScoreboard & IFixtureRepository with its implementation
        services.AddTransient<IScoreboard, Scoreboard>();
        services.AddSingleton<IFixtureRepository, FixtureRepository>();

        return services;
    }
}