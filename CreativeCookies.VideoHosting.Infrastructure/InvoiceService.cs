using CreativeCookies.VideoHosting.Contracts.Infrastructure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using DinkToPdf;

namespace CreativeCookies.VideoHosting.Infrastructure
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IAddressService _addressSrv;
        private readonly IMerchantService _merchantSrv;
        private readonly IMyHubBlobService _blobService;

        public InvoiceService(IAddressService addressSrv, IMerchantService merchantSrv, IMyHubBlobService blobService)
        {
            _addressSrv = addressSrv;
            _merchantSrv = merchantSrv;
            _blobService = blobService;
        }


        // HACK: GET COUNT INVOICES

        public byte[] GenerateInvoicePdf(decimal amount, string currency, AddressDto buyerAddress, MerchantDto merchant)
        {
            var converter = new BasicConverter(new PdfTools());
            var invoiceNumber = GetInvoiceNumber();
            var merchantHouseNoLine = $"{merchant.HouseNo} " + (merchant.AppartmentNo != null ? $"lok. {merchant.AppartmentNo}" : "");
            var buyerHouseNoLine = $"{buyerAddress.HouseNo} " + (buyerAddress.AppartmentNo != null ? $"lok. {buyerAddress.AppartmentNo}" : "");
            var nettAmount = amount/1.23m / 100;
            var grossAmount = amount / 100;
            var nettAmountTxt = nettAmount.ToString("N2");
            var grossAmountTxt = grossAmount.ToString("N2");

            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Invoice</title>
            <style>
              body {{
                font-family: 'Arial', sans-serif;
                margin: 0;
                padding: 0;
                background-color: #ffd700;
              }}
              .container {{
                padding: 20px;
                background: #ffd700; /* This is a yellow background */
              }}
              .header {{
                text-align: center;
                margin-bottom: 30px;
              }}
              .invoice-number {{
                text-align: right;
                margin-bottom: 20px;
              }}
              .info {{
                margin-bottom: 20px;
              }}
              .info p {{
                margin: 0;
              }}
              .info-section {{
                display: flex;
                justify-content: space-between;
                margin-bottom: 20px;
              }}
              .info-block {{
                flex-basis: 48%;
              }}
              .details-table {{
                width: 100%;
                border-collapse: collapse;
              }}
              .details-table th, .details-table td {{
                border: 1px solid #000;
                padding: 8px;
                text-align: left;
              }}
              .total {{
                text-align: right;
                margin-top: 20px;
              }}
            </style>
            </head>
            <body>
              <div class=""container"">
                <div class=""header"">
                  <img src=""path-to-your-logo.png"" alt=""MyHub Logo"">
                  <h2>Faktura VAT nr. {invoiceNumber}</h2>
                </div>
                <div class=""invoice-number"">
                  <p>Wystawiona przez:</p>
                  <p>Creative Cookies sp. z o.o.</p>
                  <p>ul. Dunikowskiego 8, 05-501 Piaseczno, Polska</p>
                  <p>NIP 1231479701 REGON 387576303</p>
                </div>
                <div class=""info-section"">
                  <div class=""info-block"">
                    <h4>Sprzedawca:</h4>
                    <p>{merchant.CompanyName}</p>
                    <p>NIP: {merchant.CompanyTaxId}</p>
                    <p>{merchant.Street} {merchantHouseNoLine}</p>
                    <p>{merchant.PostCode}, {merchant.City}, {merchant.Country}</p>
                  </div>
                  <div class=""info-block"">
                    <h4>Kupujący:</h4>
                    <p>{buyerAddress.FirstName} {buyerAddress.LastName}, {buyerAddress.Street}, {buyerHouseNoLine}</p>
                    <p>{buyerAddress.PostCode}, {buyerAddress.City}, {buyerAddress.Country}</p>
                  </div>
                </div>
                <div class=""info"">
                  <p>Data wystawienia: {DateTime.UtcNow.Date}</p>
                  <p>Data sprzedaży: {DateTime.UtcNow.Date}</p>
                </div>
                <table class=""details-table"">
                  <thead>
                    <tr>
                      <th>Ilość</th>
                      <th>Usługa</th>
                      <th>Netto</th>
                      <th>VAT</th>
                      <th>Brutto</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>1.</td>
                      <td>Subskrypcja</td>
                      <td>{nettAmountTxt} {currency.ToUpper()}</td>
                      <td>23%</td>
                      <td>{grossAmountTxt} {currency.ToUpper()}</td>
                    </tr>
                  </tbody>
                </table>
                <div class=""total"">
                  <p>Kwota należna ogółem: {grossAmountTxt} {currency.ToUpper()}</p>
                </div>
              </div>
            </body>
            </html>";

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = DinkToPdf.ColorMode.Color,
                    Orientation = DinkToPdf.Orientation.Portrait,
                    PaperSize = DinkToPdf.PaperKind.A4,
            },
                Objects = {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { /* ... */ },
                        FooterSettings = { /* ... */ }
                    }
                }
            };

            var result = converter.Convert(doc);

            _blobService.UploadPdfToAzureAsync(result, $"{invoiceNumber}.pdf");

            return result;
        }

        public string GetInvoiceNumber()
        {
            return "Próbna FV212312";
        }
    }
}
