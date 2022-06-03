﻿using Domain;

namespace Service.Show;

public interface IShowService
{
    Task<List<APIShowBasic>> SearchShowByTitle(string title);

    Task<APIShow?> GetShowById(int id, ShowType showType);
}