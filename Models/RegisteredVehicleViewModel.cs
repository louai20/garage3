using System.ComponentModel.DataAnnotations;

namespace garage3.Models
{
    public class RegisteredVehicleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        public int VehicleTypeId { get; set; }

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        [Display(Name = "Parked")]
        public bool IsParked { get; set; }

        [Display(Name = "Spot Number")]
        public int? SpotNumber { get; set; }

        [Display(Name = "Spot Size")]
        public int? SpotSize { get; set; }

        [Display(Name = "Check In Time")]
        public DateTime? CheckInTime { get; set; }

        public string SpotSizeName
        {
            get
            {
                return SpotSize switch
                {
                    1 => "Small",
                    2 => "Medium",
                    3 => "Large",
                    _ => "Unknown"
                };
            }
        }
    }
}
