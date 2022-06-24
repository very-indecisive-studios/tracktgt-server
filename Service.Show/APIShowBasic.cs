using Domain;
using Domain.Media;

namespace Service.Show;

public record APIShowBasic(
    string Id,
    string CoverImageURL,
    string Title,
    ShowType ShowType
);