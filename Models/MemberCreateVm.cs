using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class MemberCreateVm
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PersonalNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "Member";

        public string MembershipType { get; set; } = "Standard";
    }

}