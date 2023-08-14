using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Films
{
    public class FilmsPaginatedResultDto
    {
        public IEnumerable<FilmTileDto> Films { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasMore { get; set; }
    }
}
