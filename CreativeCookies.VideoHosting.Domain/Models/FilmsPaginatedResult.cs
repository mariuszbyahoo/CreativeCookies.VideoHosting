﻿using CreativeCookies.VideoHosting.Contracts.ModelContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Models
{
    public class FilmsPaginatedResult : IFilmsPaginatedResult
    {
        public IEnumerable<IFilmTile> Films { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasMore { get; set; }
    }
}
