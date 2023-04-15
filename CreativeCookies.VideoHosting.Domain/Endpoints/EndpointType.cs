using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Endpoints
{
    public enum EndpointType
    {
        ListBlobs = 0,
        BlobRead = 1,
        BlobUpload = 2
    }
}
