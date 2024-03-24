namespace LiveScoreboard.Models;

public class FixtureScore
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public FixtureScore(int homeScore = 0, int awayScore = 0)
    {
        HomeScore = homeScore;
        AwayScore = awayScore;
    }

    public int TotalScore => HomeScore + AwayScore;
}
