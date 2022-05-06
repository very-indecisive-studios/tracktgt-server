namespace Tracker.Service.Game;

public record APIGame(
    long Id,
    string CoverImageURL,
    string Title,
    string Summary,
    double Rating,
    List<string> Platforms,
    List<string> Companies
);
