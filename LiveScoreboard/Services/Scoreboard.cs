using LiveScoreboard.Interfaces;
using LiveScoreboard.Models;
using Microsoft.Extensions.Logging;

namespace LiveScoreboard.Services;

/// <summary>
/// Provides functionality to manage football match scores and fixtures.
/// </summary>
public class Scoreboard : IScoreboard
{
    private readonly IFixtureRepository _fixtureRepository;
    private readonly ILogger<Scoreboard> _logger;

    /// <summary>
    /// Initializes a new instance of the Scoreboard service.
    /// </summary>
    /// <param name="fixtureRepository">The repository to manage fixtures.</param>
    /// <param name="logger">Logger for logging operations within the service.</param>
    public Scoreboard(IFixtureRepository fixtureRepository, ILogger<Scoreboard> logger)
    {
        _fixtureRepository = fixtureRepository;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new fixture with given team names. Ensures the fixture does not already exist.
    /// </summary>
    /// <param name="fixtureId">Unique identifier for the fixture.</param>
    /// <param name="homeTeam">Name of the home team.</param>
    /// <param name="awayTeam">Name of the away team.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when team names are null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a fixture with the given ID already exists.</exception>
    public async Task StartFixtureAsync(int fixtureId, string homeTeam, string awayTeam)
    {
        if (string.IsNullOrWhiteSpace(homeTeam))
        {
            throw new ArgumentException("Home team name cannot be null or empty.", nameof(homeTeam));
        }

        if (string.IsNullOrWhiteSpace(awayTeam))
        {
            throw new ArgumentException("Away team name cannot be null or empty.", nameof(awayTeam));
        }

        try
        {
            var existingFixture = await _fixtureRepository.GetByIdAsync(fixtureId);
            if (existingFixture != null)
            {
                var message = $"Attempted to start a fixture that already exists. Match ID: {fixtureId}";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            var newFixture = new Fixture
            {
                Id = fixtureId,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                StartTime = DateTime.UtcNow
            };

            await _fixtureRepository.AddAsync(newFixture);
            _logger.LogInformation($"Match started: {homeTeam} vs {awayTeam} with ID: {fixtureId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting match.");
            throw;
        }
    }

    /// <summary>
    /// Updates the score for an existing fixture.
    /// </summary>
    /// <param name="fixtureId">The identifier of the fixture to update.</param>
    /// <param name="homeScore">The new score for the home team.</param>
    /// <param name="awayScore">The new score for the away team.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the scores are negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the fixture does not exist.</exception>
    public async Task UpdateScoreAsync(int fixtureId, int homeScore, int awayScore)
    {
        if (homeScore < 0 || awayScore < 0)
        {
            throw new ArgumentOutOfRangeException("Scores must be non-negative.");
        }

        try
        {
            var fixture = await _fixtureRepository.GetByIdAsync(fixtureId);
            if (fixture == null)
            {
                var message = $"Attempted to update score for a fixture that does not exist. Match ID: {fixtureId}";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            fixture.Score.HomeScore = homeScore;
            fixture.Score.AwayScore = awayScore;
            await _fixtureRepository.UpdateAsync(fixture);

            _logger.LogInformation($"Score updated for Fixture ID: {fixtureId}. New Score: {homeScore} - {awayScore}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating score.");
            throw;
        }
    }


    /// <summary>
    /// Marks a fixture as finished and removes it from the active fixtures list.
    /// </summary>
    /// <param name="fixtureId">The identifier of the fixture to finish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the fixture does not exist.</exception>
    public async Task FinishFixtureAsync(int fixtureId)
    {
        try
        {
            var fixture = await _fixtureRepository.GetByIdAsync(fixtureId);
            if (fixture == null)
            {
                var message = $"Attempted to finish a fixture that does not exist. Fixture ID: {fixtureId}";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            await _fixtureRepository.DeleteAsync(fixtureId);
            _logger.LogInformation($"Fixture finished: {fixture.HomeTeam} vs {fixture.AwayTeam} with ID: {fixtureId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finishing fixture.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a summary of all fixtures, ordered by total score and start time.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a list of fixture summaries.</returns>
    public async Task<IList<string>> GetSummaryAsync()
    {
        try
        {
            // Fetch all fixtures and order them by total score descending, then by start time ascending
            var fixtures = await _fixtureRepository.GetAllAsync(
                fixtures => fixtures.OrderByDescending(m => m.Score.TotalScore)
                                    .ThenBy(m => m.StartTime));
            var summary = fixtures.Select(f => $"{f.HomeTeam} {f.Score.HomeScore} - {f.AwayTeam} {f.Score.AwayScore}").ToList();
            _logger.LogInformation("Summary requested. Total fixtures: {0}", summary.Count);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fixture summary.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of all fixtures.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a list of all fixtures.</returns>
    public async Task<List<Fixture>> GetFixturesAsync()
    {
        try
        {
            var fixtures = await _fixtureRepository.GetAllAsync();
            return fixtures.ToList();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fixtures.");
            throw;
        }
    }
}
