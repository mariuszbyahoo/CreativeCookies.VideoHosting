using CreativeCookies.VideoHosting.Contracts.Infrastructure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using CreativeCookies.VideoHosting.DTOs.Email;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace CreativeCookies.VideoHosting.Infrastructure
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IMyHubBlobService _blobService;
        private readonly IInvoiceNumsRepository _invoiceNumsRepository;
        private readonly ILogger<IInvoiceService> _logger;

        public InvoiceService(IMyHubBlobService blobService, IInvoiceNumsRepository invoiceNumsRepo, ILogger<IInvoiceService> logger)
        {
            _blobService = blobService;
            _invoiceNumsRepository = invoiceNumsRepo;
            _logger = logger;
        }

        public async Task<Attachement> GenerateInvoicePdf(decimal amount, string currency, InvoiceAddressDto buyerAddress, MerchantDto merchant)
        {            
            _logger.LogInformation($"Starting invoice generation to: {buyerAddress.FirstName} {buyerAddress.LastName}");
            var invoiceNumber = await _invoiceNumsRepository.GetNewNumber();
            var merchantHouseNoLine = $"{merchant.HouseNo} " + (merchant.AppartmentNo != null ? $"lok. {merchant.AppartmentNo}" : "");
            var buyerHouseNoLine = $"{buyerAddress.HouseNo} " + (buyerAddress.AppartmentNo != null ? $"lok. {buyerAddress.AppartmentNo}" : "");
            var merchantAddress = $"{merchant.Street} {merchantHouseNoLine}, {merchant.PostCode}, {merchant.City}, {merchant.Country}";
            var buyerAddressLine = $"{buyerAddress.Street} {buyerHouseNoLine}, {buyerAddress.PostCode}, {buyerAddress.City}, {buyerAddress.Country}";
            var nettAmount = amount / 1.23m / 100;
            var grossAmount = amount / 100;
            var nettAmountTxt = nettAmount.ToString("N2");
            var grossAmountTxt = grossAmount.ToString("N2");
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Faktura VAT nr. {invoiceNumber}";

            // Add a page
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Create fonts
            XFont titleFont = new XFont("Default", 20, XFontStyleEx.Bold);
            XFont headerFont = new XFont("Default", 14, XFontStyleEx.Bold);
            XFont regularBoldFont = new XFont("Default", 10, XFontStyleEx.Bold);
            XFont regularFont = new XFont("Default", 10);

            // Define the colors
            XBrush brush = new XSolidBrush(XColor.FromArgb(255, 255, 215)); // Background color (Gold)
            XBrush blackBrush = new XSolidBrush(XColor.FromArgb(0, 0, 0)); // Text color (Black)

            // Draw the background
            gfx.DrawRectangle(brush, 0, 0, page.Width, page.Height);

            // Draw the title
            gfx.DrawString($"Faktura VAT nr. {invoiceNumber}", titleFont, blackBrush, new XRect(0, 50, page.Width, page.Height), XStringFormats.TopCenter);

            // Define the initial vertical position
            var issuerYPos = 120;
            var merchantYPos = 240;
            var buyerYPos = 240;
            var lineSpacing = 20; // Space between lines
            // Define the page layout margins
            var leftMargin = 40;
            var rightMargin = (int)page.Width - 40; // Adjust as needed
            var topMargin = 120;
            var columnWidth = (int)page.Width / 3; // Middle point of the page for two-column layout

            gfx.DrawString("Wystawione przez:", headerFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth, page.Height), XStringFormats.TopLeft);
            issuerYPos += lineSpacing;
            gfx.DrawString("Creative Cookies sp. z o.o.", regularFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            issuerYPos += lineSpacing;
            gfx.DrawString("ul. Dunikowskiego 8" + " " + merchantHouseNoLine, regularFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            issuerYPos += lineSpacing;
            gfx.DrawString("05-501 Piaseczno", regularFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            issuerYPos += lineSpacing;
            gfx.DrawString("Polska", regularFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            issuerYPos += lineSpacing;
            gfx.DrawString("VAT EU 1231479701", regularFont, blackBrush, new XRect(leftMargin, issuerYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);


            // Draw the header for the seller
            gfx.DrawString("Sprzedawca:", headerFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            merchantYPos += lineSpacing;
            gfx.DrawString(merchant.CompanyName, regularFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            merchantYPos += lineSpacing;
            gfx.DrawString(merchant.Street + " " + merchantHouseNoLine, regularFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            merchantYPos += lineSpacing;
            gfx.DrawString(merchant.PostCode + " " + merchant.City, regularFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            merchantYPos += lineSpacing;
            gfx.DrawString(merchant.Country, regularFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);
            merchantYPos += lineSpacing;
            gfx.DrawString("NIP " + merchant.CompanyTaxId, regularFont, blackBrush, new XRect(leftMargin, merchantYPos, columnWidth - leftMargin, lineSpacing), XStringFormats.TopLeft);

            // Draw the header for the buyer
            gfx.DrawString("Kupujący:", headerFont, blackBrush, new XRect(columnWidth, buyerYPos, rightMargin - columnWidth, lineSpacing), XStringFormats.TopLeft);
            buyerYPos += lineSpacing;
            gfx.DrawString(buyerAddress.FirstName + " " + buyerAddress.LastName, regularFont, blackBrush, new XRect(columnWidth, buyerYPos, rightMargin - columnWidth, lineSpacing), XStringFormats.TopLeft);
            buyerYPos += lineSpacing;
            gfx.DrawString(buyerAddress.Street + " " + buyerHouseNoLine, regularFont, blackBrush, new XRect(columnWidth, buyerYPos, rightMargin - columnWidth, lineSpacing), XStringFormats.TopLeft);
            buyerYPos += lineSpacing;
            gfx.DrawString(buyerAddress.PostCode + " " + buyerAddress.City, regularFont, blackBrush, new XRect(columnWidth, buyerYPos, rightMargin - columnWidth, lineSpacing), XStringFormats.TopLeft);
            buyerYPos += lineSpacing;
            gfx.DrawString(buyerAddress.Country, regularFont, blackBrush, new XRect(columnWidth, buyerYPos, rightMargin - columnWidth, lineSpacing), XStringFormats.TopLeft);

            int tableStartY = 370;
            int columnSpacing = 70; // Adjust the spacing between the columns as needed

            // Draw the table headers with adjusted positions
            gfx.DrawString("Ilość", regularBoldFont, blackBrush, new XRect(leftMargin, tableStartY, columnSpacing, page.Height), XStringFormats.TopLeft);
            gfx.DrawString("Usługa", regularBoldFont, blackBrush, new XRect(leftMargin + columnSpacing, tableStartY, columnSpacing, page.Height), XStringFormats.TopLeft);
            gfx.DrawString("Netto", regularBoldFont, blackBrush, new XRect(leftMargin + 3 * columnSpacing, tableStartY, columnSpacing, page.Height), XStringFormats.TopLeft);
            gfx.DrawString("VAT", regularBoldFont, blackBrush, new XRect(leftMargin + 4 * columnSpacing, tableStartY, columnSpacing, page.Height), XStringFormats.TopLeft);
            gfx.DrawString("Brutto", regularBoldFont, blackBrush, new XRect(leftMargin + 5 * columnSpacing, tableStartY, columnSpacing, page.Height), XStringFormats.TopLeft);

            // Draw the table content
            gfx.DrawString("1.", regularFont, blackBrush, new XRect(40, tableStartY + 20, 50, page.Height), XStringFormats.TopLeft);
            gfx.DrawString("Subskrypcja", regularFont, blackBrush, new XRect(100, tableStartY + 20, 50, page.Height), XStringFormats.TopLeft);
            gfx.DrawString($"{nettAmountTxt} {currency}", regularFont, blackBrush, new XRect(250, tableStartY + 20, 50, page.Height), XStringFormats.TopLeft);
            gfx.DrawString($"23%", regularFont, blackBrush, new XRect(350, tableStartY + 20, 50, page.Height), XStringFormats.TopLeft);
            gfx.DrawString($"{grossAmountTxt} {currency}", regularFont, blackBrush, new XRect(450, tableStartY + 20, page.Width, page.Height), XStringFormats.TopLeft);

            // Draw the total
            gfx.DrawString($"Kwota należna ogółem: {grossAmount.ToString("N2")} {currency}", regularFont, blackBrush, new XRect(40, tableStartY + 80, page.Width, page.Height), XStringFormats.TopLeft);

            // Save the document into a MemoryStream
            MemoryStream stream = new MemoryStream();
            document.Save(stream, false);
            _logger.LogInformation($"Generated an invoice number: {invoiceNumber}");

            var uploadRes = await _blobService.UploadPdfToAzureAsync(stream.ToArray(), $"{invoiceNumber}.pdf");
            _logger.LogInformation($"Uploaded an invoice number: {invoiceNumber} to Azure Container");
            _logger.LogInformation($"Retunrning invoice: {invoiceNumber} from the method");
            var result = new Attachement(invoiceNumber, stream.ToArray());
            return result;
        }
    }
}