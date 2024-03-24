using LiveScoreboard.Interfaces;
using LiveScoreboard.Models;
using Microsoft.Extensions.Logging;

namespace LiveScoreboard.Repo;

/// <summary>
/// An implementation of IFixtureRepository that manages football match fixtures in memory.
/// </summary>
public class FixtureRepository : IFixtureRepository
{
    private readonly ILogger<FixtureRepository> _logger;

    private readonly List<Fixture> _fixtures = new();

    /// <summary>
    /// Initializes a new instance of the FixtureRepository class.
    /// </summary>
    /// <param name="logger">Logger for capturing runtime information and errors.</param>
    public FixtureRepository(ILogger<FixtureRepository> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously adds a new fixture to the repository, ensuring it does not already exist.
    /// </summary>
    /// <param name="fixture">The fixture to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the fixture is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the HomeTeam or AwayTeam is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the fixture already exists.</exception>
    public async Task AddAsync(Fixture fixture)
    {
        await Task.Run(() => 
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture), "Cannot add a null fixture.");
            }

            // Check for null or empty HomeTeam and AwayTeam
            if (string.IsNullOrWhiteSpace(fixture.HomeTeam) || string.IsNullOrWhiteSpace(fixture.AwayTeam))
                throw new ArgumentException("HomeTeam and AwayTeam cannot be null or empty.");


            try
            {
                if (_fixtures.Any(m => m.Id == fixture.Id))
                {
                    var message = $"Attempted to add a fixture that already exists. Fixture ID: {fixture.Id}";
                    _logger.LogWarning(message);
                    throw new InvalidOperationException(message);
                }
                _fixtures.Add(fixture);
                _logger.LogInformation($"Fixture added: {fixture.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding fixture");
                throw;
            }
        });
    }

    /// <summary>
    /// Asynchronously retrieves a fixture by its ID.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to find.</param>
    /// <returns>The found fixture, or null if not found.</returns>
    public Task<Fixture> GetByIdAsync(int fixtureId)
    {
        var fixture = _fixtures.FirstOrDefault(m => m.Id == fixtureId);
        if (fixture == null)
        {
            _logger.LogWarning($"Fixture not found. Fixture ID: {fixtureId}");
        }
        return Task.FromResult(fixture);
    }

    /// <summary>
    /// Asynchronously updates an existing fixture's score.
    /// </summary>
    /// <param name="fixture">The fixture to update, including the new scores.</param>
    /// <exception cref="ArgumentNullException">Thrown if the fixture is null.</exception>
    /// <exception cref="ArgumentException">Thrown if scores are negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the fixture does not exist.</exception>
    public async Task UpdateAsync(Fixture fixture)
    {
        await Task.Run(async () =>
        {
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture), "Cannot update with a null fixture.");
            }

            // Validate scores for negativity
            if (fixture.Score.HomeScore < 0 || fixture.Score.AwayScore < 0)
            {
                throw new ArgumentException("Scores must be non-negative.", nameof(fixture));
            }

            try
            {
                var existingFixture = await GetByIdAsync(fixture.Id);
                if (existingFixture == null)
                {
                    var message = $"Attempted to update a fixture that does not exist. Fixture ID: {fixture.Id}";
                    _logger.LogWarning(message);
                    throw new InvalidOperationException(message);
                }

                existingFixture.Score = fixture.Score;

                _logger.LogInformation($"Fixture updated: {fixture.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating fixture. Fixture ID: {fixture.Id}");
                throw;
            }
        });
    }

    /// <summary>
    /// Asynchronously deletes a fixture by its ID.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if the fixture does not exist.</exception>
    public async Task DeleteAsync(int fixtureId)
    {
        await Task.Run(async () => 
        {
            try
            {
                var fixture = await GetByIdAsync(fixtureId);
                if (fixture != null)
                {
                    _fixtures.Remove(fixture);
                    _logger.LogInformation($"Fixture deleted: {fixtureId}");
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete a fixture that does not exist. Fixture ID: {fixtureId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting fixture. Fixture ID: {fixtureId}");
                throw;
            }
        }); 
    }

    /// <summary>
    /// Retrieves all fixtures from the repository, with an optional ordering.
    /// </summary>
    /// <param name="orderBy">An optional function to order the fixtures. If provided, the function is applied to order the fixtures; otherwise, fixtures are returned as they are stored.</param>
    /// <returns>A task representing the asynchronous operation, which upon completion contains an enumerable of fixtures, ordered as specified by the orderBy function if provided.</returns>
    public async Task<IEnumerable<Fixture>> GetAllAsync(Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>> orderBy = null)
    {
        if (orderBy != null)
        {
            return orderBy(_fixtures);
        }
        else
        {
            return _fixtures;
        }
    }

}
