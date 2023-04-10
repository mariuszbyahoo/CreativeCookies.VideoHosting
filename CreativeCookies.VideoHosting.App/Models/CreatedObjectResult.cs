using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.App.Models
{
    public class CreatedObjectResult: ObjectResult
    {
        public CreatedObjectResult(object value) : base(value)
        {
            StatusCode = 201;
        }
    }
}
