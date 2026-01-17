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
        public string SizeName => Size switch
        {
            1 => "Small",
            2 => "Small",
            3 => "Medium",
            4 => "Medium",
            5 => "Medium",
            6 => "Large",
            7 => "Large",
            8 => "Large",
            9 => "Large",
            10 => "Large",
            11 => "Small",
            12 => "Small",
            13 => "Medium",
            14 => "Medium",
            15 => "Medium",
            16 => "Large",
            17 => "Large",
            18 => "Large",
            19 => "Large",
            20 => "Large",
            _ => "Unknown"
        };

        [Display(Name = "Vehicle Type")]
        public string? VehicleTypeName { get; set; }

        [Display(Name = "Vehicle Types")]
        public string VehicleTypesAllowed => SpotNumber switch
        {
            1 => "Motorcycle",
            2 => "Car, Motorcycle",
            3 => "Car",
            4 => "Car",
            5 => "Car",
            6 => "Truck",
            7 => "Bus, Truck",
            8 => "Bus, Truck",
            9 => "Truck",
            10 => "Bus, Truck",
            11 => "Motorcycle",
            12 => "Car, Motorcycle",
            13 => "Car",
            14 => "Car",
            15 => "Car",
            16 => "Truck",
            17 => "Bus, Truck",
            18 => "Bus, Truck",
            19 => "Truck",
            20 => "Bus, Truck",
            _ => "Unknown"
        };

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
    }
}
