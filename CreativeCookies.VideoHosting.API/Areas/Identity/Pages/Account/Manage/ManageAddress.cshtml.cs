using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account.Manage
{
    public class ManageAddressModel : PageModel
    {
        private readonly IMyHubUserManager _userManager;
        private readonly IAddressService _addressService;

        [BindProperty]
        public InvoiceAddressDto Address { get; set; }

        public ManageAddressModel(IMyHubUserManager userManager, IAddressService addressService)
        {
            _userManager = userManager;
            _addressService = addressService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            Address = await _addressService.GetAddress(user.Id.ToString());
            if (Address == null)
            {
                Address = new InvoiceAddressDto();
                Address.Id = Guid.Empty;
                Address.UserId = Guid.Empty.ToString();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            Address.UserId = userId;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _addressService.UpsertAddress(Address);

            return RedirectToPage("./ManageAddress", new { success = true });
        }
    }

}
