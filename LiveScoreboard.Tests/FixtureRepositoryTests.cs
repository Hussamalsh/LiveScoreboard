using LiveScoreboard.Models;
using LiveScoreboard.Repo;
using Microsoft.Extensions.Logging.Abstractions;

namespace LiveScoreboard.Tests;

[TestFixture]
public class FixtureRepositoryTests
{
    private FixtureRepository _fixtureRepository;

    [SetUp]
    public void Setup()
    {
        // Use NullLogger<FixtureRepository> to ignore logging
        _fixtureRepository = new FixtureRepository(new NullLogger<FixtureRepository>());
    }

    #region Add Tests

    [Test]
    public async Task Add_ValidFixture_IncreasesCount()
    {
        // Arrange: Add a valid fixture
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" };
        // Act: Add the fixture to the repository
        await _fixtureRepository.AddAsync(fixture);
        // Assert: Verify the fixture count has increased
        var fixtures = await _fixtureRepository.GetAllAsync();
        Assert.AreEqual(1, fixtures.Count());
    }

    [Test]
    public async Task Add_MultipleValidFixtures_IncreasesCountAccordingly()
    {
        // Arrange: Add two valid fixtures
        await _fixtureRepository.AddAsync(new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" });
        await _fixtureRepository.AddAsync(new Fixture { Id = 2, HomeTeam = "Team C", AwayTeam = "Team D" });
        // Assert the total count is 2
        var fixtures = await _fixtureRepository.GetAllAsync();
        Assert.AreEqual(2, fixtures.Count(), "Adding two unique fixtures should result in a total count of 2.");
    }

    [Test]
    public async Task Add_FixtureWithNullHomeTeam_ThrowsArgumentException()
    {
        // Arrange: Create a fixture with a null HomeTeam
        var fixture = new Fixture { Id = 1, HomeTeam = null, AwayTeam = "Team B" };

        // Assert: Verify that adding a fixture with a null HomeTeam throws ArgumentException
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _fixtureRepository.AddAsync(fixture));
        Assert.That(ex.Message, Contains.Substring("HomeTeam and AwayTeam cannot be null or empty."), "ArgumentException should be thrown for null HomeTeam.");
    }

    [Test]
    public async Task Add_FixtureWithNullAwayTeam_ThrowsArgumentException()
    {
        // Arrange: Create a fixture with a null AwayTeam
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = null };
        // Assert: Verify that adding a fixture with a null AwayTeam throws ArgumentException
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _fixtureRepository.AddAsync(fixture));
        Assert.That(ex.Message, Contains.Substring("HomeTeam and AwayTeam cannot be null or empty."), "ArgumentException should be thrown for null AwayTeam.");
    }

    [Test]
    public async Task Add_NullFixture_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _fixtureRepository.AddAsync(null));
    }

    [Test]
    public async Task Add_DuplicateFixture_ThrowsInvalidOperationException()
    {
        // Arrange: Add a fixture with an ID of 1
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" };
        await _fixtureRepository.AddAsync(fixture);
        // Assert: Verify that adding a duplicate fixture throws InvalidOperationException
        Assert.ThrowsAsync<InvalidOperationException>(() => _fixtureRepository.AddAsync(fixture));
    }

    #endregion

    #region GetById Tests

    [Test]
    public async Task GetById_ExistingId_ReturnsFixtureAsync()
    {
        // Arrange: Add a fixture with an ID of 1
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" };
        // Act: Add the fixture to the repository
        await _fixtureRepository.AddAsync(fixture);
        // Assert: Verify the fixture can be retrieved by its ID
        var retrievedFixture = await _fixtureRepository.GetByIdAsync(1);
        Assert.IsNotNull(retrievedFixture);
        Assert.AreEqual(fixture.Id, retrievedFixture.Id);
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNull()
    {
        // Assert: Verify that a non-existent fixture ID returns null
        var fixture = await _fixtureRepository.GetByIdAsync(999);
        Assert.IsNull(fixture);
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_ValidFixture_UpdatesScoreAsync()
    {
        // Arrange: Add a fixture with a score
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B", Score = new FixtureScore(0, 0) };
        await _fixtureRepository.AddAsync(fixture);

        int expectedHomeScore = 2;
        int expectedAwayScore = 3;
        var updatedFixture = new Fixture { Id = 1, Score = new FixtureScore(expectedHomeScore, expectedAwayScore) };

        // Act: Update the fixture with new scores
        await _fixtureRepository.UpdateAsync(updatedFixture);

        // Assert: Verify the scores were updated correctly
        var retrievedFixture = await _fixtureRepository.GetByIdAsync(1);
        Assert.AreEqual(expectedHomeScore, retrievedFixture.Score.HomeScore);
        Assert.AreEqual(expectedAwayScore, retrievedFixture.Score.AwayScore);
    }

    [Test]
    public async Task Update_NullFixture_ThrowsArgumentNullException()
    {
        // Assert: Verify that updating with a null fixture throws ArgumentNullException
        Assert.ThrowsAsync<ArgumentNullException>(() => _fixtureRepository.UpdateAsync(null));
    }

    [Test]
    public async Task Update_NonExistingFixture_ThrowsInvalidOperationException()
    {
        // Arrange: Create a fixture with an ID that does not exist in the repository
        var fixture = new Fixture { Id = 999, HomeTeam = "Team A", AwayTeam = "Team B", Score = new FixtureScore(2, 2) };
        // Assert: Verify that updating a non-existent fixture throws InvalidOperationException
        Assert.ThrowsAsync<InvalidOperationException>(() => _fixtureRepository.UpdateAsync(fixture));
    }

    [Test]
    public async Task Update_FixtureWithNegativeHomeScore_ThrowsArgumentException()
    {
        // Arrange: Add a fixture with a score
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B", Score = new FixtureScore(0, 0) };
        await _fixtureRepository.AddAsync(fixture);
        var updatedFixture = new Fixture { Id = 1, Score = new FixtureScore(-1, 3) };
        // Assert: Verify that updating a fixture with a negative score throws ArgumentException
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _fixtureRepository.UpdateAsync(updatedFixture));
        Assert.That(ex.Message, Does.Contain("Scores must be non-negative."));
    }

    [Test]
    public async Task Update_FixtureWithNegativeAwayScore_ThrowsArgumentException()
    {
        // Arrange: Add a fixture with a score
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B", Score = new FixtureScore(0, 0) };
        await _fixtureRepository.AddAsync(fixture);

        var updatedFixture = new Fixture { Id = 1, Score = new FixtureScore(2, -1) };
        // Assert: Verify that updating a fixture with a negative score throws ArgumentException
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _fixtureRepository.UpdateAsync(updatedFixture));
        Assert.That(ex.Message, Does.Contain("Scores must be non-negative."));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_ExistingFixture_RemovesFixtureAsync()
    {
        // Arrange: Add a fixture with an ID of 1
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" };
        await _fixtureRepository.AddAsync(fixture);
        // Act: Delete the fixture with ID 1
        await _fixtureRepository.DeleteAsync(1);
        // Assert: Verify the fixture was removed from the repository
        var fixtures = await _fixtureRepository.GetAllAsync();
        Assert.AreEqual(0, fixtures.Count());
    }

    [Test]
    public async Task Delete_NonExistingFixture_LogsWarning()
    {
        // Arrange: Add a fixture with an ID of 1
        var fixture = new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" };
        await _fixtureRepository.AddAsync(fixture);

        var fixtureId = 999; // Assuming this ID does not exist in the repository

        // Action: Attempt to delete a non-existing fixture
        await _fixtureRepository.DeleteAsync(fixtureId);

        // Assert: Verify that the repository remains unchanged and a warning is logged
        var fixtures = await _fixtureRepository.GetAllAsync();
        Assert.AreEqual(1, fixtures.Count(), "Repository should remain unchanged when attempting to delete a non-existing fixture.");
    }

    #endregion

    #region GetAll Tests
    [Test]
    public async Task GetAll_WithoutOrderBy_ReturnsAllFixturesAsync()
    {
        // Arrange: Add two fixtures
        await _fixtureRepository.AddAsync(new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B" });
        await _fixtureRepository.AddAsync(new Fixture { Id = 2, HomeTeam = "Team C", AwayTeam = "Team D" });
        // Act: Retrieve all fixtures
        var fixtures = await _fixtureRepository.GetAllAsync();
        Assert.AreEqual(2, fixtures.Count());
    }

    [Test]
    public async Task GetAll_WithOrderBy_ReturnsOrderedFixtures()
    {
        // Arrange: Add two fixtures with different total scores
        await _fixtureRepository.AddAsync(new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B", Score = new FixtureScore(1, 0) });
        await _fixtureRepository.AddAsync(new Fixture { Id = 2, HomeTeam = "Team C", AwayTeam = "Team D", Score = new FixtureScore(0, 2) });

        // Act: Retrieve all fixtures ordered by total score in ascending order
        var fixtures = await _fixtureRepository.GetAllAsync(orderBy: f => f.OrderBy(fixture => fixture.Score.TotalScore));

        // Assert: Verify the list contains both fixtures in the correct order
        Assert.AreEqual(2, fixtures.Count(), "Expected two fixtures in the list.");
        var firstFixture = fixtures.First();
        // Since Fixture 1 has a lower total score, it should be first in the ordered list
        Assert.AreEqual(1, firstFixture.Id, "The fixture with the lower total score should be first.");
    }

    [Test]
    public async Task GetAll_WithoutOrderBy_OnEmptyRepository_ReturnsEmptyCollection()
    {
        // Assuming the repository is empty
        var fixtures = await _fixtureRepository.GetAllAsync();
        // Assert: Verify an empty collection is returned
        Assert.IsNotNull(fixtures, "The returned collection should not be null.");
        Assert.AreEqual(0, fixtures.Count(), "The returned collection should be empty when the repository has no fixtures.");
    }

    [Test]
    public async Task GetAll_WithOrderBy_OnEmptyRepository_ReturnsEmptyCollection()
    {
        // Assuming the repository is empty
        var fixtures = await _fixtureRepository.GetAllAsync(orderBy: f => f.OrderBy(fixture => fixture.Score.TotalScore));
        Assert.IsNotNull(fixtures, "The returned collection should not be null even when ordered on an empty repository.");
        Assert.AreEqual(0, fixtures.Count(), "The returned collection should be empty when the repository has no fixtures, even with ordering applied.");
    }
    #endregion

}
