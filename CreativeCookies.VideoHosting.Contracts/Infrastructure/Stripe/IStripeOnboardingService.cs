using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.DTOs.Stripe;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public interface IStripeOnboardingService
    {
        /// <summary>
        /// Gets an account's status from Stripe API 
        /// </summary>
        /// <param name="idStoredInDatabase">ID of an account to return it's status</param>
        /// <returns>
        /// object with Success = true and an ErrorMessage = StripeException.message if none found, 
        /// or object with Success = true and Data = StripeConnectAccountStatus if an account found
        /// </returns>
        public StripeResultDto<StripeConnectAccountStatus> GetAccountStatus(string idStoredInDatabase);

        /// <summary>
        /// Generates an Onboarding link for a new connect account and returns an IStripeResult with IAccountCreationResult
        /// </summary>
        /// <returns>If no exceptions occur = IStripeResult with Success = true, a new account's Id in Data.AccountOnboardingUrl and Success = true
        /// otherwise, retrns IStripeResult with Success = false and an exception's message in ErrorMessage field. </returns>
        public StripeResultDto<AccountCreationResultDto> GenerateConnectAccountLink();

        /// <summary>
        /// Generates an Onboarding link for an existing connect account and returns an IStripeResult with an IAccountCreationResult
        /// </summary>
        /// <param name="existingAccountId">An existing connect account's Id</param>
        /// <returns>If no exceptions occur = IStripeResult with Success = true, a new account's Id in Data.AccountOnboardingUrl and Success = true
        /// otherwise, retrns IStripeResult with Success = false and an exception's message in ErrorMessage field. </returns>
        public StripeResultDto<AccountCreationResultDto> GenerateConnectAccountLink(string existingAccountId);

    }
}
