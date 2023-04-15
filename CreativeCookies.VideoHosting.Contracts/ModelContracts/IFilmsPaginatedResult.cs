using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.ModelContracts
{
    public interface IFilmsPaginatedResult
    {
        IEnumerable<IFilmTile> Films { get; set; }
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
        bool HasMore { get; set; }
    }
}
