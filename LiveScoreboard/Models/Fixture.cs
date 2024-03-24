namespace LiveScoreboard.Models;

public class Fixture
{
    public int Id { get; set; }
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public FixtureScore Score { get; set; } = new FixtureScore();
    public DateTime StartTime { get; set; }

    public Fixture(int matchId, string homeTeam, string awayTeam, int homeScore = 0, int awayScore = 0, DateTime? startTime = null)
    {
        Id = matchId;
        HomeTeam = homeTeam ?? throw new ArgumentNullException(nameof(homeTeam));
        AwayTeam = awayTeam ?? throw new ArgumentNullException(nameof(awayTeam));
        Score = new FixtureScore(homeScore, awayScore);
        StartTime = startTime ?? DateTime.UtcNow;
    }

    public Fixture() { }
}
