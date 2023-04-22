using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface ISasTokenRepository
    {
        ISasTokenResult GetSasTokenForContainer(string containerName);
        ISasTokenResult GetSasTokenForFilm(string filmName);
        ISasTokenResult GetSasTokenForFilmUpload(string filmName);
        ISasTokenResult GetSasTokenForThumbnail(string thumbnailName);
        ISasTokenResult GetSasTokenForThumbnailUpload(string thumbnailName);
    }
}
