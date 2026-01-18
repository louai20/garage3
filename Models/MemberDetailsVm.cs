namespace garage3.Models
{
    public class MemberDetailsVm
    {
        public string UserId { get; set; }
        public string PersonalNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public string Role { get; set; }
        public string MembershipType { get; set; }
        public DateTime? MembershipValidUntil { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<VehicleVm> Vehicles { get; set; }
    }

    public class VehicleVm
    {
        public string RegistrationNumber { get; set; }
        public string VehicleType { get; set; }
        public bool IsParked { get; set; }
    }
}