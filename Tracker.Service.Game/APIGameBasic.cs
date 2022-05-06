namespace Tracker.Service.Game;

public record APIGameBasic(
    long Id,
    string Title,
    List<string> Platforms
);