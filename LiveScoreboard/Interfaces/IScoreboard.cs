using LiveScoreboard.Models;

namespace LiveScoreboard.Interfaces;

public interface IScoreboard
{
    /// <summary>
    /// Starts a new fixture with the given details.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture.</param>
    /// <param name="homeTeam">The name of the home team.</param>
    /// <param name="awayTeam">The name of the away team.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StartFixtureAsync(int fixtureId, string homeTeam, string awayTeam);

    /// <summary>
    /// Updates the score for a specific fixture.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to update.</param>
    /// <param name="homeScore">The new home team score.</param>
    /// <param name="awayScore">The new away team score.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateScoreAsync(int fixtureId, int homeScore, int awayScore);

    /// <summary>
    /// Finishes a fixture, removing it from active fixtures.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to finish.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task FinishFixtureAsync(int fixtureId);

    /// <summary>
    /// Retrieves a summary of all current fixtures.
    /// </summary>
    /// <returns>A list of strings representing the summary of each fixture.</returns>
    Task<IList<string>> GetSummaryAsync();

    /// <summary>
    /// Retrieves all fixtures.
    /// </summary>
    /// <returns>A list of <see cref="Fixture"/> objects.</returns>
    Task<List<Fixture>> GetFixturesAsync();
}
