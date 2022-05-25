using Domain;

namespace Service.Show;

public record APIShowBasic(
    int Id,
    string CoverImageURL,
    string Title,
    ShowType ShowType
);