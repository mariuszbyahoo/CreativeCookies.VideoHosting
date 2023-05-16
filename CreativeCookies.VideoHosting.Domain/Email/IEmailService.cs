﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
