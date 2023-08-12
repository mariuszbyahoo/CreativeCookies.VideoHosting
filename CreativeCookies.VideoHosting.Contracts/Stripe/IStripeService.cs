﻿using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Stripe
{
    public interface IStripeService
    {
        /// <summary>
        /// Gets an account's status from Stripe API 
        /// </summary>
        /// <param name="idStoredInDatabase">ID of an account to return it's status</param>
        /// <returns>
        /// object with Success = false and an ErrorMessage = StripeException.message if none found, 
        /// or object with Success = true and Data = StripeConnectAccountStatus if an account found
        /// </returns>
        public IStripeResult<StripeConnectAccountStatus> GetAccountStatus(string idStoredInDatabase);

        /// <summary>
        /// Generates an Onboarding link for a new connect account and returns an IStripeResult with IAccountCreationResult
        /// </summary>
        /// <returns>If no exceptions occur = IStripeResult with Success = true, a new account's Id in Data.AccountOnboardingUrl and Success = true
        /// otherwise, retrns IStripeResult with Success = false and an exception's message in ErrorMessage field. </returns>
        public IStripeResult<IAccountCreationResult> GenerateConnectAccountLink();
    }
}
