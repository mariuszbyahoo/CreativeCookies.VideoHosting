﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IClientStore
    {
        Task<IOAuthClient> FindByClientIdAsync(string clientId);
    }
}
