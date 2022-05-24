using Domain;

namespace Service.Show;

public record APIShow(
    int Id,
    string CoverImageURL,
    string Title,
    string Summary,
    ShowType ShowType
);
