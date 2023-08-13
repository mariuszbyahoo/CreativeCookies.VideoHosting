﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Enums
{
    public enum StripeConnectAccountStatus
    {
        Disconnected,
        Restricted,
        Connected,
        PendingSave
    }
}
