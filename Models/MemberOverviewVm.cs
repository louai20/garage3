namespace garage3.Models
{
    public class MemberOverviewVm
    {
        public string UserId { get; set; }
        public string PersonalNumber { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string MembershipType { get; set; }

        public int VehicleCount { get; set; }
        public decimal ActiveParkingCost { get; set; }
    }
}
