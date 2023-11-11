using CreativeCookies.VideoHosting.DTOs;
using CreativeCookies.VideoHosting.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates an invoice and uploads it to Blob storage associated with MyHub
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="buyerAddress"></param>
        /// <param name="merchant"></param>
        /// <returns>byte[] data for pdf file</returns>
        Task<Attachement> GenerateInvoicePdf(decimal amount, string currency, InvoiceAddressDto buyerAddress, MerchantDto merchant);
    }
}
