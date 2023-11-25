using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs
{
    public class AddressDto
    {
        [RegularExpression(@"^[A-Za-z\sążźśćęłóńĄŻŹŚĆĘŁÓŃ]{3,}$", ErrorMessage = "First name must be at least 3 characters long and can only contain letters and spaces.")]
        public string FirstName { get; set; }

        [RegularExpression(@"^[A-Za-z\sążźśćęłóńĄŻŹŚĆĘŁÓŃ]{3,}$", ErrorMessage = "Last name must be at least 3 characters long and can only contain letters and spaces.")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Street name must be at least 3 characters long.")]
        public string Street { get; set; }

        [RegularExpression(@"^[0-9]+[A-Za-z]?\/?[0-9]*[A-Za-z]?$", ErrorMessage = "Invalid house number.")]
        public string HouseNo { get; set; }

        [RegularExpression(@"^(?!0+$)\d+$", ErrorMessage = "Invalid apartment number.")]
        public int? AppartmentNo { get; set; }

        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Postal code must be in the format XX-XXX.")]
        public string PostCode { get; set; }

        [RegularExpression(@"^[A-Za-z\sążźśćęłóńĄŻŹŚĆĘŁÓŃ]{3,}$", ErrorMessage = "City must be at least 3 characters long and can only contain letters and spaces.")]
        public string City { get; set; }
        public string Country { get; set; }
    }
}
