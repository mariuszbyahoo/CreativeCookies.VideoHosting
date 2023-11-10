using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Email
{
    public class Attachement
    {
        public string InvoiceNumber { get; set; }
        public string FileNameWithExtension { get; set; }
        public byte[] FileData { get; set; }

        public Attachement(string invoiceNumber, byte[] fileData)
        {
            FileNameWithExtension = $"{invoiceNumber}.pdf";
            FileData = fileData;
            InvoiceNumber = invoiceNumber;
        }
    }
}
