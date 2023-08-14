using CreativeCookies.VideoHosting.DTOs.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface ISasTokenRepository
    {
        SasTokenResultDto GetSasTokenForContainer(string containerName);
        SasTokenResultDto GetSasTokenForFilm(string filmName);
        SasTokenResultDto GetSasTokenForFilmUpload(string filmName);
        SasTokenResultDto GetSasTokenForThumbnail(string thumbnailName);
        SasTokenResultDto GetSasTokenForThumbnailUpload(string thumbnailName);
    }
}
