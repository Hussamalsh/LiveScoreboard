using LiveScoreboard.Interfaces;
using LiveScoreboard.Models;
using LiveScoreboard.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Runtime.CompilerServices;

namespace LiveScoreboard.Tests;

[TestFixture]
public class ScoreboardTests
{
    private Scoreboard _scoreboard;
    private Mock<IFixtureRepository> _mockRepository;
    private ILogger<Scoreboard> _logger;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IFixtureRepository>();
        _logger = new NullLogger<Scoreboard>();
        _scoreboard = new Scoreboard(_mockRepository.Object, _logger);
    }

    #region StartMatch Tests

    [Test]
    public async Task StartFixture_AddsNewFixture_WhenFixtureDoesNotExist()
    {
        int fixtureId = 1;
        string homeTeam = "Team A";
        string awayTeam = "Team B";

        // Arrange: Ensure GetById returns null to simulate fixture does not exist
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync((Fixture)null);

        // Act
        await _scoreboard.StartFixtureAsync(fixtureId, homeTeam, awayTeam);

        var fixtures = await _scoreboard.GetFixturesAsync();

        // Assert: Verify that Add method is called with a Fixture having correct properties
        _mockRepository.Verify(r => r.AddAsync(It.Is<Fixture>(f =>
            f.Id == fixtureId &&
            f.HomeTeam == homeTeam &&
            f.AwayTeam == awayTeam)), Times.Once);
    }

    [Test]
    public async Task StartFixture_NullHomeTeam_ThrowsArgumentException()
    {
        int fixtureId = 2;

        // Arrange: Ensure GetById returns null for this new fixture ID
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync((Fixture)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _scoreboard.StartFixtureAsync(fixtureId, null, "Team B"));
        Assert.That(ex.ParamName, Is.EqualTo("homeTeam"), "Exception should be thrown for null homeTeam.");
    }

    [Test]
    public async Task StartFixture_WithDuplicateFixtureId_ThrowsInvalidOperationException()
    {
        int fixtureId = 1;
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync(new Fixture());

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _scoreboard.StartFixtureAsync(fixtureId, "Team A", "Team B"));
        Assert.That(ex.Message, Is.EqualTo($"Attempted to start a fixture that already exists. Match ID: {fixtureId}"));
    }

    [Test]
    public async Task StartFixture_WhenAddThrowsException_RethrowsException()
    {
        int fixtureId = 4;
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync((Fixture)null);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Fixture>())).Throws(new Exception("Unexpected error during add."));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _scoreboard.StartFixtureAsync(fixtureId, "Team E", "Team F"), "Method should rethrow exceptions from the repository.");
    }

    #endregion

    #region UpdateScore Tests

    [TestCase(1, 3, 2)]
    [TestCase(2, 0, 0, Description = "Verifying that zero scores are handled correctly")]
    public async Task UpdateScore_UpdatesCorrectly(int fixtureId, int homeScore, int awayScore)
    {
        // Arrange
        var fixture = new Fixture { Id = fixtureId, HomeTeam = "Team Home", AwayTeam = "Team Away", Score = new FixtureScore() };
        _mockRepository.Setup(repo => repo.GetByIdAsync(fixtureId)).ReturnsAsync(fixture);

        // Act
        await _scoreboard.UpdateScoreAsync(fixtureId, homeScore, awayScore);

        // Assert
        _mockRepository.Verify(repo => repo.UpdateAsync(It.Is<Fixture>(f =>
            f.Id == fixtureId &&
            f.Score.HomeScore == homeScore &&
            f.Score.AwayScore == awayScore)),
            Times.Once, "UpdateScore should correctly update the fixture's scores in the repository.");
    }

    [Test]
    public async Task UpdateScore_WhenFixtureDoesNotExist_ThrowsInvalidOperationException()
    {
        int fixtureId = 99;

        // Arrange: The repository does not find the fixture (returns null).
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync((Fixture)null);

        // Act & Assert: Attempting to update the score for a non-existent fixture.
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _scoreboard.UpdateScoreAsync(fixtureId, 1, 1));

        var expectedMessage = $"Attempted to update score for a fixture that does not exist. Match ID: {fixtureId}";
        Assert.That(exception.Message, Is.EqualTo(expectedMessage), "The exception message should accurately reflect the absence of the specified fixture.");
    }

    [Test]
    public async Task UpdateScore_WithNegativeHomeScore_ThrowsArgumentOutOfRangeException()
    {
        // Arrange: Setup a fixture with ID 3 and no scores
        var fixture = new Fixture { Id = 3, HomeTeam = "Team X", AwayTeam = "Team Y", Score = new FixtureScore() };
        _mockRepository.Setup(repo => repo.GetByIdAsync(3)).ReturnsAsync(fixture);
        // Act & Assert: Attempting to update the score with a negative home score should throw ArgumentOutOfRangeException
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scoreboard.UpdateScoreAsync(3, -1, 0), "Updating with a negative home score should throw ArgumentOutOfRangeException.");
    }

    [Test]
    public async Task UpdateScore_WithNegativeAwayScore_ThrowsArgumentOutOfRangeException()
    {
        // Arrange: Setup a fixture with ID 4 and no scores
        var fixture = new Fixture { Id = 4, HomeTeam = "Team Z", AwayTeam = "Team W", Score = new FixtureScore() };
        _mockRepository.Setup(repo => repo.GetByIdAsync(4)).ReturnsAsync(fixture);
        // Act & Assert: Attempting to update the score with a negative away score should throw ArgumentOutOfRangeException
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _scoreboard.UpdateScoreAsync(4, 0, -1), "Updating with a negative away score should throw ArgumentOutOfRangeException.");
    }

    [Test]
    public async Task UpdateScore_ThrowsException_WhenUnexpectedExceptionOccursDuringUpdate()
    {
        // Arrange
        int fixtureId = 1;
        int homeScore = 2;
        int awayScore = 3;

        // Setup the GetById method to return a fixture
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync(new Fixture { Id = fixtureId, Score = new FixtureScore(0, 0) });

        // Setup the Update method to throw an exception
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Fixture>())).Throws(new Exception("Unexpected error during update."));

        // Act & Assert: Expect the custom exception to be thrown when Update is called
        var ex = Assert.ThrowsAsync<Exception>(() => _scoreboard.UpdateScoreAsync(fixtureId, homeScore, awayScore));

        Assert.That(ex.Message, Is.EqualTo("Unexpected error during update."), "Method should rethrow exceptions from the repository.");
    }


    #endregion

    #region FinishMatch Tests

    [Test]
    public void FinishFixture_WhenFixtureExists_RemovesFixture()
    {
        // Arrange
        int fixtureId = 1;
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync(new Fixture { Id = fixtureId });
        // Act
        _scoreboard.FinishFixtureAsync(fixtureId);
        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(fixtureId), Times.Once, "Expected Delete to be called once for existing fixture");
    }

    [Test]
    public async Task FinishFixture_WhenFixtureDoesNotExist_ThrowsInvalidOperationExceptionAsync()
    {
        int fixtureId = 99;

        // Arrange: The repository does not find the fixture (returns null).
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync((Fixture)null);

        // Act & Assert: Attempting to finish a non-existent fixture should throw InvalidOperationException.
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _scoreboard.FinishFixtureAsync(fixtureId));

        // Assert: Verify the exact exception message.
        var expectedMessage = $"Attempted to finish a fixture that does not exist. Fixture ID: {fixtureId}";
        Assert.That(exception.Message, Is.EqualTo(expectedMessage), "The exception message should match the expected message for a non-existent fixture.");
    }


    [Test]
    public async Task FinishFixture_ThrowsException_WhenUnexpectedExceptionOccursDuringDelete()
    {
        // Arrange
        int fixtureId = 1;
        _mockRepository.Setup(r => r.GetByIdAsync(fixtureId)).ReturnsAsync(new Fixture { Id = fixtureId });

        // Setup the Delete method to throw an exception
        _mockRepository.Setup(expression: r => r.DeleteAsync(fixtureId)).Throws(new Exception("Unexpected error during delete."));

        // Act & Assert: Expect the custom exception to be thrown when Delete is called
        var ex = Assert.ThrowsAsync<Exception>(async () => await _scoreboard.FinishFixtureAsync(fixtureId));

        Assert.That(ex.Message, Is.EqualTo("Unexpected error during delete."), "Method should rethrow exceptions from the repository.");
    }

    #endregion

    #region GetSummary Tests

    [Test]
    public async Task GetSummary_ReturnsFormattedSummary()
    {
        // Arrange
        var fixtures = new List<Fixture>
        {
            new Fixture { Id = 1, HomeTeam = "Team A", AwayTeam = "Team B", StartTime = DateTime.UtcNow.AddHours(-1), Score = new FixtureScore(2, 3) },
            new Fixture { Id = 2, HomeTeam = "Team C", AwayTeam = "Team D", StartTime = DateTime.UtcNow, Score = new FixtureScore(1, 1) }
        };

        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>>>()))
               .ReturnsAsync(fixtures);

        // Act
        var summary = await _scoreboard.GetSummaryAsync();
        // Assert
        Assert.AreEqual(2, summary.Count, "Summary should include all fixtures.");
        Assert.AreEqual("Team A 2 - Team B 3", summary[0], "The summary for the first fixture should match expected formatting.");
        Assert.AreEqual("Team C 1 - Team D 1", summary[1], "The summary for the second fixture should match expected formatting.");
    }

    // Testing GetSummary with no matches
    [Test]
    public async Task GetSummary_WhenNoMatches_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(expression: repo => repo.GetAllAsync(null)).ReturnsAsync(new List<Fixture>());

        // Act
        var summary = await _scoreboard.GetSummaryAsync();

        // Assert
        Assert.IsEmpty(summary, "Summary should be empty when no fixtures have been started.");
    }

    // Additional test to cover ordering by total score and then start time
    [Test]
    public async Task GetSummary_WithSameTotalScores_OrdersByStartTime()
    {
        // Arrange: Two fixtures with the same total score, but different start times
        var fixtureEarlier = new Fixture(1, "Team A", "Team B", 2, 3, DateTime.UtcNow.AddMinutes(-10));
        var fixtureLater = new Fixture(2, "Team C", "Team D", 3, 2, DateTime.UtcNow.AddMinutes(-5));

        var fixturesInCorrectOrder = new List<Fixture> { fixtureEarlier, fixtureLater };
        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>>>()))
               .ReturnsAsync(fixturesInCorrectOrder);
        // Act
        var summary = await _scoreboard.GetSummaryAsync();

        // Verify that the summary reflects the order provided by the repository
        Assert.AreEqual(2, summary.Count, "Expected summary to contain exactly 2 fixtures.");
        Assert.AreEqual($"Team A 2 - Team B 3", summary[0], "The first fixture in the summary should be the one that started earlier.");
        Assert.AreEqual($"Team C 3 - Team D 2", summary[1], "The second fixture in the summary should be the one that started later.");
    }

    // Testing the robustness of the GetSummary method with a large number of fixtures
    [Test]
    public async Task GetSummary_WithLargeNumberOfFixtures_HandlesAllCorrectly()
    {
        // Adjusting the instantiation of Fixture objects to use the constructor
        var fixtures = Enumerable.Range(1, 100).Select(i =>
            new Fixture(i, $"Team {i}", $"Team {i + 100}", i % 3, (i + 1) % 3, DateTime.UtcNow.AddSeconds(-i))
        ).ToList();

        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>>>()))
               .ReturnsAsync(fixtures);

        var summary = await _scoreboard.GetSummaryAsync();

        Assert.AreEqual(100, summary.Count, "Expected summary to contain 100 fixtures.");
    }

    [Test]
    public async Task GetSummary_ThrowsException_WhenUnexpectedExceptionOccursDuringRetrieval()
    {
        // Setup the GetAll method to throw an exception
        _mockRepository.Setup(repo => repo.GetAllAsync(It.IsAny<Func<IEnumerable<Fixture>, IOrderedEnumerable<Fixture>>>()))
                       .Throws(new Exception("Unexpected error during retrieval."));

        // Act & Assert: Expect the custom exception to be thrown when GetAll is called
        var ex = Assert.ThrowsAsync<Exception>(() => _scoreboard.GetSummaryAsync());

        Assert.That(ex.Message, Is.EqualTo("Unexpected error during retrieval."), "Method should rethrow exceptions from the retrieval process.");
    }


    #endregion

}
