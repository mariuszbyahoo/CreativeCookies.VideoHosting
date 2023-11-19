using System.ComponentModel.DataAnnotations;

namespace CreativeCookies.VideoHosting.API.Attributes
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool boolValue && boolValue;
        }
    }
}
