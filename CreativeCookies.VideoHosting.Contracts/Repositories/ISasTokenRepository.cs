using CreativeCookies.VideoHosting.Contracts.ModelContracts;
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
        ISasTokenResult GetSasTokenForFilm(string filmName, string containerName);
        ISasTokenResult GetSasTokenForFilmUpload(string filmName, string containerName);
        ISasTokenResult GetSasTokenForThumbnail(string thumbnailName, string containerName);
        ISasTokenResult GetSasTokenForThumbnailUpload(string thumbnailName, string containerName);
    }
}
