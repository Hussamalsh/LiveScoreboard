using LiveScoreboard.Models;

namespace LiveScoreboard.Interfaces;

public interface IFixtureRepository
{
    /// <summary>
    /// Adds a new fixture to the repository asynchronously.
    /// </summary>
    /// <param name="fixture">The fixture to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the fixture is null.</exception>
    Task AddAsync(Fixture fixture);

    /// <summary>
    /// Retrieves a fixture by its ID asynchronously.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to retrieve.</param>
    /// <returns>A task result containing the found fixture, or null if not found.</returns>
    Task<Fixture> GetByIdAsync(int fixtureId);

    /// <summary>
    /// Updates an existing fixture in the repository asynchronously.
    /// </summary>
    /// <param name="fixture">The fixture with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the fixture is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the fixture to update does not exist.</exception>
    Task UpdateAsync(Fixture fixture);

    /// <summary>
    /// Deletes a fixture from the repository asynchronously by its ID.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the fixture to delete does not exist.</exception>
    Task DeleteAsync(int fixtureId);

    /// <summary>
    /// Retrieves all fixtures, with an optional ordering function, asynchronously.
    /// Allows the caller to specify how the returned collection should be ordered.
    /// </summary>
    /// <param name="orderBy">A function to order the fixtures. Can be null, in which case the default ordering is used.</param>
    /// <returns>A task result containing an enumerable of all fixtures, possibly ordered according to the provided function.</returns>
    Task<IEnumerable<Fixture>> GetAllAsync(Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>> orderBy = null);
}
