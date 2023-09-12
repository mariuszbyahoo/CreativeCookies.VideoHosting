using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.Extensions
{
    public static class ControllerBaseExtensions
    {
            public static ActionResult SeeOther(this ControllerBase controller, Uri location)
            {
                controller.Response.Headers.Add("Location", location.ToString());
                return new StatusCodeResult(303);
            }
    }
}