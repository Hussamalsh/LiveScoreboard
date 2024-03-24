# Live Football World Cup Scoreboard Library

The Live Football World Cup Scoreboard library provides a simple in-memory solution for tracking and displaying scores for ongoing football matches. Designed with simplicity and quality in mind, it supports operations such as starting a new match, updating scores, finishing a match, and retrieving a summary of matches in progress.

## Features

- Start a new match with an initial score of 0 - 0.
- Update the score of an ongoing match.
- Finish a match, removing it from the scoreboard.
- Get a summary of matches, ordered by total score and most recently started.

## Requirements

- Use an in-memory store solution (for example just use collections to store the information you might require).
- We don’t expect the solution to be a REST API, command line application, a Web Service, or Microservice. Just a simple library implementation.
- Focus on Quality. Use Test-Driven Development (TDD), pay attention to OO design, Clean Code and adherence to SOLID principles.
- Add a README.md file where beside the project documentation you can make notes of any assumption or things you would like to mention to us about your solution. 

## Assumptions

In developing the Live Football World Cup Scoreboard library, made several key assumptions that influenced our technology choices and design considerations:

- **Technology Stack:** Assumed that users and developers integrating this library have access to environments that support C# and .NET Core 8. 
  
- **Match Uniqueness:** Each match is identified by a unique ID, assumed to be managed externally to ensure uniqueness across matches.

- **Score Non-negativity:** Scores for both home and away teams are non-negative integers. The library does not accommodate negative scores.

- **Concurrent Matches:** The library is designed to manage multiple matches concurrently, assuming the volume of concurrent matches and frequency of score updates will be within the capacity of an in-memory storage solution.

- **Order of Operations:** Matches will be logically managed (started, updated, and finished in order). Strict chronological enforcement beyond basic checks is not implemented.

- **Environmental Simplicity:** Developed with the assumption of operating in a controlled, possibly single-threaded environment. Highly concurrent or multi-threaded scenarios may require additional synchronization.

These assumptions guided the initial development phase and helped to streamline the design process.


## Getting Started

To use the Live Football World Cup Scoreboard library in your project, include the library and ensure your project targets .NET Core 8. The library does not require any external dependencies outside of the .NET Core framework.

### Installation

Clone this repository or download the library into your project. Use the .NET CLI or your preferred development environment to build and reference the assembly in your application.

### Usage

1. **Initialize the Scoreboard Service:**

First, add the required services to your service collection. This typically occurs in the `Startup.cs` or wherever you configure services for your application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddLiveScoreboard();
    // Add other services
}
```

2. **Start a New Match:**

To start tracking a new match, use the `StartFixtureAsync` method with the match ID, home team, and away team names.

```csharp
await scoreboardService.StartFixtureAsync(fixtureId, "HomeTeamName", "AwayTeamName");
```

3. **Update Score:**

To update the score for an ongoing match, use the `UpdateScoreAsync` method with the match ID and the new scores.

```csharp
await scoreboardService.UpdateScoreAsync(fixtureId, homeScore, awayScore);
```

4. **Finish a Match:**

To mark a match as finished and remove it from the scoreboard, use the `FinishFixtureAsync` method with the match ID.

```csharp
await scoreboardService.FinishFixtureAsync(fixtureId);
```

5. **Get Summary:**

To get a summary of all ongoing matches, ordered by total score and most recently started, use the `GetSummaryAsync` method.

```csharp
var summary = await scoreboardService.GetSummaryAsync();
```

### Contributing

Contributions to the Live Football World Cup Scoreboard library are welcome. Please follow standard coding conventions and add unit tests for any new or changed functionality. Ensure all tests pass before submitting a pull request.

## Author

Designed with ❤️ by Hussam