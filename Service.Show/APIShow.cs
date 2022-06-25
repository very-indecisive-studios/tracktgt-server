﻿using Domain;
using Domain.Media;

namespace Service.Show;

public record APIShow(
    string Id,
    string CoverImageURL,
    string Title,
    string Summary,
    ShowType ShowType
);
