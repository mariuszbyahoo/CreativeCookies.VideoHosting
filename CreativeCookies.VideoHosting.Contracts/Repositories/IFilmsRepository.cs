﻿using CreativeCookies.VideoHosting.Contracts.ModelContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IFilmsRepository
    {
        IFilmsPaginatedResult GetFilmsPaginatedResult(string search, int pageNumber, int pageSize);
    }
}
