using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class ParkingSpotSearchViewModel
    {
        [Display(Name = "Size")]
        public string? Size { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Vehicle Type")]
        public int? VehicleTypeId { get; set; }

        [Display(Name = "Available Parking Spots")]
        public List<ParkingSpotViewModel> AvailableSpots { get; set; } = new List<ParkingSpotViewModel>();

        [Display(Name = "Total Parking Spots")]
        public int TotalSpots { get; set; }

        [Display(Name = "Available Spots")]
        public int AvailableCount { get; set; }
    }

    public class ParkingSpotViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Spot Number")]
        public int SpotNumber { get; set; }

        [Display(Name = "Size")]
        public int Size { get; set; }

        [Display(Name = "Size Name")]
        public string SizeName => $"Size {Size}";

        [Display(Name = "Vehicle Type")]
        public string? VehicleTypeName { get; set; }

        [Display(Name = "Vehicle Types")]
        public string VehicleTypesAllowed { get; set; } = string.Empty;

        [Display(Name = "Admin Reserved")]
        public bool IsAdminReserved { get; set; }

        [Display(Name = "Reserved Reason")]
        public string? ReservedReason { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Status")]
        public string Status 
        {
            get
            {
                if (IsAdminReserved)
                    return "Admin Reserved";
                return IsAvailable ? "Available" : "Occupied";
            }
        }

        // Vehicle details for occupied spots
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "Manufacturer")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Check-in Time")]
        public DateTime? CheckInTime { get; set; }

        [Display(Name = "User ID")]
        public string? UserId { get; set; }

        [Display(Name = "Spot Size")]
        public string SpotSizeName => $"Size {Size}";

        public int SpotSize => Size;
    }
}
