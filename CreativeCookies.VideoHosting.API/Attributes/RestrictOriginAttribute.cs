using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Attributes
{
    public class RestrictOriginAttribute : ActionFilterAttribute
    {
        private readonly string _allowedOrigin;

        public RestrictOriginAttribute(string allowedOrigin)
        {
            _allowedOrigin = allowedOrigin;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var originHeader = context.HttpContext.Request.Headers["Origin"].ToString();

            if (!string.Equals(originHeader, _allowedOrigin, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedResult(); // or use ForbidResult() based on your need
                return;
            }

            base.OnActionExecuting(context);
        }
    }

}
