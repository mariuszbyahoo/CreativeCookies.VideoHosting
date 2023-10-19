﻿using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersSrv;
        private readonly ICheckoutService _checkoutSrv;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUsersService usersService, ICheckoutService checkoutSrv, ILogger<UsersController> logger)
        {
            _usersSrv = usersService;
            _checkoutSrv = checkoutSrv;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<IList<MyHubUserDto>>> GetAll(int pageNumber = 1, int pageSize = 10, string search = "", string role = "any")
        { 
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            var result = await _usersSrv.GetUsersPaginatedResult(search, pageNumber, pageSize, role);

            return Ok(result);
        }

        [HttpGet("IsASubscriber")]
        [Authorize]
        public async Task<ActionResult<bool>> IsUserASubscriber()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var stac = Request.Cookies["stac"];
            if (tokenHandler.CanReadToken(stac))
            {
                var token = tokenHandler.ReadJwtToken(stac);
                string userId = null;

                foreach (var claim in token.Claims)
                {
                    if (claim.Type.Equals("nameid"))
                    {
                        userId = claim.Value;
                        break;
                    }
                }

                if (userId != null)
                {
                    return await _usersSrv.IsUserSubscriber(userId);
                }
            }
            return false;
        }

        [HttpGet("SubscriptionDates")]
        [Authorize]
        public async Task<ActionResult<SubscriptionDateRange?>> SubscriptionDates()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var stac = Request.Cookies["stac"];
            if (tokenHandler.CanReadToken(stac))
            {
                var token = tokenHandler.ReadJwtToken(stac);
                string userId = null;

                foreach (var claim in token.Claims)
                {
                    if (claim.Type.Equals("nameid"))
                    {
                        userId = claim.Value;
                        break;
                    }
                }

                if (userId != null)
                {
                    var res = await _usersSrv.GetSubscriptionDates(userId);
                    return res;
                }
            }
            return null;
        }

        [HttpPost("OrderCancellation")]
        [Authorize]
        public async Task<ActionResult<bool>> OrderCancellation()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var stac = Request.Cookies["stac"];
            if (tokenHandler.CanReadToken(stac))
            {
                var token = tokenHandler.ReadJwtToken(stac);
                string userId = null;

                foreach (var claim in token.Claims)
                {
                    if (claim.Type.Equals("nameid"))
                    {
                        userId = claim.Value;
                        break;
                    }
                }

                if (userId != null)
                {
                    var res = await _usersSrv.DeleteBackgroundJobForUser(userId);

                    if (res) _logger.LogInformation($"Background job for user {userId} deleted successfully");
                    else _logger.LogError($"Error occured when deleting job for user {userId}");

                    var refundRes = await _checkoutSrv.RefundCanceledOrder(userId);

                    var datesRes = await _usersSrv.ResetSubscriptionDates(userId);


                    if (refundRes) _logger.LogInformation($"Full refund for user {userId} created successfully");
                    else _logger.LogError($"Error occured when initiating full refund for user {userId}");
                    if (res && refundRes) return true;
                }
            }
            return false;
        }

        [HttpPost("SubscriptionCancellation")]
        [Authorize(Roles="subscriber,Subscriber,SUBSCRIBER")]
        public async Task<IActionResult> SubscriptionCancellation()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var stac = Request.Cookies["stac"];
            if (tokenHandler.CanReadToken(stac))
            {
                var token = tokenHandler.ReadJwtToken(stac);
                string userId = null;

                foreach (var claim in token.Claims)
                {
                    if (claim.Type.Equals("nameid"))
                    {
                        userId = claim.Value;
                        break;
                    }
                }

                if (userId != null)
                {
                    _logger.LogInformation($"Subscription cancelation for user {userId} successfully");

                    try
                    {
                        /* HACK: check following conditions:
                        if SubscriptionStartDateUTC Lower than DateTime.UTCNow 
                        and SubscriptionEndDateUTC higher than DateTime.UTCNow
                        and Customer has active subscription on the Stripe's API side
                        If A-C will be met, then perform normally and return OK
                        If no, then return Forbidden 403 */
                        var user = await _usersSrv.GetUserById(userId);
                        if (user == null) return BadRequest();
                        var datesActive = user.SubscriptionStartDateUTC < DateTime.UtcNow && user.SubscriptionEndDateUTC < DateTime.UtcNow;
                        var userHasSubscription = await _checkoutSrv.HasUserActiveSubscription(user.StripeCustomerId);
                        // HACK TODO
                        await _checkoutSrv.CancelSubscription(userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occured when cancelling subscription for user: {userId}: {ex.Message}, {ex.StackTrace}, {ex.InnerException}, {ex.HResult}");
                        return BadRequest();
                    }

                    return Ok();
                }
            }
            return BadRequest();

        }
    }
}
