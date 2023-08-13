﻿using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using CreativeCookies.VideoHosting.Contracts.Wrappers;
using CreativeCookies.VideoHosting.Domain.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IConnectAccountsRepository _connectAccountsRepository;
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeController> _logger;
        private readonly IConfiguration _configuration;

        public StripeController(
            IConnectAccountsRepository connectAccountsRepository, IStripeService stripeService, 
            ILogger<StripeController> logger, IConfiguration configuration)
        {
            _connectAccountsRepository = connectAccountsRepository;
            _stripeService = stripeService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("IsSetUp")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<IStripeResult<StripeConnectAccountStatus>>> IsStripeAccountSetUp()
        {
            IStripeResult<StripeConnectAccountStatus> result = new StripeResult<StripeConnectAccountStatus>()
            {
                Data = StripeConnectAccountStatus.Disconnected,
                Success = true,
                ErrorMessage = "account_missing"
            };
            var idStoredInDatabase = await _connectAccountsRepository.GetConnectedAccountId();
            if (!string.IsNullOrWhiteSpace(idStoredInDatabase))
            {
                var olderThanMinute = await _connectAccountsRepository.CanBeQueriedOnStripe(idStoredInDatabase);
                if (olderThanMinute) result = _stripeService.GetAccountStatus(idStoredInDatabase);
                else 
                { 
                    result.Data = StripeConnectAccountStatus.PendingSave;
                    result.Success = true;
                    result.ErrorMessage = "An account has been recently submitted, check at least one minute after creating";
                }
            }
            return Ok(result);
        }

        [HttpGet("OnboardingRefresh")]
        public async Task<IActionResult> RefreshOnboarding()
        {
            var accountLinkResponse = _stripeService.GenerateConnectAccountLink();
            return Redirect(accountLinkResponse.Data.AccountOnboardingUrl);
        }

        [HttpPost("AccountUpdatedWebhook")]
        public async Task<IActionResult> AccountUpdatedWebHook()
        {
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret);

                if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Account;
                    await _connectAccountsRepository.EnsureSaved(account.Id);
                }
                else
                {
                    _logger.LogWarning($"Unexpected Stripe event's type: {stripeEvent.ToJson()}");
                    return BadRequest();
                }
            }
            catch (StripeException e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest("Stripe exception occured");
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
            }

            return Ok();
        }
    }
}
