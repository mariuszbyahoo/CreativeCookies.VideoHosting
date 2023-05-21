using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class BlobUrlResult : IBlobUrlResult
    {
        public string BlobUrl { get; }

        public BlobUrlResult(string blobUrl)
        {
            BlobUrl = blobUrl;
        }
    }
}
