using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(decimal amount, string currency, AddressDto buyerAddress, MerchantDto merchant);
        string GetInvoiceNumber();
    }
}
