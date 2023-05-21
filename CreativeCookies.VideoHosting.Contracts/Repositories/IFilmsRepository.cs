﻿using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IFilmsRepository
    {
        Task<IBlobUrlResult> GetBlobUrl(Guid videoId);
        Task<IFilmsPaginatedResult> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize);
    }
}
