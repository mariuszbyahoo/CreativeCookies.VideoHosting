﻿using CreativeCookies.VideoHosting.Contracts.ModelContracts;

namespace CreativeCookies.VideoHosting.Domain.Models
{
    public class FilmTile : IFilmTile
    {
        public string Name { get; set; }
        public string ThumbnailName { get; set; }
        public string Length { get; set; }
        public string CreatedOn { get; set; }
    }
}