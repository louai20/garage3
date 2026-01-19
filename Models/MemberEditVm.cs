namespace garage3.Models
{
    public class MemberEditVm
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }

        public DateTime? MembershipValidUntil { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

}